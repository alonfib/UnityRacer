using Unity.Collections;
using UnityEngine;

namespace T2D.Modules
{
    /// <summary>
    /// Base class for all 2D Terrain modules
    /// </summary>
    public abstract class Terrain2DModule : MonoBehaviour
    {
        public Terrain2D Parent
        {
            get => _parent;
            set => SetParent(value);
        }
        [SerializeField, HideInInspector] private Terrain2D _parent;

        private Terrain2D _lastRegisteredParent;


        protected virtual void Awake()
        {
            RegisterCallbacks(Parent);
        }

        protected virtual void OnEnable()
        {
            RegisterCallbacks(Parent);
        }

        protected virtual void OnDisable()
        {
            UnRegisterCallbacks(Parent);
        }

        protected virtual void OnDestroy()
        {
            UnRegisterCallbacks(Parent);
        }

        /// <summary>
        /// Called whenever <see cref="Terrain2D.BuildDataReady"/> of <see cref="Parent"/> has been invoked
        /// </summary>
        protected virtual void OnBuildDataReady(Terrain2D.BuildData buildData) { }

        /// <summary>
        /// Called whenever <see cref="Terrain2D.BuildPerformed"/> of <see cref="Parent"/> has been invoked
        /// </summary>
        protected virtual void OnBuildPerformed(Terrain2D.BuildData buildData) { }


        internal void SetParent(Terrain2D parent)
        {
            var oldParent = _parent;
            _parent = parent;

            if (oldParent != null)
                UnRegisterCallbacks(oldParent);

            if (oldParent != parent && parent != null)
                RegisterCallbacks(parent);
        }

        protected void RegisterCallbacks(Terrain2D parent)
        {
            if (parent == null || parent == _lastRegisteredParent)
                return;

            parent.BuildDataReady += OnBuildDataReady;
            parent.BuildPerformed += OnBuildPerformed;

            _lastRegisteredParent = parent;
        }

        protected void UnRegisterCallbacks(Terrain2D parent)
        {
            if (parent == null || _lastRegisteredParent == null)
                return;

            parent.BuildDataReady -= OnBuildDataReady;
            parent.BuildPerformed -= OnBuildPerformed;

            _lastRegisteredParent = null;
        }

        /// <summary>
        /// Manually build module based on <see cref="Terrain2D.BuildData"/> of <see cref="Parent"/>
        /// </summary>
        public virtual void Build()
        {
            if (Parent != null)
            {
                var buildData = Parent.GetBuildData(Allocator.TempJob);
                OnBuildPerformed(buildData);
                buildData.Dispose();
            }
        }
    }
}