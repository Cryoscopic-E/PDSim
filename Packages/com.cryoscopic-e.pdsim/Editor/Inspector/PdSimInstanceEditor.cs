using PDSim.Simulation;
using PDSim.PlanningModel;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace PDSim.Editor.Inspector
{
    /// <summary>
    /// Custom inspector for the Problem class.
    /// </summary>
    [CustomEditor(typeof(PdSimInstance))]
    public class PdSimInstanceEditor : UnityEditor.Editor
    {
        private PdSimInstance _instance;
        private ReorderableList _objectsList;
        private ReorderableList _initList;


        private void OnEnable()
        {
            _instance = (PdSimInstance)target;

            _objectsList = new ReorderableList(serializedObject, serializedObject.FindProperty("objects"), true, false, false, false);

            _objectsList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var objectSting = _instance.objects[index].ToString();

                EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), objectSting);
            };

            _initList = new ReorderableList(serializedObject, serializedObject.FindProperty("init"), true, false, false, false);

            _initList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var initString = _instance.init[index].ToString();

                EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), initString);
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.BeginVertical();
            // OBJECTS
            var objects = _instance.objects;

            EditorGUILayout.LabelField("OBJECTS", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.BeginVertical();
            _objectsList.DoLayoutList();
            EditorGUILayout.EndVertical();
            EditorGUI.indentLevel--;

            EditorGUILayout.EndVertical();

            // INIT
            var init = _instance.init;
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField("INITIAL STATE", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.BeginVertical();
            _initList.DoLayoutList();
            EditorGUILayout.EndVertical();
            EditorGUI.indentLevel--;
            EditorGUI.indentLevel--;

            EditorGUILayout.Space();

            serializedObject.ApplyModifiedProperties();

            // PLAN
            var plan = _instance.plan;
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField("PLAN", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.BeginVertical();
            foreach (var action in plan)
            {
                EditorGUILayout.LabelField(action.ToString(), EditorStyles.largeLabel);
            }
            EditorGUILayout.EndVertical();
            EditorGUI.indentLevel--;
        }

        private void DrawNodes(PdSimTypesDeclaration.TypeNode node, int depth = 0)
        {
            if (node == null)
                return;
            var label = string.Empty;
            // Add tabs for each depth
            for (var i = 0; i < depth; i++)
            {
                label += "\t";
            }
            // Add a branching symbol for each depth
            if (depth > 0)
            {
                label += '\u221F'.ToString();
            }
            // Recursively draw the tree
            if (node.children.Count > 0)
            {
                GUILayout.Label(label + node.Name);
                var children = node.children;
                foreach (var c in children)
                {
                    DrawNodes(c, depth + 1);
                }
            }
            else
            {
                GUILayout.Label(label + node.Name, EditorStyles.linkLabel);
            }
        }
    }
}