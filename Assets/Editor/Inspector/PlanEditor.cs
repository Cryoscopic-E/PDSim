using UnityEditor;

namespace Editor.Inspector
{
    /// <summary>
    /// Custom inspector for the Plan class.
    /// </summary>
    [CustomEditor(typeof(Plan))]
    public class PlanEditor : UnityEditor.Editor
    {
        //private Plan _plan;

        //private void OnEnable()
        //{
        //    _plan = (Plan)target;
        //}

        //public override void OnInspectorGUI()
        //{
        //    var plan = _plan.actions;
        //    EditorGUILayout.BeginVertical();

        //    foreach (var action in plan)
        //    {
        //        EditorGUILayout.LabelField(action.name, EditorStyles.boldLabel);
        //        EditorGUI.indentLevel++;
        //        EditorGUILayout.LabelField("Parameters:", EditorStyles.linkLabel);
        //        EditorGUILayout.BeginVertical();
        //        // join the parameters into a single string
        //        var parameters = string.Join(", ", action.parameters);
        //        EditorGUI.indentLevel++;
        //        EditorGUILayout.LabelField(parameters, EditorStyles.largeLabel, GUILayout.ExpandWidth(false));
        //        EditorGUI.indentLevel--;
        //        EditorGUILayout.EndVertical();
        //        EditorGUI.indentLevel--;
        //        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        //        GUILayout.Space(10);
        //    }

        //    EditorGUILayout.EndVertical();
        //}
    }
}