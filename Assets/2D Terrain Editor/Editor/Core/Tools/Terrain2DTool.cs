using System.Linq;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;

namespace T2D.Editor
{
    [EditorTool("Edit Terrain 2D Vertex Colors", typeof(Terrain2D))]
    public abstract class Terrain2DTool : EditorTool
    {
        protected Terrain2D Target { get; private set; }
        protected bool IsEditing { get; private set; }

        protected bool ShouldRecordTarget
        {
            get => _shouldRecordTarget;
            set => _shouldRecordTarget = value || _shouldRecordTarget;
        }
        private bool _shouldRecordTarget;

        protected Rect SettingsWindowRect;


        protected virtual void OnEnable()
        {
            ToolManager.activeToolChanged += SwitchEditMode;
            Selection.selectionChanged += SwitchEditMode;
            Undo.undoRedoPerformed += SwitchEditMode;
            EditorApplication.playModeStateChanged += SwitchEditMode;
        }

        protected virtual void OnDisable()
        {
            ToolManager.activeToolChanged -= SwitchEditMode;
            Selection.selectionChanged -= SwitchEditMode;
            Undo.undoRedoPerformed -= SwitchEditMode;
            EditorApplication.playModeStateChanged -= SwitchEditMode;

            StopEdit();
        }

        void SwitchEditMode(PlayModeStateChange state) => SwitchEditMode();

        void SwitchEditMode()
        {
            if (target is Terrain2D t)
                Target = t;
            else return;

            if (ToolManager.IsActiveTool(this))
                StartEdit();
            else StopEdit();
            
            Target.Build();
        }


        protected void StartEdit()
        {
            if (IsEditing)
                StopEdit();

            IsEditing = true;
            OnStartEdit();
        }

        protected void StopEdit()
        {
            if (!IsEditing)
                return;

            _shouldRecordTarget = false;

            IsEditing = false;
            OnStopEdit();
        }

        protected void CancelEdit()
        {
            StopEdit();
            ToolManager.RestorePreviousTool();
        }


        protected Vector3 GetMousePosOnTerrain()
        {
            Plane plane = new Plane(Target.transform.forward, Target.transform.position);
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            if (!plane.Raycast(ray, out float dist))
                return Vector3.zero;

            return ray.GetPoint(dist);
        }

        protected Rect GetGroupSelectionRect(Vector2 startMousePos)
        {
            Rect rect = new Rect(startMousePos.x, startMousePos.y, Event.current.mousePosition.x - startMousePos.x, Event.current.mousePosition.y - startMousePos.y);
            if (rect.width < 0.0)
            {
                rect.x += rect.width;
                rect.width = -rect.width;
            }
            if (rect.height < 0.0)
            {
                rect.y += rect.height;
                rect.height = -rect.height;
            }

            return rect;
        }

        protected void DrawSelectionRect(Vector2 startMousePos)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            Handles.BeginGUI();
            GUIStyle guiStyle = "SelectionRect";
            guiStyle.Draw(GetGroupSelectionRect(startMousePos), false, false, false, false);
            Handles.EndGUI();
        }

        protected void DrawSettingsWindow(string title, Rect sceneViewRect)
        {
            SettingsWindowRect = GUILayout.Window(
                800600,
                SettingsWindowRect, 
                OnSettingsWindowGUI,
                title, 
                GUILayout.MaxWidth(80));
            
            SettingsWindowRect.x = sceneViewRect.width - SettingsWindowRect.width - 10;
            SettingsWindowRect.y = sceneViewRect.height - SettingsWindowRect.height - 10;

            Handles.EndGUI();
        }


        protected float FloatSettingField(string fieldName, float min, float max, out bool changed, params float[] input)
        {
            changed = false;
            if (input.Length == 0)
                return 0;

            EditorGUI.showMixedValue = input.Any(f => !Mathf.Approximately(f, input[0]));

            EditorGUI.BeginChangeCheck();
            input[0] = Mathf.Clamp(EditorGUILayout.FloatField(fieldName, input[0]), min, max);
            if (EditorGUI.EndChangeCheck())
            {
                for (int i = 0; i < input.Length; i++)
                    input[i] = input[0];
                ShouldRecordTarget = true;
                changed = true;
            }

            EditorGUI.showMixedValue = false;
            return input[0];
        }

        protected float FloatSettingField(string fieldName, out bool changed, params float[] input)
        {
            return FloatSettingField(fieldName, float.MinValue, float.MaxValue, out changed, input);
        }


        protected virtual void OnStartEdit() { }

        protected virtual void OnStopEdit() { }

        protected abstract void OnToolGUIInternal(EditorWindow window);

        protected virtual void OnSettingsWindowGUI(int windowId) { }

        protected virtual void OnTargetRecorded() { }


        public override bool IsAvailable() => true;

        public sealed override void OnToolGUI(EditorWindow window)
        {
            if (!IsEditing || Target == null)
                return;

            OnToolGUIInternal(window);

            if (ShouldRecordTarget)
            {
                Undo.RecordObject(Target, "Edit Terrain 2D");

                OnTargetRecorded();
                Target.Build();

                _shouldRecordTarget = false;
            }

            window.Repaint();
        }
    }
}