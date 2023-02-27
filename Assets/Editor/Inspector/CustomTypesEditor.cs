using PDSim.Components;
using PDSim.Simulation.Data;
using UnityEditor;
using UnityEngine;

namespace Editor.Inspector
{
	[CustomEditor(typeof(CustomTypes))]
	public class CustomTypesEditor : UnityEditor.Editor
	{
		private CustomTypes _customTypes;
		
		private void OnEnable()
		{
			_customTypes = (CustomTypes) target;
		}
		
		public override void OnInspectorGUI()
		{
			EditorGUILayout.BeginVertical();
			DrawNodes(_customTypes.typeTree.GetRoot());
			EditorGUILayout.EndVertical();
		}
		
		private void DrawNodes(PdTypeTree.TypeNode node, int depth =  0)
		{
			if (node == null)
				return;
			var label = string.Empty;
			for (var i = 0; i < depth; i++)
			{
				label += "\t";
			}
			if (depth > 0)
			{
				label += '\u221F'.ToString();
			}

			if (node.children.Count > 0)
			{
				GUILayout.Label(label + node.Name);
				var children = node.children;
				foreach (var c in children)
				{
					DrawNodes(c, depth +1);
				}
			}
			else
			{
				GUILayout.Label(label + node.Name, EditorStyles.linkLabel);
			}
		}
	}
}