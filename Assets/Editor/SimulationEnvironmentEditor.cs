using System.IO;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(SimulationEnvironment))]
    public class SimulationEnvironmentEditor : UnityEditor.Editor
    {
        private SimulationEnvironment _simulationEnvironment;

        public override void OnInspectorGUI()
        {
            _simulationEnvironment = (SimulationEnvironment)target;
            DrawProblemField();
            GUILayout.Space(10);
            if (_simulationEnvironment.problem != null)
            {
                DrawModelTypes();
            }
        }

        private void DrawProblemField()
        {
            // ==========================
            // ======== PROBLEM =========
            // ==========================
        
            // Check if changed
            EditorGUI.BeginChangeCheck();
            // Problem File 
            EditorGUILayout.BeginVertical();
            GUILayout.Label("Problem File", EditorStyles.boldLabel);
            GUILayout.Space(5);
            // Draw domain filed
            EditorGUILayout.BeginHorizontal();
            var problem = (TextAsset) EditorGUILayout.ObjectField(_simulationEnvironment.problem, typeof(TextAsset), true);
            if (GUILayout.Button("Clear"))
            {
                problem = null;
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_simulationEnvironment, "Problem File Change");
                _simulationEnvironment.problem = problem;

                if (problem != null)
                {
                    Parser.ParseProblem(problem.text, 
                        out _simulationEnvironment.types, 
                        out _simulationEnvironment.objects, 
                        out _simulationEnvironment.initBlock);
        
                    // instantiate models array
                    _simulationEnvironment.typesModels = new GameObject[_simulationEnvironment.types.Count];
                    // instantiate objects transforms array
                    _simulationEnvironment.objectsPositions = new Vector3[_simulationEnvironment.objects.Count];
                }
            
                // save asset
                EditorUtility.SetDirty(_simulationEnvironment);
            }

            if (problem != null) return;
            _simulationEnvironment.plan = null;
            _simulationEnvironment.objects = null;
            _simulationEnvironment.types = null;
            _simulationEnvironment.typesModels = null;
            _simulationEnvironment.objectsPositions = null;
            _simulationEnvironment.initBlock = null;
        }
    
        private void DrawModelTypes()
        {
            EditorGUILayout.BeginVertical();
            {
                GUILayout.Label("Create Models for PDDL Types", EditorStyles.boldLabel);
                for (int i = 0; i < _simulationEnvironment.types.Count; i++)
                {
                    GUILayout.Space(10);
                    EditorGUILayout.BeginHorizontal();
                    {
                        DrawModelInput(i);
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawModelInput(int index)
        {
            // DRAW LABEL OF TYPE NAME
            GUILayout.Label(_simulationEnvironment.types[index].ToUpper(), EditorStyles.largeLabel);
            EditorGUI.BeginChangeCheck();
            // NEW MODEL BUTTON
            if (_simulationEnvironment.typesModels[index] == null)
            {
                if (GUILayout.Button("New Model", GUILayout.ExpandWidth(false)))
                {
                    _simulationEnvironment.typesModels[index] = CreateNewTypeModel(_simulationEnvironment.types[index]);
                }
            }
            else
            {
                // GET PREFAB FIELD
                _simulationEnvironment.typesModels[index] =
                    (GameObject) EditorGUILayout.ObjectField(_simulationEnvironment.typesModels[index], typeof(GameObject),
                        false);

                // EDIT PREFAB BUTTON
                if (GUILayout.Button("Edit", GUILayout.ExpandWidth(false)))
                {
                    if (!AssetDatabase.OpenAsset(_simulationEnvironment.typesModels[index]))
                    {
                        throw new UnityException("Can't Open Prefab");
                    }
                }
            }

            // CLEAR BUTTON
            if (GUILayout.Button("Clear", GUILayout.ExpandWidth(false)))
            {
                _simulationEnvironment.typesModels[index] = null;
            }

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(_simulationEnvironment);
            }
        }
    
        private GameObject CreateNewTypeModel(string typeName)
        {
            string folderPath = NewSimulationWindow.SIMULATIONS_BASE_DIR + _simulationEnvironment.simulationName + "/Types Models";
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            // Get the generic object prefab
            Object originalPrefab = (GameObject) AssetDatabase.LoadAssetAtPath("Assets/Prefabs/ObjectGeneric.prefab", typeof(GameObject));
            // Create instance of generic object
            var prefabInstance = PrefabUtility.InstantiatePrefab(originalPrefab, null) as GameObject;
            // Save new model
            var newModel = PrefabUtility.SaveAsPrefabAsset(prefabInstance, folderPath + "/" + typeName + ".prefab");
            // Remove from scene
            DestroyImmediate(prefabInstance);
            
            return newModel;
        }
    }
}
