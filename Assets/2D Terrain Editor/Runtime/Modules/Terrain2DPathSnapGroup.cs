using UnityEngine;

namespace T2D.Modules
{
    /// <summary>
    /// <see cref="Terrain2DModule"/> component that snaps all the child transforms to <see cref="Terrain2D"/> path along Y-axis
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [Terrain2DModuleInfo("Path Snap Group", true)]
    [AddComponentMenu("2D Terrain/Path Snap Group (Module)")]
    public class Terrain2DPathSnapGroup : Terrain2DModule
    {
        private static readonly Quaternion DirToNormalRotation = Quaternion.Euler(0, 0, -90);

        /// <summary>
        /// If checked, all child transforms will be aligned to <see cref="Terrain2D"/> path normal
        /// </summary>
        public bool AlignToNormal
        {
            get => _alignToNormal;
            set => _alignToNormal = value;
        }
        [Tooltip("If checked, all child transforms will be aligned to Terrain2D path normal")]
        [SerializeField] private bool _alignToNormal = true;


        protected override void OnBuildPerformed(Terrain2D.BuildData buildData)
        {
            AnimationCurve tempCurve = new AnimationCurve();
            foreach (Transform t in transform)
            {
                Vector3 pos = Parent.transform.InverseTransformPoint(t.position);
                
                if (pos.x <= buildData.MeshBounds.min.x || pos.x >= buildData.MeshBounds.max.x)
                    continue;
                
                float normPos = pos.x.Remap(buildData.MeshBounds.min.x, buildData.MeshBounds.max.x, 0, 1);
                
                Vector2 lp = buildData.PathPoints[Mathf.FloorToInt(normPos * (buildData.PathPoints.Length - 1))];
                Vector2 rp = buildData.PathPoints[Mathf.CeilToInt(normPos * (buildData.PathPoints.Length - 1))];
                
                tempCurve.AddKey(lp.x, lp.y);
                tempCurve.AddKey(rp.x, rp.y);

                pos.y = tempCurve.Evaluate(pos.x);

                for (int i = 0; i < tempCurve.length; i++)
                    tempCurve.RemoveKey(0);

                t.position = Parent.transform.TransformPoint(pos);

                if (AlignToNormal)
                    t.localRotation = Quaternion.FromToRotation(Vector3.up, DirToNormalRotation * (lp - rp).normalized);
            }
        }
    }
}