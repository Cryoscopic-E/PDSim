using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Plan))]
public class PlanEditor : Editor
{
    private Plan _plan;

    public override void OnInspectorGUI()
    {
        _plan = target as Plan;
        if (_plan == null) return;
        
        GUILayout.BeginVertical();
        foreach (var action in _plan.actions)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(action.name, EditorStyles.largeLabel);
            GUILayout.Label(action.Parameters(), EditorStyles.label);
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();
    }
}