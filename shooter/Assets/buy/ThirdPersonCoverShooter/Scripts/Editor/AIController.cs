using UnityEditor;
using UnityEngine;

namespace CoverShooter
{
    [CustomEditor(typeof(AIController))]
    [CanEditMultipleObjects]
    public class AIControllerEditor : Editor
    {
        private static int _editorHash = "CoverWaypointEditor".GetHashCode();

        private AIController _lastSelectedController;
        private int _lastSelectedWaypoint;
        private bool _dontDrawPreview = false;
        private bool _wasMouseTooFarAway = false;

        [DrawGizmo(GizmoType.NotInSelectionHierarchy | GizmoType.InSelectionHierarchy)]
        public static void DrawDebugAINotSelected(Transform transform, GizmoType gizmoType)
        {
            if (!Application.isPlaying)
                return;

            debugAI(transform);
        }

        private static void debugAI(Transform transform)
        {
            var motor = transform.GetComponent<CharacterMotor>();
            if (motor == null || !motor.IsAlive) return;

            var ai = transform.GetComponent<AIController>();
            if (ai == null) return;

            var position = transform.position + Vector3.up * 2;
            var offset = -8;

            DebugText.Draw(ai.State.ToString(), position, 0, offset);
            offset += 16;

            DebugText.Draw(ai.StateReason.ToString(), position, 0, offset);
            offset += 16;

            if (ai.IsAlerted)
            {
                if (ai.Situation.WouldLikeToRetreat)
                {
                    DebugText.Draw("Would like to retreat", position, 0, offset, Color.yellow);
                    offset += 16;
                }

                if (motor.Target != null)
                {
                    var actor = motor.Target.GetComponent<Actor>();

                    if (actor != null && actor.Side == ai.Actor.Side)
                    {
                        DebugText.Draw("Is aiming at a friend", position, 0, offset, Color.red);
                        offset += 16;
                    }
                }

                if (ai.Situation.IsAllowedToBeAggressive)
                {
                    DebugText.Draw("Is aggressive", position, 0, offset, Color.green);
                    offset += 16;
                }

                if (ai.IsAiming)
                {
                    DebugText.Draw("Is aiming", position, 0, offset, Color.cyan);
                    offset += 16;
                }

                if (ai.Situation.Threat != null)
                {
                    if (ai.Situation.CanSeeTheThreat)
                        Debug.DrawLine(ai.Situation.CurrentPosition, ai.Situation.ThreatGroundPosition, Color.green);
                    else
                        Debug.DrawLine(ai.Situation.CurrentPosition, ai.Situation.ThreatGroundPosition, Color.red);
                }
                else
                    Debug.DrawLine(ai.Situation.CurrentPosition, ai.Situation.ThreatGroundPosition, Color.red);
            }

            if (AIUtil.IsMovingState(ai.State))
                Debug.DrawLine(ai.Situation.CurrentPosition, ai.Situation.TargetPosition, Color.blue);
        }

        protected virtual void OnSceneGUI()
        {
            var controller = (AIController)target;
            var controlId = GUIUtility.GetControlID(_editorHash, FocusType.Passive);

            var hasMousePosition = false;
            var mousePosition = Vector3.zero;

            Undo.RecordObject(controller, "Edit waypoints");

            if (SceneView.currentDrawingSceneView.camera.pixelRect.Contains(HandleUtility.GUIPointToScreenPixelCoordinate(Event.current.mousePosition)))
            {
                var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                    if (Vector3.Dot(hit.normal, Vector3.up) > 0.25f)
                    {
                        mousePosition = hit.point;
                        hasMousePosition = true;
                    }
            }

            var current = Event.current;
            var hasToSelect = false;

            switch (current.GetTypeForControl(controlId))
            {
                case EventType.KeyDown:
                    if (current.keyCode == KeyCode.Delete)
                        if (_lastSelectedController == controller && _lastSelectedController.Waypoints != null && _lastSelectedWaypoint >= 0 && _lastSelectedWaypoint < _lastSelectedController.Waypoints.Length)
                        {
                            deleteWaypoint(_lastSelectedController, _lastSelectedWaypoint);

                            while (_lastSelectedWaypoint >= _lastSelectedController.Waypoints.Length)
                                _lastSelectedWaypoint--;

                            current.Use();
                        }
                    break;

                case EventType.MouseDown:
                case EventType.MouseDrag:
                    if (current.button == 0)
                        if ((GUIUtility.hotControl == 0 && current.GetTypeForControl(controlId) != EventType.MouseDrag && HandleUtility.nearestControl == controlId) || 
                            GUIUtility.hotControl == controlId)
                        {
                            _dontDrawPreview = false;

                            if (hasMousePosition)
                            {
                                if (!_wasMouseTooFarAway)
                                    if (current.type == EventType.MouseDown)
                                    {
                                        GUIUtility.hotControl = controlId;
                                        hasToSelect = true;
                                    }

                                current.Use();
                            }
                        }
                    break;

                case EventType.MouseUp:
                    if (GUIUtility.hotControl == controlId)
                        GUIUtility.hotControl = 0;
                    break;

                case EventType.MouseMove:
                    if (HandleUtility.nearestControl != controlId)
                    {
                        hasMousePosition = false;
                        _dontDrawPreview = !_wasMouseTooFarAway;
                    }
                    else
                        _dontDrawPreview = false;

                    SceneView.RepaintAll();
                    break;

                case EventType.Layout:
                    if (!_wasMouseTooFarAway)
                        HandleUtility.AddDefaultControl(controlId);
                    break;
            }

            var isHoveringPoint = false;

            var hasLowestPoint = false;
            var lowestPoint = 0f;

            if (controller.Waypoints != null)
            {
                for (int i = 0; i < controller.Waypoints.Length; i++)
                    if (!hasLowestPoint || controller.Waypoints[i].Position.y < lowestPoint)
                    {
                        hasLowestPoint = true;
                        lowestPoint = controller.Waypoints[i].Position.y;
                    }

                var oldColor = Handles.color;
                Handles.color = Color.magenta;

                for (int i = 0; i < controller.Waypoints.Length; i++)
                {
                    var bottom = controller.Waypoints[i].Position;
                    bottom.y = lowestPoint;

                    Handles.DrawLine(controller.Waypoints[i].Position, bottom);
                }

                Handles.color = oldColor;
            }

            if (controller.Waypoints != null)
            {
                var oldColor = Handles.color;
                Handles.color = Color.yellow;

                const float maxDistance = 0.5f;

                for (int i = 0; i < controller.Waypoints.Length; i++)
                {
                    if (i == 0 && controller.Waypoints.Length > 1)
                    {
                        var p0 = controller.Waypoints[0].Position;
                        var p1 = controller.Waypoints[1].Position;
                        var right = Vector3.Cross(p1 - p0, Vector3.up);

                        Handles.DrawLine(p1, p1 + Vector3.Lerp(p0 - p1, right, 0.35f) * 0.75f);
                        Handles.DrawLine(p1, p1 + Vector3.Lerp(p0 - p1, -right, 0.35f) * 0.75f);
                        Handles.DrawWireDisc(controller.Waypoints[i].Position, Vector3.up, maxDistance * 0.75f);
                    }

                    var isHovered = false;
                    var wasJustSelected = false;

                    if (hasMousePosition && !isHoveringPoint && (GUIUtility.hotControl == 0 || hasToSelect))
                    {
                        if (Vector3.Distance(mousePosition, controller.Waypoints[i].Position) < maxDistance)
                        {
                            isHovered = true;
                            isHoveringPoint = true;

                            if (!_dontDrawPreview)
                            {
                                Handles.color = Color.white;
                                Handles.DrawWireDisc(controller.Waypoints[i].Position, Vector3.up, maxDistance);
                                Handles.color = Color.yellow;
                            }

                            if (hasToSelect)
                            {
                                _lastSelectedWaypoint = i;
                                _lastSelectedController = controller;
                                hasToSelect = false;
                                wasJustSelected = true;
                                EditorUtility.SetDirty(target);
                            }
                        }
                    }

                    if (!hasToSelect && !wasJustSelected)
                    {
                        if (_lastSelectedController == controller && _lastSelectedWaypoint == i)
                        {
                            Handles.color = Color.green;
                            Handles.DrawWireDisc(controller.Waypoints[i].Position, Vector3.up, maxDistance);

                            if (hasMousePosition)
                                if (GUIUtility.hotControl == controlId)
                                    controller.Waypoints[i].Position = mousePosition;

                            Handles.color = Color.yellow;
                        }
                        else if (!isHovered)
                            Handles.DrawWireDisc(controller.Waypoints[i].Position, Vector3.up, maxDistance * 0.5f);
                    }

                    if (controller.Waypoints.Length > 1)
                    {
                        var next = controller.Waypoints[i].Position;

                        Vector3 previous;

                        if (i == 0)
                            previous = controller.Waypoints[controller.Waypoints.Length - 1].Position;
                        else
                            previous = controller.Waypoints[i - 1].Position;

                        Handles.DrawLine(previous, next);
                    }
                }

                Handles.color = oldColor;
            }

            var canPlacePoint = false;

            if (hasMousePosition && !isHoveringPoint && (GUIUtility.hotControl == 0 || hasToSelect))
            {
                var oldColor = Handles.color;
                Handles.color = Color.white;

                var indexInBetween = -1;
                var minDist = 0f;

                if (controller.Waypoints != null)
                {
                    const float maxDistance = 8;

                    if (controller.Waypoints.Length == 0)
                    {
                        indexInBetween = -1;
                        canPlacePoint = Vector3.Distance(mousePosition, controller.transform.position) < maxDistance;
                    }
                    else if (controller.Waypoints.Length == 1)
                    {
                        indexInBetween = 0;
                        canPlacePoint = Vector3.Distance(mousePosition, controller.Waypoints[0].Position) < maxDistance;

                        if (canPlacePoint && !_dontDrawPreview)
                            Handles.DrawLine(mousePosition, controller.Waypoints[0].Position);
                    }
                    else
                    {
                        for (int i = 0; i < controller.Waypoints.Length; i++)
                        {
                            var next = (i == controller.Waypoints.Length - 1) ? 0 : i + 1;

                            var p0 = controller.Waypoints[i].Position;
                            var p1 = controller.Waypoints[next].Position;
                            var dist = Vector3.Distance(mousePosition, Util.FindClosestToPath(p0, p1, mousePosition));

                            if (dist < maxDistance)
                            {
                                if (indexInBetween < 0 || dist < minDist)
                                {
                                    indexInBetween = i;
                                    minDist = dist;
                                }
                            }
                        }

                        if (indexInBetween >= 0)
                        {
                            canPlacePoint = true;

                            if (!_dontDrawPreview)
                            {
                                var second = (indexInBetween == controller.Waypoints.Length - 1) ? 0 : indexInBetween + 1;
                                Handles.DrawLine(mousePosition, controller.Waypoints[indexInBetween].Position);
                                Handles.DrawLine(mousePosition, controller.Waypoints[second].Position);
                            }
                        }
                    }
                }

                if (canPlacePoint)
                {
                    if (!_dontDrawPreview)
                    {
                        Handles.DrawWireDisc(mousePosition, Vector3.up, 0.5f);

                        if (hasLowestPoint)
                        {
                            Handles.color = Color.grey;
                            Handles.DrawLine(mousePosition, new Vector3(mousePosition.x, lowestPoint, mousePosition.z));
                            Handles.color = Color.white;
                        }
                    }

                    if (hasToSelect)
                    {
                        var index = indexInBetween + 1;

                        if (controller.Waypoints == null)
                            controller.Waypoints = new Waypoint[1];
                        else if (controller.Waypoints.Length == 1)
                        {
                            var old = controller.Waypoints[0];
                            controller.Waypoints = new Waypoint[2];
                            controller.Waypoints[0] = old;
                        }
                        else
                        {
                            var old = controller.Waypoints;
                            controller.Waypoints = new Waypoint[old.Length + 1];

                            for (int i = 0; i < index; i++)
                                controller.Waypoints[i] = old[i];

                            for (int i = index; i < old.Length; i++)
                                controller.Waypoints[i + 1] = old[i];
                        }

                        controller.Waypoints[index].Position = mousePosition;

                        if (index > 0)
                            controller.Waypoints[index].Run = controller.Waypoints[index - 1].Run;
                        else
                            controller.Waypoints[index].Run = controller.Waypoints[controller.Waypoints.Length - 1].Run;

                        _lastSelectedController = controller;
                        _lastSelectedWaypoint = index;
                    }
                }

                Handles.color = oldColor;
            }

            _wasMouseTooFarAway = !canPlacePoint && !isHoveringPoint;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (targets.Length != 1)
                return;

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.LabelField("Waypoints", EditorStyles.boldLabel);

            var controller = (AIController)targets[0];

            Undo.RecordObject(controller, "Change waypoints");

            if (controller.Waypoints != null)
            {
                int toDelete = -1;
                int toMakeFirst = -1;

                if (GUILayout.Button("Reverse"))
                {
                    if (controller.Waypoints.Length == 2)
                    {
                        var t = controller.Waypoints[0];
                        controller.Waypoints[0] = controller.Waypoints[1];
                        controller.Waypoints[1] = t;
                    }
                    else if (controller.Waypoints.Length > 2)
                    {
                        for (int i = 1; i < controller.Waypoints.Length / 2 + 1; i++)
                        {
                            var t = controller.Waypoints[i];
                            controller.Waypoints[i] = controller.Waypoints[controller.Waypoints.Length - i];
                            controller.Waypoints[controller.Waypoints.Length - i] = t;
                        }
                    }

                    if (_lastSelectedWaypoint > 0)
                        _lastSelectedWaypoint = controller.Waypoints.Length - _lastSelectedWaypoint;
                }

                for (int i = 0; i < controller.Waypoints.Length; i++)
                {
                    EditorGUILayout.Space();

                    var isSelected = controller == _lastSelectedController && i == _lastSelectedWaypoint;

                    if (isSelected)
                    {
                        var oldColor = GUI.backgroundColor;
                        GUI.backgroundColor = Color.green;
                        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                        GUI.backgroundColor = oldColor;
                    }
                    else
                        EditorGUILayout.BeginHorizontal();

                    var rect = EditorGUILayout.BeginVertical();
                    GUI.Box(rect, GUIContent.none);                    

                    controller.Waypoints[i].Position = EditorGUILayout.Vector3Field("Position", controller.Waypoints[i].Position);
                    controller.Waypoints[i].Pause = EditorGUILayout.FloatField("Pause", controller.Waypoints[i].Pause);
                    controller.Waypoints[i].Run = EditorGUILayout.Toggle("Run", controller.Waypoints[i].Run);

                    if (!isSelected)
                        if (GUILayout.Button("Select"))
                        {
                            _lastSelectedController = controller;
                            _lastSelectedWaypoint = i;
                            SceneView.RepaintAll();
                        }

                    if (i > 0)
                        if (GUILayout.Button("Make First"))
                            toMakeFirst = i;

                    EditorGUILayout.EndVertical();

                    {
                        var oldColor = GUI.backgroundColor;
                        GUI.backgroundColor = Color.red;
                        if (GUILayout.Button("X", GUILayout.Width(20)))
                            toDelete = i;
                        GUI.backgroundColor = oldColor;
                    }

                    EditorGUILayout.EndHorizontal();
                }

                if (toDelete >= 0)
                    deleteWaypoint(controller, toDelete);

                if (toMakeFirst >= 0)
                {
                    var old = controller.Waypoints;
                    controller.Waypoints = new Waypoint[old.Length];

                    for (int i = 0; i < old.Length;i++)
                        controller.Waypoints[i] = old[(toMakeFirst + i) % old.Length];

                    _lastSelectedWaypoint = 0;
                }
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Add Waypoint"))
            {
                if (controller.Waypoints == null)
                    controller.Waypoints = new Waypoint[1];
                else
                {
                    var old = controller.Waypoints;
                    controller.Waypoints = new Waypoint[old.Length + 1];

                    for (int i = 0; i < old.Length; i++)
                        controller.Waypoints[i] = old[i];
                }

                var value = new Waypoint();

                if (controller.Waypoints.Length > 1)
                    value.Position = controller.Waypoints[controller.Waypoints.Length - 2].Position;
                else
                    value.Position = controller.transform.position;

                controller.Waypoints[controller.Waypoints.Length - 1] = value;
            }
        }

        private void deleteWaypoint(AIController controller, int index)
        {
            var old = controller.Waypoints;
            controller.Waypoints = new Waypoint[old.Length - 1];

            for (int i = 0; i < index; i++)
                controller.Waypoints[i] = old[i];

            for (int i = index + 1; i < old.Length; i++)
                controller.Waypoints[i - 1] = old[i];
        }
    }
}
