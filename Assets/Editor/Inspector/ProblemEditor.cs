using PDSim.Simulation.Data;
using UnityEditor;
using UnityEngine;

namespace Editor.Inspector
{
	[CustomEditor(typeof(Problem))]
	[CanEditMultipleObjects]
	public class ProblemEditor : UnityEditor.Editor
	{
		private Problem _problem;

		private void OnEnable()
		{
			_problem = (Problem)target;
		}

		public override void OnInspectorGUI()
		{
			EditorGUILayout.BeginVertical();

			var initialState = _problem.initialState;
			var goalState = _problem.goalState;
			var objects = _problem.objects;

			EditorGUILayout.LabelField("INITIAL STATE", EditorStyles.boldLabel);
			EditorGUI.indentLevel++;
			EditorGUILayout.BeginVertical();
			foreach (var fluent in initialState)
			{
				EditorGUILayout.LabelField(fluent.ToString(), EditorStyles.largeLabel, GUILayout.ExpandWidth(false));
			}

			EditorGUILayout.EndVertical();
			EditorGUI.indentLevel--;
			
			
			EditorGUILayout.LabelField("GOAL STATE", EditorStyles.boldLabel);
			EditorGUI.indentLevel++;
			EditorGUILayout.BeginVertical();
			foreach (var fluent in goalState)
			{
				EditorGUILayout.LabelField(fluent.ToString(), EditorStyles.largeLabel, GUILayout.ExpandWidth(false));
			}
			EditorGUILayout.EndVertical();
			EditorGUI.indentLevel--;
			
			EditorGUILayout.LabelField("OBJECTS", EditorStyles.boldLabel);
			EditorGUI.indentLevel++;
			EditorGUILayout.BeginVertical();
			foreach (var obj in objects)
			{
				EditorGUILayout.LabelField(obj.ToString(), EditorStyles.largeLabel, GUILayout.ExpandWidth(false));
			}
			EditorGUILayout.EndVertical();
			EditorGUI.indentLevel--;

			EditorGUILayout.EndVertical();
		}
	}
}