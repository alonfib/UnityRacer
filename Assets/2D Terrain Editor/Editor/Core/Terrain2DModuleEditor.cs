using System.Linq;
using UnityEditor;

namespace T2D.Modules.Editor
{
    [CustomEditor(typeof(Terrain2DModule), true)]
    [CanEditMultipleObjects]
    public class Terrain2DModuleEditor : UnityEditor.Editor
    {
        protected SerializedObject Objects;

        void OnEnable()
        {
            Objects = new SerializedObject(targets);
            Undo.undoRedoPerformed += RebuildTargets;
        }

        void OnDisable()
        {
            Undo.undoRedoPerformed -= RebuildTargets;
        }

        void RebuildTargets()
        {
            foreach (var t in targets.OfType<Terrain2DModule>())
                t.Build();
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(Objects.FindProperty("_parent"));
            if (EditorGUI.EndChangeCheck())
                foreach (var t in targets.OfType<Terrain2DModule>())
                    t.SetParent(Objects.FindProperty("_parent").objectReferenceValue as Terrain2D);

            EditorGUILayout.Separator();

            EditorGUI.BeginChangeCheck();
            base.OnInspectorGUI();
            if (EditorGUI.EndChangeCheck())
                RebuildTargets();
        }
    }
}