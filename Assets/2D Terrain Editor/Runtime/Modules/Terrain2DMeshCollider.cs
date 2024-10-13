using System.Runtime.InteropServices;
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
    /// <see cref="Terrain2DModule"/> component that generates 3D mesh for <see cref="MeshCollider"/>
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MeshCollider))]
    [Terrain2DModuleInfo("3D Mesh Collider", true)]
    [AddComponentMenu("2D Terrain/3D Mesh Collider (Module)")]
    public class Terrain2DMeshCollider : Terrain2DModule
    {
        public MeshCollider MeshCollider
        {
            get
            {
                if (_meshCollider == null && (_meshCollider = GetComponent<MeshCollider>()) == null)
                    _meshCollider = gameObject.AddComponent<MeshCollider>();
                return _meshCollider;
            }
        }
        [HideInInspector] private MeshCollider _meshCollider;

        /// <summary>
        /// Z-axis mesh size in units
        /// </summary>
        public float Size
        {
            get => _size;
            set => _size = value;
        }
        [Tooltip("Z-axis mesh size in units")]
        [SerializeField] private float _size = 1;


        protected override void OnDestroy()
        {
            if (MeshCollider.sharedMesh != null && MeshCollider.sharedMesh.name == gameObject.GetTempMeshName<Terrain2DMeshCollider>())
                DestroyImmediate(MeshCollider.sharedMesh);

            base.OnDestroy();
        }

        protected override void OnBuildPerformed(Terrain2D.BuildData buildData)
        {
            Mesh mesh = MeshCollider.sharedMesh;
            if (mesh == null || mesh.name != gameObject.GetTempMeshName<Terrain2DMeshCollider>())
            {
                MeshCollider.sharedMesh = mesh = new Mesh
                {
                    hideFlags = HideFlags.HideAndDontSave,
                    name = gameObject.GetTempMeshName<Terrain2DMeshCollider>()
                };
            }

            var vertexCount = buildData.PathPoints.Length * 2;
            var vertexBuffer = new NativeArray<Vertex3D>(vertexCount, Allocator.TempJob);

            var indexCount = (vertexCount - 2) * 3;
            var indexBuffer = new NativeArray<uint>(indexCount, Allocator.TempJob);

            var fillVerticesJobHandle = new FillVerticesJob(buildData.PathPoints, Size, vertexBuffer).Schedule(vertexCount, 1);
            var fillIndicesJobHandle = new Terrain2D.FillIndicesJob(vertexCount, indexBuffer).Schedule(indexCount, 1);

            JobHandle.CompleteAll(ref fillVerticesJobHandle, ref fillIndicesJobHandle);

            mesh.SetVertexBufferParams(vertexCount, Vertex3D.Descriptor);
            mesh.SetVertexBufferData(vertexBuffer, 0, 0, vertexCount);

            mesh.SetIndexBufferParams(indexCount, IndexFormat.UInt32);
            mesh.SetIndexBufferData(indexBuffer, 0, 0, indexBuffer.Length);

            mesh.SetSubMesh(0, new SubMeshDescriptor(0, indexCount),
                MeshUpdateFlags.DontResetBoneBounds | MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontRecalculateBounds);

            Vector3 boundsOffset = new Vector3(0, 0, Size);
            var bounds = buildData.MeshBounds;
            bounds.min -= boundsOffset;
            bounds.max += boundsOffset;
            mesh.bounds = bounds;
            
            vertexBuffer.Dispose();
            indexBuffer.Dispose();

            if (gameObject.activeSelf)
            {
                gameObject.SetActive(false);
                gameObject.SetActive(true);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Vertex3D
        {
            public static readonly VertexAttributeDescriptor[] Descriptor =
            {
                new VertexAttributeDescriptor(VertexAttribute.Position)
            };

            public Vector3 Pos;
        }

#if UNITY_BURST
        [BurstCompile]
#endif
        public struct FillVerticesJob : IJobParallelFor
        {
            [ReadOnly] private readonly NativeArray<Vector3> _pathPoints;
            [ReadOnly] private readonly float _size;

            private NativeArray<Vertex3D> _vertexBuffer;


            public FillVerticesJob(NativeArray<Vector3> pathPoints, float size, NativeArray<Vertex3D> vertexBuffer)
            {
                _pathPoints = pathPoints;
                _size = size;

                _vertexBuffer = vertexBuffer;
            }

            public void Execute(int index)
            {
                Vertex3D vertex = _vertexBuffer[index];

                int pointId = index >> 1;
                vertex.Pos = _pathPoints[pointId];
                vertex.Pos.z = index % 2 == 0 ? _size : -_size;

                _vertexBuffer[index] = vertex;
            }
        }
    }
}