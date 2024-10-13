using System.Linq;
using UnityEngine;

namespace T2D.Modules
{
    /// <summary>
    /// <see cref="Terrain2DModule"/> component that generates <see cref="UnityEngine.EdgeCollider2D"/> points
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(EdgeCollider2D))]
    [Terrain2DModuleInfo("Edge Collider")]
    [AddComponentMenu("2D Terrain/Edge Collider (Module)")]
    public class Terrain2DEdgeCollider : Terrain2DModule
    {
        public EdgeCollider2D EdgeCollider2D
        {
            get
            {
                if (_edgeCollider2D == null && (_edgeCollider2D = GetComponent<EdgeCollider2D>()) == null)
                    _edgeCollider2D = gameObject.AddComponent<EdgeCollider2D>();
                return _edgeCollider2D;
            }
        }
        [HideInInspector] private EdgeCollider2D _edgeCollider2D;


        protected override void OnBuildPerformed(Terrain2D.BuildData buildData)
        {
            EdgeCollider2D.points = buildData.PathPoints.Select(p => new Vector2(p.x, p.y)).ToArray();
        }
    }
}