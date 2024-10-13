using System.Linq;
using UnityEngine;

namespace T2D.Modules
{
    /// <summary>
    /// <see cref="Terrain2DModule"/> component that generates <see cref="UnityEngine.PolygonCollider2D"/> points
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PolygonCollider2D))]
    [Terrain2DModuleInfo("Polygon Collider")]
    [AddComponentMenu("2D Terrain/Polygon Collider (Module)")]
    public class Terrain2DPolygonCollider : Terrain2DModule
    {
        public PolygonCollider2D PolygonCollider2D
        {
            get
            {
                if (_polygonCollider2D == null && (_polygonCollider2D = GetComponent<PolygonCollider2D>()) == null)
                    _polygonCollider2D = gameObject.AddComponent<PolygonCollider2D>();
                return _polygonCollider2D;
            }
        }
        [HideInInspector] private PolygonCollider2D _polygonCollider2D;


        protected override void OnBuildPerformed(Terrain2D.BuildData buildData)
        {
            Vector2[] bottomCorners =
            {
                new Vector2(buildData.MeshBounds.max.x, 0),
                Vector2.zero,
            };
            PolygonCollider2D.points = buildData.PathPoints.Select(p => new Vector2(p.x, p.y)).Union(bottomCorners).ToArray();
        }
    }
}