using UnityEditor;
using UnityEngine;
using PDSim.Helpers;

namespace PDSim.Editor.Inspector
{
    /// <summary>
    /// Custom editor for Helper_AreaPointSelect, providing scene handles for resizing the area.
    /// </summary>
    [CustomEditor(typeof(Helper_AreaPointSelect))]
    public class Helper_AreaPointSelectEditor : UnityEditor.Editor
    {
        #region Fields
        // Each axis has two face handles (+X/-X, +Y/-Y, +Z/-Z)
        private static readonly Vector3[] _faceDirections =
        {
            Vector3.right, Vector3.left,
            Vector3.up,    Vector3.down,
            Vector3.forward, Vector3.back
        };

        // Directional light from top-right-front; maps face normals to [minShade, 1].
        private static readonly Vector3 _sunDir =
            new Vector3(0.45f, 1f, 0.35f).normalized;

        private Vector3? _previewPoint;
        private float _previewTimer;
        #endregion

        #region Unity Lifecycle
        /// <summary>
        /// Draws the custom inspector GUI for AreaPointSelect.
        /// </summary>
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(6);

            var area = (Helper_AreaPointSelect)target;

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Preview Random Point", GUILayout.Height(24)))
            {
                Vector3 pt = area.GetRandomPoint();
                Debug.Log($"[AreaPointSelect] '{area.name}' random point: {pt}");
                SceneView.lastActiveSceneView?.Repaint();
                _previewPoint = pt;
                _previewTimer = (float)EditorApplication.timeSinceStartup + 2f;
            }
            if (GUILayout.Button("Reset Offset", GUILayout.Height(24)))
            {
                Undo.RecordObject(area, "Reset AreaPointSelect Offset");
                area.Offset = Vector3.zero;
                EditorUtility.SetDirty(area);
            }
            EditorGUILayout.EndHorizontal();

            if (_previewPoint.HasValue)
            {
                EditorGUILayout.HelpBox(
                    $"Last preview: {_previewPoint.Value:F2}  (visible for 2 s in Scene view)",
                    MessageType.None);
            }
        }

        private void OnSceneGUI()
        {
            var area = (Helper_AreaPointSelect)target;

            // Expire preview point
            if (_previewPoint.HasValue && EditorApplication.timeSinceStartup > _previewTimer)
            {
                _previewPoint = null;
                Repaint();
            }

            Color solid = area.GizmoColor;
            Vector3 center = area.Center;

            // Shaded faces
            DrawShadedBox(center, area.Size, solid);

            // Wire box on top
            Handles.color = solid;
            Handles.DrawWireCube(center, area.Size);

            // Dimension label
            if (area.ShowLabel)
            {
                Handles.color = Color.white;
                GUIStyle labelStyle = new GUIStyle(GUI.skin.label)
                {
                    normal = { textColor = area.GizmoColor },
                    fontStyle = FontStyle.Bold,
                    fontSize = 11
                };
                Handles.Label(
                    center + Vector3.up * (area.Size.y * 0.5f + 0.15f),
                    $"{area.name}\n{area.Size.x:F1} × {area.Size.y:F1} × {area.Size.z:F1}",
                    labelStyle);
            }

            // Rendering the offset position handle for the area.
            EditorGUI.BeginChangeCheck();
            Vector3 newCenter = Handles.PositionHandle(center, Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(area, "Move AreaPointSelect Offset");
                area.Offset = newCenter - area.transform.position;
                EditorUtility.SetDirty(area);
            }

            // Rendering handles to resize the area's faces.
            Handles.color = solid;
            float capSize = HandleUtility.GetHandleSize(center) * 0.08f;

            Vector3 halfSize = area.Size * 0.5f;
            Vector3[] faceOffsets =
            {
                new Vector3( halfSize.x, 0, 0), new Vector3(-halfSize.x, 0, 0),
                new Vector3(0,  halfSize.y, 0), new Vector3(0, -halfSize.y, 0),
                new Vector3(0, 0,  halfSize.z), new Vector3(0, 0, -halfSize.z)
            };

            for (int i = 0; i < _faceDirections.Length; i++)
            {
                Vector3 handlePos = center + faceOffsets[i];
                EditorGUI.BeginChangeCheck();
                Vector3 newPos = Handles.Slider(handlePos, _faceDirections[i],
                    capSize * 2f, Handles.DotHandleCap, 0.5f);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(area, "Resize AreaPointSelect");

                    // delta > 0 means the face moved outward (_faceDirections already
                    // encodes the sign, so no extra sign flip needed).
                    float delta = Vector3.Dot(newPos - handlePos, _faceDirections[i]);
                    Vector3 dir = _faceDirections[i];
                    Vector3 newSize   = area.Size;
                    Vector3 newOffset = area.Offset;

                    // Grow the size by delta, shift the offset by delta/2 so the
                    // opposite face stays fixed.
                    if (i < 2)
                    {
                        newSize.x    = Mathf.Max(0.01f, newSize.x + delta);
                        newOffset.x += dir.x * delta * 0.5f;
                    }
                    else if (i < 4)
                    {
                        newSize.y    = Mathf.Max(0.01f, newSize.y + delta);
                        newOffset.y += dir.y * delta * 0.5f;
                    }
                    else
                    {
                        newSize.z    = Mathf.Max(0.01f, newSize.z + delta);
                        newOffset.z += dir.z * delta * 0.5f;
                    }

                    area.Size   = newSize;
                    area.Offset = newOffset;
                    EditorUtility.SetDirty(area);
                }
            }

            // Rendering the preview point if one is currently active.
            if (_previewPoint.HasValue)
            {
                Handles.color = Color.magenta;
                Handles.SphereHandleCap(0, _previewPoint.Value,
                    Quaternion.identity,
                    HandleUtility.GetHandleSize(_previewPoint.Value) * 0.18f,
                    EventType.Repaint);
                Handles.Label(_previewPoint.Value + Vector3.up * 0.2f, "●", new GUIStyle
                {
                    normal = { textColor = Color.magenta },
                    fontStyle = FontStyle.Bold
                });
                HandleUtility.Repaint();
            }
        }
        #endregion

        #region Private Methods
        private static void DrawShadedBox(Vector3 center, Vector3 size, Color baseColor)
        {
            Vector3 h = size * 0.5f;

            // Each face: (normal, 4 corners in order)
            (Vector3 normal, Vector3[] verts)[] faces =
            {
                (Vector3.up,      new[]{ center+new Vector3(-h.x, h.y,-h.z), center+new Vector3( h.x, h.y,-h.z), center+new Vector3( h.x, h.y, h.z), center+new Vector3(-h.x, h.y, h.z) }),
                (Vector3.down,    new[]{ center+new Vector3(-h.x,-h.y, h.z), center+new Vector3( h.x,-h.y, h.z), center+new Vector3( h.x,-h.y,-h.z), center+new Vector3(-h.x,-h.y,-h.z) }),
                (Vector3.right,   new[]{ center+new Vector3( h.x,-h.y,-h.z), center+new Vector3( h.x, h.y,-h.z), center+new Vector3( h.x, h.y, h.z), center+new Vector3( h.x,-h.y, h.z) }),
                (Vector3.left,    new[]{ center+new Vector3(-h.x,-h.y, h.z), center+new Vector3(-h.x, h.y, h.z), center+new Vector3(-h.x, h.y,-h.z), center+new Vector3(-h.x,-h.y,-h.z) }),
                (Vector3.forward, new[]{ center+new Vector3(-h.x,-h.y, h.z), center+new Vector3( h.x,-h.y, h.z), center+new Vector3( h.x, h.y, h.z), center+new Vector3(-h.x, h.y, h.z) }),
                (Vector3.back,    new[]{ center+new Vector3( h.x,-h.y,-h.z), center+new Vector3(-h.x,-h.y,-h.z), center+new Vector3(-h.x, h.y,-h.z), center+new Vector3( h.x, h.y,-h.z) }),
            };

            const float minShade = 0.3f;
            const float baseAlpha = 0.13f;

            foreach (var (normal, verts) in faces)
            {
                float shade = Mathf.Lerp(minShade, 1f,
                    (Vector3.Dot(normal, _sunDir) + 1f) * 0.5f);
                Color faceColor = new Color(
                    baseColor.r * shade,
                    baseColor.g * shade,
                    baseColor.b * shade,
                    baseAlpha);
                Handles.DrawSolidRectangleWithOutline(verts, faceColor, Color.clear);
            }
        }
        #endregion
    }
}
