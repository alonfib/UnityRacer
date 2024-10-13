using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;

#if UNITY_BURST
using Unity.Burst;
#endif

namespace T2D.Modules
{
    /// <summary>
    /// <see cref="Terrain2DModule"/> component that generates Cap mesh
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [Terrain2DModuleInfo("Cap", true)]
    [AddComponentMenu("2D Terrain/Cap (Module)")]
    public class Terrain2DCap : Terrain2DModule
    {
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
        /// Cap mesh height
        /// </summary>
        public float Size
        {
            get => _size;
            set => _size = value;
        }
        [Tooltip("Cap mesh height")]
        [SerializeField] private float _size = 0.5f;

        /// <summary>
        /// Cap mesh vertical offset
        /// </summary>
        public float Offset
        {
            get => _offset;
            set => _offset = value;
        }
        [Tooltip("Cap mesh vertical offset")]
        [SerializeField] private float _offset;

        /// <summary>
        /// Horizontal mesh UV0 scale
        /// </summary>
        public float UvScale
        {
            get => _uvScale;
            set => _uvScale = value;
        }
        [Tooltip("Horizontal mesh UV0 scale")]
        [SerializeField] private float _uvScale = 1;

        /// <summary>
        /// Horizontal mesh UV offset
        /// </summary>
        public float UvOffset
        {
            get => _uvOffset;
            set => _uvOffset = value;
        }
        [Tooltip("Horizontal mesh UV offset")]
        [SerializeField] private float _uvOffset;

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
        [SerializeField] private SortingLayer _sortingLayer = new SortingLayer(0, 1);


        protected override void Awake()
        {
            base.Awake();

            SortingLayer.SetToRenderer(MeshRenderer);
        }

        protected override void OnDestroy()
        {
            if (MeshFilter.sharedMesh != null && MeshFilter.sharedMesh.name == gameObject.GetTempMeshName<Terrain2DCap>())
                DestroyImmediate(MeshFilter.sharedMesh);
            
            base.OnDestroy();
        }

        protected override void OnBuildPerformed(Terrain2D.BuildData buildData)
        {
            if (Parent == null)
                return;

            if (GetComponent<Terrain2D>() == Parent)
            {
                Debug.LogWarning($"Building {nameof(Terrain2DCap)} mesh failed. {nameof(Terrain2DCap)} component should be attached to separated {nameof(GameObject)}");
                return;
            }

            Mesh mesh = MeshFilter.sharedMesh;
            if (mesh == null || mesh.name != gameObject.GetTempMeshName<Terrain2D>())
            {
                MeshFilter.sharedMesh = mesh = new Mesh
                {
                    hideFlags = HideFlags.HideAndDontSave,
                    name = gameObject.GetTempMeshName<Terrain2D>()
                };
            }

            var vertexCount = buildData.PathPoints.Length * 2;
            var vertexBuffer = new NativeArray<Terrain2D.Vertex>(vertexCount, Allocator.TempJob);

            var indexCount = (vertexCount - 2) * 3;
            var indexBuffer = new NativeArray<uint>(indexCount, Allocator.TempJob);

            var fillVerticesJobHandle = new FillVerticesJob(buildData.PathPoints, buildData.PathPointColors, Size, Offset, UvScale, UvOffset, vertexBuffer).Schedule(vertexCount, 1);
            var fillIndicesJobHandle = new Terrain2D.FillIndicesJob(vertexCount, indexBuffer).Schedule(indexCount, 1);

            JobHandle.CompleteAll(ref fillVerticesJobHandle, ref fillIndicesJobHandle);

            mesh.SetVertexBufferParams(vertexCount, Terrain2D.Vertex.Descriptor);
            mesh.SetVertexBufferData(vertexBuffer, 0, 0, vertexCount);

            mesh.SetIndexBufferParams(indexCount, IndexFormat.UInt32);
            mesh.SetIndexBufferData(indexBuffer, 0, 0, indexBuffer.Length);

            mesh.SetSubMesh(0, new SubMeshDescriptor(0, indexCount),
                MeshUpdateFlags.DontResetBoneBounds | MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontRecalculateBounds);

            Vector3 boundsOffset = new Vector2(Size, Size);
            var bounds = buildData.MeshBounds;
            bounds.min -= boundsOffset;
            bounds.max += boundsOffset;
            mesh.bounds = bounds;

            vertexBuffer.Dispose();
            indexBuffer.Dispose();
        }


        public override void Build()
        {
            base.Build();

            SortingLayer.SetToRenderer(MeshRenderer);
        }

#if UNITY_BURST
        [BurstCompile]
#endif
        public struct FillVerticesJob : IJobParallelFor
        {
            [ReadOnly] private readonly NativeArray<Vector3> _pathPoints;
            [ReadOnly] private readonly NativeArray<Color32> _pathPointColors;
            [ReadOnly] private readonly float _size;
            [ReadOnly] private readonly float _offset;
            [ReadOnly] private readonly float _uvScale;
            [ReadOnly] private readonly float _uvOffset;

            private readonly Quaternion _dirToNormalRotation;

            private NativeArray<Terrain2D.Vertex> _vertexBuffer;


            public FillVerticesJob(NativeArray<Vector3> pathPoints, NativeArray<Color32> pathPointColors, float size, float offset, float uvScale, float uvOffset, NativeArray<Terrain2D.Vertex> vertexBuffer)
            {
                _pathPoints = pathPoints;
                _pathPointColors = pathPointColors;
                _size = size;
                _offset = offset;
                _uvScale = uvScale;
                _uvOffset = uvOffset;

                _vertexBuffer = vertexBuffer;

                _dirToNormalRotation = Quaternion.Euler(0, 0, -90);
            }
            
            public void Execute(int index)
            {
                Terrain2D.Vertex vertex = _vertexBuffer[index];

                int pointId = index >> 1;
                float offsetMul = (index % 2 == 0 ? _size : -_size) + _offset;

                Vector2 pointPos = _pathPoints[pointId];
                Vector2 lp = _pathPoints[pointId <= 0 ? pointId : pointId - 1];
                Vector2 rp = _pathPoints[pointId >= _pathPoints.Length - 1 ? pointId : pointId + 1];
                Vector2 up = _dirToNormalRotation * (lp - rp).normalized;

                vertex.Pos = pointPos + up * offsetMul;
                vertex.Pos.z = 0;
                
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

                vertex.Uv0.x = _uvOffset + (_pathPoints[pointId].z * _uvScale);
                vertex.Uv0.y = 1 - index % 2;

                _vertexBuffer[index] = vertex;
            }
        }
    }
}