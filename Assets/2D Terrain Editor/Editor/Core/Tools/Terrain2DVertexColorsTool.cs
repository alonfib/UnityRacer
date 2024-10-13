using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;

namespace T2D.Editor
{
    [EditorTool("Edit Vertex Colors", typeof(Terrain2D))]
    public class Terrain2DVertexColorsTool : Terrain2DTool
    {
        private const float POINT_HANDLE_SIZE = 0.1f;

        private static GUIContent _toolbarIcon;

        public override GUIContent toolbarIcon => _toolbarIcon ?? (_toolbarIcon = new GUIContent(EditorGUIUtility.IconContent("SceneViewRGB").image, "Edit Vertex Colors"));

        private Vector2 _groupSelectionStartPos;
        private bool _isGroupSelection;

        private readonly List<VertexColorPoint> _vertexColorPoints = new List<VertexColorPoint>();


        protected override void OnStartEdit()
        {
            _vertexColorPoints.Clear();
            for (int i = 0; i < Target.VertexColorKeys.Count; i++)
                _vertexColorPoints.Add(new VertexColorPoint(i, Target.VertexColorKeys[i]));
        }

        protected override void OnStopEdit()
        {
            _isGroupSelection = false;
        }

        void SetGroupSelection(Vector2 startMousePos)
        {
            Rect selRect = GetGroupSelectionRect(startMousePos);
            foreach (var p in _vertexColorPoints)
            {
                var guiPos = HandleUtility.WorldToGUIPoint(Target.transform.TransformPoint(new Vector3(p.Pos, 0, 0)));
                if (selRect.Contains(guiPos))
                    p.Selected = !p.Selected;
            }
        }

        Color32 ColorSettingField(string fieldName, out bool changed, params Color32[] input)
        {
            changed = false;
            if (input.Length == 0)
                return Color.white;
            
            EditorGUI.showMixedValue = input.Any(f => !f.Equals(input[0]));

            EditorGUI.BeginChangeCheck();
            input[0] = EditorGUILayout.ColorField(fieldName, input[0]);
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


        void HandleSelectPoints()
        {
            switch (Event.current.type)
            {
                case EventType.KeyDown:
                    if (Event.current.control && Event.current.keyCode == KeyCode.A)
                    {
                        _vertexColorPoints.ForEach(p => p.Selected = true);
                        Event.current.Use();
                    }
                    else if (Event.current.keyCode == KeyCode.Escape)
                    {
                        if (_vertexColorPoints.Any(p => p.Selected))
                        {
                            _vertexColorPoints.ForEach(p => p.Selected = false);
                            Event.current.Use();
                        }
                    }
                    break;
                case EventType.MouseDown:
                    if (Event.current.button == 0 && !SettingsWindowRect.Contains(Event.current.mousePosition))
                    {
                        var pointUnderMouse = _vertexColorPoints.FirstOrDefault(p => p.ControlId == HandleUtility.nearestControl);
                        if (pointUnderMouse != null)
                        {
                            if (Event.current.control)
                                pointUnderMouse.Selected = !pointUnderMouse.Selected;
                            else if (!pointUnderMouse.Selected)
                            {
                                _vertexColorPoints.ForEach(p => p.Selected = false);
                                pointUnderMouse.Selected = true;
                            }

                            pointUnderMouse.SelectionPos = pointUnderMouse.Pos;
                        }
                        else if (!Event.current.control)
                            _vertexColorPoints.ForEach(p => p.Selected = false);

                        _vertexColorPoints.ForEach(p => p.SelectionPos = p.Pos);
                    }
                    break;
                case EventType.MouseUp:
                    if (_isGroupSelection)
                    {
                        SetGroupSelection(_groupSelectionStartPos);
                        _isGroupSelection = false;
                    }
                    break;
                case EventType.MouseDrag:
                    if (GUIUtility.hotControl <= 0 && Event.current.button == 0)
                    {
                        if (!_isGroupSelection && Event.current.button == 0)
                        {
                            _groupSelectionStartPos = Event.current.mousePosition;
                            _isGroupSelection = true;
                        }
                    }
                    else _isGroupSelection = false;
                    break;
                case EventType.MouseLeaveWindow:
                    _isGroupSelection = false;
                    break;
                case EventType.Repaint:
                    if (_isGroupSelection)
                        DrawSelectionRect(_groupSelectionStartPos);
                    break;
            }
        }

        void HandleMovePoints()
        {
            VertexColorPoint movingPoint = null;
            float movingPointPos = 0;

            Color prevHandlesColor = Handles.color;

            foreach (var point in _vertexColorPoints.OrderBy(p => p.Selected))
            {
                Vector3 worldPos = Target.transform.TransformPoint(new Vector3(point.Pos, 0, 0));
                float handleSize = HandleUtility.GetHandleSize(worldPos) * POINT_HANDLE_SIZE;

                if (point.Selected)
                {
                    Handles.color = Color.white;
                    Handles.DotHandleCap(-1, worldPos, Quaternion.identity, handleSize * 1.6f, EventType.Repaint);
                }

                Handles.color = new Color32(point.Color.r, point.Color.g, point.Color.b, 255);

                EditorGUI.BeginChangeCheck();
                Handles.Slider2D(point.ControlId, worldPos,
                    Target.transform.forward,
                    Target.transform.up,
                    Target.transform.right,
                    handleSize,
                    Handles.DotHandleCap, Vector2.one, false);

                if (!EditorGUI.EndChangeCheck())
                    continue;

                movingPoint = point;
                movingPointPos = Target.transform.InverseTransformPoint(GetMousePosOnTerrain()).x;
                break;
            }

            Handles.color = prevHandlesColor;

            if (movingPoint != null)
            {
                float offset = movingPointPos - movingPoint.SelectionPos;
                foreach (var p in _vertexColorPoints.Where(p => p.Selected))
                    p.Pos = p.SelectionPos + offset;

                ShouldRecordTarget = true;
            }
        }

        void HandleAddPoint()
        {
            if (GUIUtility.hotControl != 0 || Event.current.control)
                return;

            if (Event.current.type != EventType.MouseDown || Event.current.button != 0 || Event.current.clickCount < 2)
                return;

            float pos = Target.transform.InverseTransformPoint(GetMousePosOnTerrain()).x;
            VertexColorPoint newPoint = new VertexColorPoint(_vertexColorPoints.Count, pos, Color.white) {Selected = true};
            
            _vertexColorPoints.ForEach(p => p.Selected = false);
            _vertexColorPoints.Add(newPoint);

            GUIUtility.hotControl = newPoint.ControlId;

            ShouldRecordTarget = true;

            Event.current.Use();
        }

        void HandleRemovePoint()
        {
            if (Event.current.type != EventType.KeyDown || Event.current.keyCode != KeyCode.Delete)
                return;

            var selectedPoints = _vertexColorPoints.Where(p => p.Selected).ToArray();
            if (selectedPoints.Length > 0 && GUIUtility.hotControl == 0)
            {
                foreach (var p in selectedPoints)
                    _vertexColorPoints.Remove(p);
                ShouldRecordTarget = true;
            }

            Event.current.Use();
        }


        protected override void OnToolGUIInternal(EditorWindow window)
        {
            SerializedObject o = new SerializedObject(Target);
            o.Update();

            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape && !_vertexColorPoints.Any(p => p.Selected))
            {
                CancelEdit();
                Event.current.Use();
                return;
            }
            
            HandleUtility.AddDefaultControl((int)HandleControlId.Background);

            HandleSelectPoints();
            HandleMovePoints();
            HandleAddPoint();
            HandleRemovePoint();

            var selectedPoints = _vertexColorPoints.Count(p => p.Selected);
            if (selectedPoints > 0)
                DrawSettingsWindow(selectedPoints > 1 ? $"Vertex Color Keys ({selectedPoints})" : "Vertex Color Key", window.position);

            o.ApplyModifiedProperties();
        }

        protected override void OnSettingsWindowGUI(int windowId)
        {
            VertexColorPoint point = _vertexColorPoints.LastOrDefault(p => p.Selected);
            if (point == null)
                return;

            var selectedPoints = _vertexColorPoints.Where(p => p.Selected).ToArray();

            float pos = FloatSettingField("Position", out var valueChanged, selectedPoints.Select(p => p.Pos).ToArray());
            if (valueChanged)
                foreach (var p in selectedPoints)
                    p.Pos = pos;

            Color color = ColorSettingField("Color", out valueChanged, selectedPoints.Select(p => p.Color).ToArray());
            if (valueChanged)
                foreach (var p in selectedPoints)
                    p.Color = color;
        }

        protected override void OnTargetRecorded()
        {
            Target.VertexColorKeys.Clear();
            foreach (var p in _vertexColorPoints)
                Target.VertexColorKeys.Add(p.GetKey());

            for (int i = 0; i < _vertexColorPoints.Count; i++)
                _vertexColorPoints[i].Id = i;
        }


        class VertexColorPoint
        {
            public int Id
            {
                set => ControlId = (int) HandleControlId.Point + value;
            }
            public int ControlId { get; private set; }
            
            public float Pos;
            public Color32 Color;
            public float SelectionPos;
            public bool Selected;


            public VertexColorPoint(int id, Terrain2D.VertexColorKeyPoint keyPoint)
            {
                Id = id;
                Pos = keyPoint.Position;
                Color = keyPoint.Color;
                SelectionPos = keyPoint.Position;
            }

            public VertexColorPoint(int id, float pos, Color color)
            {
                ControlId = (int)HandleControlId.Point + id;
                Pos = pos;
                Color = color;
                SelectionPos = pos;
            }

            public Terrain2D.VertexColorKeyPoint GetKey() => new Terrain2D.VertexColorKeyPoint(Pos, Color);
        }

        enum HandleControlId
        {
            Background = 580000,
            PointSettingsWindow = 580300,
            Point = 590000,
        }
    }
}