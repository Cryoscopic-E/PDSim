using PDSim.Simulation.Data;
using UnityEditor;
using UnityEngine;

namespace Editor.Inspector
{
    /// <summary>
    /// Custom inspector for the Actions class.
    /// </summary>
    [CustomEditor(typeof(Actions))]
    public class ActionsEditor : UnityEditor.Editor
    {
        private Actions _actions;

        private void OnEnable()
        {
            _actions = (Actions)target;
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.BeginVertical();

            foreach (var action in _actions.actions)
            {
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField(action.name, EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField("Parameters:", EditorStyles.linkLabel);
                EditorGUI.indentLevel++;
                EditorGUILayout.BeginVertical();
                foreach (var parameter in action.parameters)
                {
                    EditorGUILayout.LabelField(parameter.ToString(), EditorStyles.largeLabel, GUILayout.ExpandWidth(false));
                }
                EditorGUILayout.EndVertical();
                EditorGUI.indentLevel--;
                EditorGUILayout.LabelField("Effects:", EditorStyles.linkLabel);
                EditorGUI.indentLevel++;
                EditorGUILayout.BeginVertical();
                // TODO: change this to a reorderable list
                foreach (var effect in action.effects)
                {
                    EditorGUILayout.LabelField(effect.ToString(), EditorStyles.largeLabel, GUILayout.ExpandWidth(false));
                }
                EditorGUILayout.EndVertical();
                EditorGUI.indentLevel--;
                EditorGUI.indentLevel--;
                EditorGUILayout.EndVertical();
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                GUILayout.Space(10);
            }

            EditorGUILayout.EndVertical();
        }
    }
}