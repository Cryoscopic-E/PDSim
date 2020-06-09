using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SimulationSettings))]
public class SimulationSettingsEditor : Editor
{
    SimulationSettings simulationSettings;

    public override void OnInspectorGUI()
    {
        simulationSettings = (SimulationSettings) target;
        DrawSpace(10);
        DrawDomainField();

        if (simulationSettings.domain != null)
        {
            DrawSpace(15);
            DrawModelTypes();
            DrawSpace(15);
            DrawActions();
        }
    }

    private static void DrawSpace(int pixels)
    {
        GUILayout.Space(pixels);
    }

    private void DrawDomainField()
    {
        GUILayout.Label("Domain File", EditorStyles.boldLabel);
        DrawSpace(5);
        EditorGUILayout.BeginHorizontal();
        {
            
            EditorGUI.BeginChangeCheck();


            TextAsset domain =
                (TextAsset) EditorGUILayout.ObjectField(simulationSettings.domain, typeof(TextAsset), true);

            if (GUILayout.Button("Clear"))
            {
                domain = null;
            }
            
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(simulationSettings, "Domain File Change");
                simulationSettings.domain = domain;
                if (domain == null)
                {
                    simulationSettings.domainElements = null;
                    simulationSettings.typesModels = null;
                }
                else
                {
                    // parse elements
                    simulationSettings.domainElements =
                        Parser.ParseDomain(simulationSettings.domain.text); //, simulationSettings.problem.text);
                    // instantiate game objects array
                    simulationSettings.typesModels = new GameObject[simulationSettings.domainElements.types.Count];
                }

                // save asset
                EditorUtility.SetDirty(simulationSettings);
            }

            // EditorGUI.BeginChangeCheck();
            // EditorGUILayout.BeginVertical();
            // GUILayout.Label("Problem File", EditorStyles.boldLabel);
            // TextAsset problem = (TextAsset)EditorGUILayout.ObjectField(simulationSettings.problem, typeof(TextAsset), true);
            // EditorGUILayout.EndVertical();
            // if (EditorGUI.EndChangeCheck())
            // {
            //     Undo.RecordObject(simulationSettings,"Problem File Change");
            //     simulationSettings.problem = problem;
            //     EditorUtility.SetDirty(simulationSettings);
            // }
        }
        EditorGUILayout.EndHorizontal();
    }

    private void DrawActions()
    {
        EditorGUILayout.BeginVertical();
        {
            GUILayout.Label("PDDL Actions", EditorStyles.boldLabel);
            for (int i = 0; i < simulationSettings.domainElements.actions.Count; i++)
            {
                DrawSpace(10);
                GUILayout.Label(simulationSettings.domainElements.actions[i].name);
            }
        }
        EditorGUILayout.EndVertical();
    }

    private void DrawModelTypes()
    {
        EditorGUILayout.BeginVertical();
        {
            GUILayout.Label("Create Models for PDDL Types", EditorStyles.boldLabel);
            for (int i = 0; i < simulationSettings.domainElements.types.Count; i++)
            {
                DrawSpace(10);
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
        GUILayout.Label(simulationSettings.domainElements.types[index].ToUpper(), EditorStyles.largeLabel);
        EditorGUI.BeginChangeCheck();
        // NEW MODEL BUTTON
        if (simulationSettings.typesModels[index] == null)
        {
            if (GUILayout.Button("New Model"))
            {
                simulationSettings.typesModels[index] = CreateNewTypeModel(simulationSettings.name,
                    simulationSettings.domainElements.types[index]);
            }
        }
        else
        {
            // GET PREFAB FIELD
            simulationSettings.typesModels[index] =
                (GameObject) EditorGUILayout.ObjectField(simulationSettings.typesModels[index], typeof(GameObject),
                    false);

            // EDIT PREFAB BUTTON
            if (GUILayout.Button("Edit"))
            {
                if (!AssetDatabase.OpenAsset(simulationSettings.typesModels[index]))
                {
                    throw new UnityException("Can't Open Prefab");
                }
            }
        }

        // CLEAR BUTTON
        if (GUILayout.Button("Clear"))
        {
            simulationSettings.typesModels[index] = null;
        }

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(simulationSettings);
        }
    }

    private static GameObject CreateNewTypeModel(string simulationName, string typeName)
    {
        string folderPath = "Assets/Prefabs/" + simulationName.Replace(" ", string.Empty) + "Models";
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets/Prefabs", simulationName.Replace(" ", string.Empty) + "Models");
        }

        // UnityEngine.Object prefab = Resources.Load("Assets/Prefabs/ObjectGeneric");
        // GameObject prefabInstance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        // GameObject newModel = PrefabUtility.SaveAsPrefabAsset(prefabInstance, modelsPath);

        Object originalPrefab =
            (GameObject) AssetDatabase.LoadAssetAtPath("Assets/Prefabs/ObjectGeneric.prefab", typeof(GameObject));
        GameObject prefabInstance = PrefabUtility.InstantiatePrefab(originalPrefab, null) as GameObject;
        GameObject newModel = PrefabUtility.SaveAsPrefabAsset(prefabInstance, folderPath + "/" + typeName + ".prefab");
        GameObject.DestroyImmediate(prefabInstance);
        return newModel;
        //throw new UnityException();
    }
}