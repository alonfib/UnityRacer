using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;
using UnityEngine.Rendering;
using System.Linq;
using System.Reflection;
using T2D.Modules;
using Unity.Jobs;
using UnityEngine;

#if UNITY_BURST
using Unity.Burst;
#endif

namespace T2D
{
    /// <summary>
    /// Main component that generates 2D Terrain base mesh
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [AddComponentMenu("2D Terrain/2D Terrain", -int.MaxValue)]
    [SelectionBase]
    public class Terrain2D : MonoBehaviour
    {
        protected const float MIN_MESH_RESOLUTION = 0.1f;
        protected const float MAX_MESH_RESOLUTION = 32f;

        private static readonly Dictionary<Type, Terrain2DModuleInfoAttribute> _knownModules = new Dictionary<Type, Terrain2DModuleInfoAttribute>();
        

        /// <summary>
        /// An event that is called right before generating base 2D Terrain mesh
        /// </summary>
        public event Action<BuildData> BuildDataReady;

        /// <summary>
        /// An event that is called right after generating base 2D Terrain mesh
        /// </summary>
        public event Action<BuildData> BuildPerformed;


        public MeshFilter MeshFilter
        {
            get
            {
                if (_meshFilter == null && (_meshFilter = GetComponent<MeshFilter>()) == null)
                    _meshFilter = gameObject.AddComponent<MeshFilter>();
                return _meshFilter;
            }
        }
        [HideInInspector] private MeshFilter _meshFilter;

        public MeshRenderer MeshRenderer
        {
            get
            {
                if (_meshRenderer == null && (_meshRenderer = GetComponent<MeshRenderer>()) == null)
                    _meshRenderer = gameObject.AddComponent<MeshRenderer>();
                return _meshRenderer;
            }
        }
        [HideInInspector] private MeshRenderer _meshRenderer;


        /// <summary>
        /// Total length of 2D Terrain path. This is the sum of distances between all Path Points
        /// </summary>
        public float PathLength { get; private set; }


        /// <summary>
        /// An <see cref="AnimationCurve"/> that is used to generate actual Path Points
        /// </summary>
        public AnimationCurve Path
        {
            get => _path;
            set => _path = value ?? (_path = new AnimationCurve(new Keyframe(0, 1), new Keyframe(5, 1)));
        }
        [SerializeField] private AnimationCurve _path = new AnimationCurve(new Keyframe(0, 1), new Keyframe(5, 1));

        /// <summary>
        /// Resulting base mesh resolution defined by Path Points count per unit
        /// </summary>
        public float MeshResolution
        {
            get => Mathf.Clamp(_meshResolution, MIN_MESH_RESOLUTION, MAX_MESH_RESOLUTION);
            set => _meshResolution = value;
        }
        [SerializeField, Range(MIN_MESH_RESOLUTION, MAX_MESH_RESOLUTION)]
        [Tooltip("Resulting base mesh resolution defined by Path Points count per unit")]
        private float _meshResolution = 2;
        
        /// <summary>
        /// Resulting base mesh UV0 scale
        /// </summary>
        public float TextureSize
        {
            get => _textureSize;
            set => _textureSize = value;
        }
        [Tooltip("Base mesh UV0 scale")]
        [SerializeField] private float _textureSize = 1;

        /// <summary>
        /// Resulting base mesh vertex colors
        /// </summary>
        public List<VertexColorKeyPoint> VertexColorKeys
        {
            get => _vertexColorKeys;
            set
            {
                if (value == null)
                    _vertexColorKeys.Clear();
                else _vertexColorKeys = value;
            }
        }
        [SerializeField] private List<VertexColorKeyPoint> _vertexColorKeys = new List<VertexColorKeyPoint>();

        /// <summary>
        /// Resulting base mesh vertex colors gradient mode
        /// </summary>
        public GradientMode VertexColorMode
        {
            get => _vertexColorMode;
            set => _vertexColorMode = value;
        }
        [Tooltip("Resulting base mesh vertex colors gradient mode")]
        [SerializeField] private GradientMode _vertexColorMode = GradientMode.Blend;

        /// <summary>
        /// Sorting layer and order of <see cref="MeshRenderer"/>
        /// </summary>
        public SortingLayer SortingLayer
        {
            get => _sortingLayer;
            set
            {
                _sortingLayer = value;
                value.SetToRenderer(MeshRenderer);
            }
        }
        [Tooltip("Sorting layer and order of MeshRenderer")]
        [SerializeField] private SortingLayer _sortingLayer;


        protected virtual void Start()
        {
            Build();
            SortingLayer.SetToRenderer(MeshRenderer);
        }
        
        protected virtual void OnDestroy()
        {
            if (MeshFilter.sharedMesh != null && MeshFilter.sharedMesh.name == gameObject.GetTempMeshName<Terrain2D>())
                DestroyImmediate(MeshFilter.sharedMesh);
        }
        
        
        protected NativeArray<Vector3> GetPathPoints(Allocator allocator, out float maxHeight)
        {
            maxHeight = -float.MaxValue;
            if (Path.length <= 1)
            {
                maxHeight = 1;
                return new NativeArray<Vector3>(new[] { Vector3.up, Vector3.one }, allocator);
            }

            int pathPointsCount = GetPathPointsCount();
            float startTime = Path.keys.First().time;
            float totalTime = Path.GetTotalTime();
            float step = totalTime / (pathPointsCount - 1);

            NativeArray<Vector3> points = new NativeArray<Vector3>(pathPointsCount, allocator, NativeArrayOptions.UninitializedMemory);

            float t = startTime;
            float val = Mathf.Clamp(Path.Evaluate(t), 0, float.MaxValue);
            Vector2 lastPointPos = new Vector2(t, val);
            float dist = 0;
            for (int i = 0; i < points.Length; i++)
            {
                t = startTime + step * i;
                val = Mathf.Clamp(Path.Evaluate(t), 0, float.MaxValue);

                Vector3 pos = new Vector3(t, val);
                pos.z = (dist += Vector2.Distance(lastPointPos, pos));
                maxHeight = Mathf.Max(maxHeight, pos.y);

                points[i] = pos;

                lastPointPos = pos;
            }

            return points;
        }

        protected NativeArray<Color32> GetPathPointColors(NativeArray<Vector3> pathPoints, Allocator allocator)
        {
            switch (VertexColorKeys.Count)
            {
                case 0:
                    return new NativeArray<Color32>(new Color32[] { Color.white }, allocator);
                case 1:
                    return new NativeArray<Color32>(new[] { VertexColorKeys[0].Color }, allocator);
            }

            var minPos = pathPoints[0].x;
            var maxPos = pathPoints[pathPoints.Length - 1].x;

            List<VertexColorKeyPoint> orderedKeys = VertexColorKeys.OrderBy(key => key.Position).ToList();

            if (orderedKeys[0].Position >= minPos)
                orderedKeys.Insert(0, new VertexColorKeyPoint(minPos, orderedKeys[0].Color));
            else
            {
                VertexColorKeyPoint lastKeyPoint = orderedKeys[0];
                while (orderedKeys.Count > 0 && orderedKeys[0].Position <= minPos)
                {
                    lastKeyPoint = orderedKeys[0];
                    orderedKeys.RemoveAt(0);
                }

                lastKeyPoint.Position = minPos;
                orderedKeys.Insert(0, lastKeyPoint);
            }

            if (orderedKeys.Last().Position <= maxPos)
                orderedKeys.Add(new VertexColorKeyPoint(maxPos, orderedKeys.Last().Color));
            else
            {
                VertexColorKeyPoint lastKeyPoint = orderedKeys.Last();
                while (orderedKeys.Count > 0 && orderedKeys.Last().Position >= maxPos)
                {
                    lastKeyPoint = orderedKeys.Last();
                    orderedKeys.RemoveAt(orderedKeys.Count - 1);
                }

                lastKeyPoint.Position = maxPos;
                orderedKeys.Add(lastKeyPoint);
            }

            Gradient gradient = new Gradient { mode = VertexColorMode };
            GradientColorKey[] gColorKeys = new GradientColorKey[orderedKeys.Count];
            GradientAlphaKey[] gAlphaKeys = new GradientAlphaKey[orderedKeys.Count];
            for (int i = 0; i < orderedKeys.Count; i++)
            {
                float t = orderedKeys[i].Position.Remap(minPos, maxPos, 0, 1);
                gColorKeys[i] = new GradientColorKey(orderedKeys[i].Color, t);
                gAlphaKeys[i] = new GradientAlphaKey(orderedKeys[i].Color.a / 255f, t);
            }
            gradient.SetKeys(gColorKeys, gAlphaKeys);

            NativeArray<Color32> pathPointColors = new NativeArray<Color32>(pathPoints.Length, allocator, NativeArrayOptions.UninitializedMemory);
            for (int i = 0; i < pathPointColors.Length; i++)
                pathPointColors[i] = gradient.Evaluate((float)i / (pathPointColors.Length - 1));

            return pathPointColors;
        }

        /// <summary>
        /// Returns total number of Path Points based on the <see cref="Path"/> and <see cref="MeshResolution"/>
        /// </summary>
        /// <returns></returns>
        public int GetPathPointsCount() => Mathf.Clamp((int)(Path.GetTotalTime() * MeshResolution), 1, int.MaxValue) + 1;

        /// <summary>
        /// Generates <see cref="BuildData"/> from <see cref="Path"/> and <see cref="VertexColorKeys"/>
        /// </summary>
        /// <param name="allocator"></param>
        /// <returns></returns>
        public BuildData GetBuildData(Allocator allocator)
        {
            var pathPoints = GetPathPoints(allocator, out var maxHeight);
            var pathPointColors = GetPathPointColors(pathPoints, allocator);

            var bounds = new Bounds();
            bounds.SetMinMax(new Vector3(pathPoints[0].x, 0), new Vector3(pathPoints[pathPoints.Length - 1].x, maxHeight));
            
            return new BuildData(pathPoints, pathPointColors, bounds);
        }

        /// <summary>
        /// Returns all <see cref="Terrain2DModule"/> components subscribed to <see cref="BuildDataReady"/> and <see cref="BuildPerformed"/>
        /// </summary>
        /// <returns></returns>
        public Terrain2DModule[] GetRegisteredModules()
        {
            var buildDataReadySubs =
                BuildDataReady?.GetInvocationList().Select(d => d.Target).OfType<Terrain2DModule>() ?? new Terrain2DModule[0];
            var buildPerformedSubs =
                BuildPerformed?.GetInvocationList().Select(d => d.Target).OfType<Terrain2DModule>() ?? new Terrain2DModule[0];

            return buildDataReadySubs.Union(buildPerformedSubs).Distinct().ToArray();
        }

        /// <summary>
        /// Creates new or returns existing <see cref="Terrain2DModule"/> of type <see cref="T"/>
        /// </summary>
        /// <typeparam name="T"><see cref="Terrain2DModule"/> type</typeparam>
        public T AddModule<T>() where T : Terrain2DModule
        {
            return AddModule(typeof(T)) as T;
        }

        /// <summary>
        /// Creates new or returns existing <see cref="Terrain2DModule"/> of type <see cref="moduleType"/>
        /// </summary>
        /// <param name="moduleType"><see cref="Terrain2DModule"/> type</param>
        public Terrain2DModule AddModule(Type moduleType)
        {
            if (moduleType == null || moduleType.IsAbstract || !moduleType.IsSubclassOf(typeof(Terrain2DModule)))
                return null;

            if (!_knownModules.ContainsKey(moduleType))
                _knownModules.Add(moduleType, (Terrain2DModuleInfoAttribute)moduleType.GetCustomAttribute(typeof(Terrain2DModuleInfoAttribute)) ??
                                              new Terrain2DModuleInfoAttribute(moduleType.Name, true));
            
            var moduleInfo = _knownModules[moduleType];
            if (!moduleInfo.AllowMultipleInstances && GetComponent(moduleType) != null)
                return GetComponent(moduleType) as Terrain2DModule;

            Terrain2DModule newModule;
            if (!moduleInfo.NewGameObjectRequired)
                newModule = (Terrain2DModule) gameObject.AddComponent(moduleType);
            else
            {
                GameObject moduleGo = new GameObject(moduleInfo.Name);
                moduleGo.transform.parent = transform;
                moduleGo.transform.localPosition = Vector3.zero;
                moduleGo.transform.localEulerAngles = Vector3.zero;

                newModule = (Terrain2DModule) moduleGo.AddComponent(moduleType);
            }
            newModule.Parent = this;

            return newModule;
        }

        /// <summary>
        /// Builds 2D Terrain base mesh based on <see cref="BuildData"/>
        /// </summary>
        public virtual void Build()
        {
            Mesh mesh = MeshFilter.sharedMesh;
            if (mesh == null || mesh.name != gameObject.GetTempMeshName<Terrain2D>())
            {
                mesh = new Mesh
                {
                    hideFlags = HideFlags.HideAndDontSave,
                    name = gameObject.GetTempMeshName<Terrain2D>()
                };
                MeshFilter.sharedMesh = mesh;
            }
            
            var buildData = GetBuildData(Allocator.TempJob);
            PathLength = buildData.PathPoints.Last().z;

            BuildDataReady?.Invoke(buildData);

            var vertexCount = buildData.PathPoints.Length * 2;
            var vertexBuffer = new NativeArray<Vertex>(vertexCount, Allocator.TempJob);

            var indexCount = (vertexCount - 2) * 3;
            var indexBuffer = new NativeArray<uint>(indexCount, Allocator.TempJob);
            
            var fillVerticesJobHandle = new FillVerticesJob(buildData.PathPoints, TextureSize, buildData.PathPointColors, vertexBuffer).Schedule(vertexCount, 1);
            var fillIndicesJobHandle = new FillIndicesJob(vertexCount, indexBuffer).Schedule(indexCount, 1);

            JobHandle.CompleteAll(ref fillVerticesJobHandle, ref fillIndicesJobHandle);

            mesh.SetVertexBufferParams(vertexCount, Vertex.Descriptor);
            mesh.SetVertexBufferData(vertexBuffer, 0, 0, vertexCount);

            mesh.SetIndexBufferParams(indexCount, IndexFormat.UInt32);
            mesh.SetIndexBufferData(indexBuffer, 0, 0, indexBuffer.Length);

            mesh.SetSubMesh(0, new SubMeshDescriptor(0, indexCount),
                MeshUpdateFlags.DontResetBoneBounds | MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontRecalculateBounds);
            
            mesh.bounds = buildData.MeshBounds;
            SortingLayer.SetToRenderer(MeshRenderer);

            BuildPerformed?.Invoke(buildData);

            vertexBuffer.Dispose();
            indexBuffer.Dispose();
            buildData.Dispose();
        }
        

        [StructLayout(LayoutKind.Sequential)]
        public struct Vertex
        {
            public static readonly VertexAttributeDescriptor[] Descriptor =
            {
                new VertexAttributeDescriptor(VertexAttribute.Position),
                new VertexAttributeDescriptor(VertexAttribute.Normal),
                new VertexAttributeDescriptor(VertexAttribute.Tangent, VertexAttributeFormat.Float32, 4), 
                new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.UNorm8, 4),
                new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2), 
            };

            public Vector3 Pos;
            public Vector3 Normal;
            public Vector4 Tangent;
            public Color32 Color;
            public Vector2 Uv0;
        }

#if UNITY_BURST
        [BurstCompile]
#endif
        public struct FillVerticesJob : IJobParallelFor
        {
            [ReadOnly] private readonly NativeArray<Vector3> _pathPoints;
            [ReadOnly] private readonly NativeArray<Color32> _pathPointColors;
            [ReadOnly] private readonly float _textureSize;

            private NativeArray<Vertex> _vertexBuffer;


            public FillVerticesJob(NativeArray<Vector3> pathPoints, float textureSize, NativeArray<Color32> pathPointColors, NativeArray<Vertex> vertexBuffer)
            {
                _pathPoints = pathPoints;
                _textureSize = textureSize;
                _pathPointColors = pathPointColors;

                _vertexBuffer = vertexBuffer;
            }

            public void Execute(int index)
            {
                Vertex vertex = _vertexBuffer[index];

                int pointId = index >> 1;
                bool isLower = index % 2 != 0;

                vertex.Pos.x = _pathPoints[pointId].x;
                vertex.Pos.y = _pathPoints[pointId].y * (isLower ? 0 : 1);

                vertex.Normal = Vector3.back;

                vertex.Tangent.x = 1;
                vertex.Tangent.w = -1;

                switch (_pathPointColors.Length)
                {
                    case 1:
                        vertex.Color = _pathPointColors[0];
                        break;
                    default:
                        vertex.Color = _pathPointColors[pointId];
                        break;
                }
                
                vertex.Uv0 = vertex.Pos * _textureSize;
                
                _vertexBuffer[index] = vertex;
            }
        }

#if UNITY_BURST
        [BurstCompile]
#endif
        public struct FillIndicesJob : IJobParallelFor
        {
            [ReadOnly] private readonly int _vertexCount;

            [WriteOnly] private NativeArray<uint> _indexBuffer;


            public FillIndicesJob(int vertexCount, NativeArray<uint> indexBuffer)
            {
                _vertexCount = vertexCount;
                _indexBuffer = indexBuffer;
            }

            public void Execute(int index)
            {
                uint startVertexId = (uint) index / 3;
                uint vertexIdOffset = (uint) index % 3;
                bool isFrontFace = startVertexId % 2 != 0;
                
                uint vertexId = startVertexId + (isFrontFace ? vertexIdOffset : 2 - vertexIdOffset);
                if (vertexId >= _vertexCount)
                    vertexId = (uint) _vertexCount - 1;

                _indexBuffer[index] = vertexId;
            }
        }

        /// <summary>
        /// Mesh vertex colors key points
        /// </summary>
        [Serializable, StructLayout(LayoutKind.Sequential)]
        public struct VertexColorKeyPoint : IEquatable<VertexColorKeyPoint>
        {
            /// <summary>
            /// Local position of key point along X axis
            /// </summary>
            public float Position;

            /// <summary>
            /// Target <see cref="Color32"/> for given <see cref="Position"/>
            /// </summary>
            public Color32 Color;


            public VertexColorKeyPoint(float position, Color32 color)
            {
                Position = position;
                Color = color;
            }


            public bool Equals(VertexColorKeyPoint other)
            {
                return Mathf.Approximately(Position, other.Position) && Color.Equals(other.Color);
            }

            public override string ToString() => $"Position: {Position} Color: {Color}";
        }

        /// <summary>
        /// 2D Terrain base mesh data
        /// </summary>
        public class BuildData : IDisposable
        {
            /// <summary>
            /// Path point positions. XY - path point position, Z - current path length for given position
            /// </summary>
            public readonly NativeArray<Vector3> PathPoints;

            /// <summary>
            /// <see cref="PathPoints"/> colors
            /// </summary>
            public readonly NativeArray<Color32> PathPointColors;

            /// <summary>
            /// Base mesh bounds
            /// </summary>
            public readonly Bounds MeshBounds;


            public BuildData(NativeArray<Vector3> pathPoints, NativeArray<Color32> pathPointColors, Bounds meshBounds)
            {
                PathPoints = pathPoints;
                PathPointColors = pathPointColors;
                MeshBounds = meshBounds;
            }

            public void Dispose()
            {
                if (PathPoints.IsCreated)
                    PathPoints.Dispose();
                if (PathPointColors.IsCreated)
                    PathPointColors.Dispose();
            }
        }
    }
}