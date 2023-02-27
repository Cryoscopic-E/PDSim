using PDSim.Simulation.Data;
using UnityEditor;
using UnityEngine;

namespace Editor.Inspector
{
	[CustomEditor(typeof(Fluents))]
	public class FluentsEditor : UnityEditor.Editor
	{
		private Fluents _fluents;
		
		private void OnEnable()
		{
			_fluents = (Fluents) target;
		}
		
		public override void OnInspectorGUI()
		{
			var fluents = _fluents.fluents;
			EditorGUILayout.BeginVertical();
			
				foreach (var fluent in fluents)
				{
					EditorGUILayout.BeginVertical();
						EditorGUILayout.LabelField(fluent.name, EditorStyles.boldLabel);
						EditorGUI.indentLevel++;
						EditorGUILayout.BeginVertical();
						foreach (var param in fluent.parameters)
						{
							EditorGUILayout.LabelField(param.ToString(), EditorStyles.largeLabel, GUILayout.ExpandWidth(false));
						}
						EditorGUILayout.EndVertical();
						EditorGUI.indentLevel--;
					EditorGUILayout.EndVertical();
				}
			
			EditorGUILayout.EndVertical();
		}
	}
}