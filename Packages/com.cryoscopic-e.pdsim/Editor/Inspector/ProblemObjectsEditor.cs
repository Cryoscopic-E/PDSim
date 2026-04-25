using PDSim.Components;
using UnityEditor;
using UnityEngine;

namespace PDSim.Editor.Inspector
{
    /// <summary>
    /// Custom inspector for the ProblemObjects component.
    /// </summary>
    [CustomEditor(typeof(ProblemObjects))]
    public class ProblemObjectsEditor : UnityEditor.Editor
    {
        #region Fields
        private ProblemObjects _problemObjects;
        #endregion

        #region Unity Lifecycle
        private void OnEnable()
        {
            _problemObjects = (ProblemObjects)target;
        }

        /// <summary>
        /// Draws the custom inspector GUI for ProblemObjects.
        /// </summary>
        public override void OnInspectorGUI()
        {
            GUILayout.Label("Problem Objects Customisation", EditorStyles.largeLabel);
            if (_problemObjects.Prefabs == null)
                return;
            for (var i = 0; i < _problemObjects.Prefabs.Count; ++i)
            {
                DrawModel(i);
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Draws the model and button to open the prefab attached.
        /// </summary>
        private void DrawModel(int index)
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            // MODEL NAME
            EditorGUILayout.LabelField(_problemObjects.Prefabs[index].name, EditorStyles.boldLabel);
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.ObjectField(_problemObjects.Prefabs[index], typeof(VisualisationObject), false);
            // EDIT PREFAB BUTTON
            if (GUILayout.Button("Edit", GUILayout.ExpandWidth(false)))
            {
                if (!AssetDatabase.OpenAsset(_problemObjects.Prefabs[index]))
                {
                    throw new UnityException("Can't Open Prefab");
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }
        #endregion
    }
}