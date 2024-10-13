using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;

namespace T2D.Editor
{
    [EditorTool("Edit Path", typeof(Terrain2D))]
    public class Terrain2DPathTool : Terrain2DTool
    {
        private const float POINT_HANDLE_SIZE = 0.05f;

        private static GUIContent _toolbarIcon;

        public override GUIContent toolbarIcon => _toolbarIcon ?? (_toolbarIcon = new GUIContent(EditorGUIUtility.IconContent("EditCollider").image, "Edit Path"));

        private Path _path;
        private Vector2 _groupSelectionStartPos;
        private bool _isGroupSelection;


        protected override void OnStartEdit()
        {
            if (Target.Path.keys.Length <= 1)
            {
                Keyframe lastKeyFrame = Target.Path.keys.LastOrDefault();
                while (Target.Path.keys.Length <= 1)
                    Target.Path.AddKey(lastKeyFrame.time += 1, 0);
            }

            _path = new Path(Target.Path);
        }

        protected override void OnStopEdit()
        {
            _isGroupSelection = false;
        }

        
        void SetGroupSelection(Vector3 startPos)
        {
            Rect selRect = GetGroupSelectionRect(startPos);
            foreach (var p in _path)
            {
                var guiPos = HandleUtility.WorldToGUIPoint(Target.transform.TransformPoint(p.Pos));
                if (selRect.Contains(guiPos))
                    p.Selected = !p.Selected;
            }
        }

        bool MovePointTangent(Vector3 pointWorldPos, HandleControlId controlId, Vector3 worldAxis, ref float angle)
        {
            float handleSize = HandleUtility.GetHandleSize(pointWorldPos) * POINT_HANDLE_SIZE * 0.8f;

            Vector3 worldDir = Target.transform.TransformDirection(Quaternion.Euler(0, 0, angle) 
                * (controlId == HandleControlId.PointLeftTangent ? Vector3.left : Vector3.right));
            Vector3 worldPos = pointWorldPos + worldDir * handleSize * 10;

            Handles.DrawAAPolyLine(pointWorldPos, worldPos);

            EditorGUI.BeginChangeCheck();
            worldPos = Handles.Slider2D((int) controlId, worldPos,
                Target.transform.forward,
                Target.transform.up,
                Target.transform.right,
                handleSize,
                Handles.DotHandleCap, Vector2.zero, false);

            if (EditorGUI.EndChangeCheck())
            {
                angle = Vector3.SignedAngle((worldPos - pointWorldPos).normalized, worldAxis, -Target.transform.forward);
                return true;
            }

            return false;
        }


        void HandleSelectPoints()
        {
            switch (Event.current.type)
            {
                case EventType.KeyDown:
                    if (Event.current.control && Event.current.keyCode == KeyCode.A)
                    {
                        _path.SetSelection(_path.ToArray());
                        Event.current.Use();
                    }
                    else if (Event.current.keyCode == KeyCode.Escape)
                    {
                        if (_path.SelectedPoints.Any())
                        {
                            _path.SetSelection();
                            Event.current.Use();
                        }
                    }
                    break;
                case EventType.MouseDown:
                    if (Event.current.button == 0 && !SettingsWindowRect.Contains(Event.current.mousePosition))
                    {
                        var pointUnderMouse = _path.FirstOrDefault(p => _path.GetControlId(p) == HandleUtility.nearestControl);
                        if (pointUnderMouse != null)
                        {
                            if (Event.current.control)
                                pointUnderMouse.Selected = !pointUnderMouse.Selected;
                            else if (!pointUnderMouse.Selected)
                            {
                                _path.SetSelection();
                                pointUnderMouse.Selected = true;
                            }

                            pointUnderMouse.SelectionPos = pointUnderMouse.Pos;
                        }
                        else if (!Event.current.control && 
                                 HandleUtility.nearestControl != (int)HandleControlId.PointRightTangent && 
                                 HandleUtility.nearestControl != (int)HandleControlId.PointLeftTangent)
                        {
                            _path.SetSelection();
                        }

                        foreach (var p in _path)
                            p.SelectionPos = p.Pos;
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
                        if (!_isGroupSelection)
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
            PathPoint movingPoint = null;
            Vector2 movingPointPos = Vector2.zero;

            Color prevHandlesColor = Handles.color;

            foreach (var point in _path.OrderBy(p => p.Selected))
            {
                Vector3 worldPos = Target.transform.TransformPoint(point.Pos);
                float handleSize = HandleUtility.GetHandleSize(worldPos) * POINT_HANDLE_SIZE;

                Handles.color = point.Selected ? Color.white : Color.green;

                EditorGUI.BeginChangeCheck();
                Handles.Slider2D(_path.GetControlId(point), worldPos,
                    Target.transform.forward,
                    Target.transform.up,
                    Target.transform.right,
                    handleSize,
                    Handles.DotHandleCap, Vector2.one, false);

                if (!EditorGUI.EndChangeCheck())
                    continue;

                movingPoint = point;
                movingPointPos = Target.transform.InverseTransformPoint(GetMousePosOnTerrain());
                break;
            }

            Handles.color = prevHandlesColor;

            if (movingPoint != null)
            {
                Vector2 offset = movingPointPos - movingPoint.SelectionPos;
                foreach (var sp in _path.SelectedPoints)
                {
                    sp.Pos = sp.SelectionPos + offset;
                }
                ShouldRecordTarget = true;
            }
        }

        void HandleEditTangents()
        {
            if (!_path.SelectedPoints.Any() || _path.SelectedPoints.Count() > 1)
                return;

            var pathPoint = _path.SelectedPoints.First();
            Vector3 pathPointWorldPos = Target.transform.TransformPoint(pathPoint.Pos);

            float angle = pathPoint.InTangentAngle;
            ShouldRecordTarget = MovePointTangent(pathPointWorldPos, HandleControlId.PointLeftTangent, -Target.transform.right, ref angle);
            pathPoint.InTangentAngle = angle;

            angle = pathPoint.OutTangentAngle;
            ShouldRecordTarget = MovePointTangent(pathPointWorldPos, HandleControlId.PointRightTangent, Target.transform.right, ref angle);
            pathPoint.OutTangentAngle = angle;
        }

        void HandleAddPoint()
        {
            bool isOtherHandleUnderMouse = HandleUtility.nearestControl != (int)HandleControlId.Background;
            if (isOtherHandleUnderMouse || GUIUtility.hotControl != 0 || Event.current.control)
                return;

            if (Event.current.type != EventType.MouseDown || Event.current.button != 0 || Event.current.clickCount < 2)
                return;

            Vector3 pointPos = Target.transform.InverseTransformPoint(GetMousePosOnTerrain());
            if (pointPos == Vector3.zero)
                return;

            PathPoint newPoint = _path.Add(pointPos, true);
            GUIUtility.hotControl = _path.GetControlId(newPoint);
            _path.SetSelection(newPoint);

            ShouldRecordTarget = true;

            Event.current.Use();
        }

        void HandleRemovePoint()
        {
            if (Event.current.type != EventType.KeyDown || Event.current.keyCode != KeyCode.Delete)
                return;

            if (_path.SelectedPoints.Any() && GUIUtility.hotControl == 0)
            {
                foreach (var sp in _path.SelectedPoints.ToArray())
                {
                    if (_path.Count <= 2)
                        break;
                    _path.Remove(sp);
                }

                _path.SetSelection();
                ShouldRecordTarget = true;
            }

            Event.current.Use();
        }


        protected override void OnToolGUIInternal(EditorWindow window)
        {
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape && !_path.SelectedPoints.Any())
            {
                CancelEdit();
                Event.current.Use();
                return;
            }

            HandleUtility.AddDefaultControl((int)HandleControlId.Background);

            HandleSelectPoints();
            HandleMovePoints();
            HandleEditTangents();
            HandleAddPoint();
            HandleRemovePoint();

            int selectedPathPoints = _path.SelectedPoints.Count();
            if (selectedPathPoints > 0)
                DrawSettingsWindow(selectedPathPoints > 1 ? $"Path Points ({selectedPathPoints})" : "Path Point", window.position);
        }

        protected override void OnSettingsWindowGUI(int windowId)
        {
            PathPoint point = _path.SelectedPoints.LastOrDefault();
            if (point == null)
                return;

            float posX = FloatSettingField("X", out var valueChanged, _path.SelectedPoints.Select(p => p.Pos.x).ToArray());
            if (valueChanged)
                foreach (var p in _path.SelectedPoints)
                    p.Pos = new Vector2(posX, p.Pos.y);

            float posY = FloatSettingField("Y", out valueChanged, _path.SelectedPoints.Select(p => p.Pos.y).ToArray());
            if (valueChanged)
                foreach (var p in _path.SelectedPoints)
                    p.Pos = new Vector2(p.Pos.x, posY);

            float inTan = FloatSettingField("In Tangent", -PathPoint.TANGENT_MAX_ANGLE, PathPoint.TANGENT_MAX_ANGLE,
                out valueChanged, _path.SelectedPoints.Select(p => p.InTangentAngle).ToArray());
            if (valueChanged)
                foreach (var p in _path.SelectedPoints)
                    p.InTangentAngle = inTan;

            float outTan = FloatSettingField("Out Tangent", -PathPoint.TANGENT_MAX_ANGLE, PathPoint.TANGENT_MAX_ANGLE,
                out valueChanged, _path.SelectedPoints.Select(p => p.OutTangentAngle).ToArray());
            if (valueChanged)
                foreach (var p in _path.SelectedPoints)
                    p.OutTangentAngle = outTan;
        }

        protected override void OnTargetRecorded()
        {
            Target.Path = _path.GetPathCurve();
        }


        class Path : IList<PathPoint>
        {
            public PathPoint this[int index]
            {
                get => _points[index];
                set => _points[index] = value;
            }

            public int Count => _points.Count;
            public bool IsReadOnly => false;

            public IEnumerable<PathPoint> SelectedPoints => _points.Where(p => p.Selected);
            
            private readonly List<PathPoint> _points = new List<PathPoint>();
            

            public Path(AnimationCurve pathCurve = null)
            {
                if (pathCurve != null)
                    foreach (var key in pathCurve.keys)
                        _points.Add(new PathPoint(key));
            }


            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


            public void SetSelection(params PathPoint[] pathPoints) => _points.ForEach(p => p.Selected = pathPoints.Contains(p));

            public int GetControlId(PathPoint pathPoint) => _points.IndexOf(pathPoint) + (int) HandleControlId.Point;

            public AnimationCurve GetPathCurve()
            {
                return new AnimationCurve(_points.Select(p => p.GetKeyFrame()).ToArray());
            }
            
            
            public void Add(PathPoint pathPoint)
            {
                _points.Add(pathPoint);
            }

            public PathPoint Add(Vector2 pos, bool selected = false)
            {
                var curve = GetPathCurve();
                int id = curve.AddKey(pos.x, pos.y);
                var newPathPoint = new PathPoint(curve[id]) {Selected = selected};

                _points.Add(newPathPoint);

                return newPathPoint;
            }

            public void Clear() => _points.Clear();

            public bool Contains(PathPoint pathPoint) => _points.Contains(pathPoint);

            public void CopyTo(PathPoint[] array, int arrayIndex) => _points.CopyTo(array, arrayIndex);

            public bool Remove(PathPoint pathPoint)
            {
                return _points.Remove(pathPoint);
            }

            public int IndexOf(PathPoint pathPoint) => _points.IndexOf(pathPoint);

            public void Insert(int index, PathPoint pathPoint) => _points.Insert(index, pathPoint);

            public void RemoveAt(int index) => _points.RemoveAt(index);
            
            public IEnumerator<PathPoint> GetEnumerator() => _points.GetEnumerator();
        }

        class PathPoint
        {
            public const int TANGENT_MAX_ANGLE = 90;

            public bool Selected;

            public Vector2 Pos
            {
                get => _pos;
                set
                {
                    _pos.x = value.x;
                    _pos.y = Mathf.Clamp(value.y, 0, float.MaxValue);
                }
            }
            private Vector2 _pos;

            public Vector2 SelectionPos;

            public float InTangentAngle
            {
                get => _inTangentAngle;
                set => _inTangentAngle = Mathf.Clamp(value, -TANGENT_MAX_ANGLE, TANGENT_MAX_ANGLE);
            }
            private float _inTangentAngle;

            public float OutTangentAngle
            {
                get => _outTangentAngle;
                set => _outTangentAngle = Mathf.Clamp(value, -TANGENT_MAX_ANGLE, TANGENT_MAX_ANGLE);
            }
            private float _outTangentAngle;


            public PathPoint(Keyframe key)
            {
                Pos = new Vector2(key.time, key.value);
                SelectionPos = Pos;

                InTangentAngle = TanToAngle(key.inTangent);
                OutTangentAngle = TanToAngle(key.outTangent);
            }

            public Keyframe GetKeyFrame() => 
                new Keyframe(Pos.x, Pos.y, AngleToTan(InTangentAngle), AngleToTan(OutTangentAngle));


            static float TanToAngle(float tan) => Mathf.Atan(tan) * Mathf.Rad2Deg;

            static float AngleToTan(float angle) => Mathf.Tan(angle * Mathf.Deg2Rad);
        }

        enum HandleControlId
        {
            Background = 580000,
            PointSettingsWindow = 580300,
            PointLeftTangent = 581000,
            PointRightTangent = 581001,
            Point = 590000,
        }
    }
}
