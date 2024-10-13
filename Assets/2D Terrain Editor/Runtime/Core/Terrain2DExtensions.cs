using System;
using System.Linq;
using UnityEngine;

namespace T2D
{
    /// <summary>
    /// Various helper methods
    /// </summary>
    public static class Terrain2DExtensions
    {
        /// <summary>
        /// Returns total time of provided <see cref="AnimationCurve"/>
        /// </summary>
        public static float GetTotalTime(this AnimationCurve curve)
        {
            if (curve.length <= 1)
                return 0;

            return Mathf.Abs(curve.keys.Last().time - curve.keys.First().time);
        }

        /// <summary>
        /// Remaps value from <see cref="min"/> - <see cref="max"/> to <see cref="from"/> - <see cref="to"/>
        /// </summary>
        public static float Remap(this float value, float min, float max, float from, float to)
        {
            return from + (value - min) * (to - from) / (max - min);
        }

        /// <summary>
        /// Returns temporary mesh name for given <see cref="GameObject"/>
        /// </summary>
        public static string GetTempMeshName<T>(this GameObject go) where T : MonoBehaviour
        {
            return $"{typeof(T).Name}_mesh_{(go == null ? new Guid().ToString() : go.GetInstanceID().ToString())}";
        }
    }
}

