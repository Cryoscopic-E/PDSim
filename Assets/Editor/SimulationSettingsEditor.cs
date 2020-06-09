using System.Collections.Generic;
using System.Net.Http;
using System.Security.Policy;
using System.Threading.Tasks;
using SimpleJSON;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SimulationSettings))]
public class SimulationSettingsEditor : Editor
{
    SimulationSettings simulationSettings;
    
    public override void OnInspectorGUI()
    {
        simulationSettings = (SimulationSettings)target;
        DrawSpace(10);
        DrawDomanProblemFields();
        
        if (simulationSettings.domain != null && simulationSettings.problem != null)
        {
            DrawSpace(10);
            DrawParseButton();
        }

        if (simulationSettings.pddlElements != null)
        {
            DrawSpace(15);
            DrawModelTypes();
            DrawSpace(15);
        }
    }
    public void DrawSpace(int pixels)
    {
        GUILayout.Space(pixels);
    }
    public void DrawDomanProblemFields()
    {


        EditorGUILayout.BeginHorizontal();
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginVertical();
            GUILayout.Label("Domain File", EditorStyles.boldLabel);
            TextAsset domain = (TextAsset)EditorGUILayout.ObjectField(simulationSettings.domain, typeof(TextAsset), true);
            EditorGUILayout.EndVertical();
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(simulationSettings,"Domain File Change");
                simulationSettings.domain = domain;
                EditorUtility.SetDirty(simulationSettings);
            }
            
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginVertical();
            GUILayout.Label("Problem File", EditorStyles.boldLabel);
            TextAsset problem = (TextAsset)EditorGUILayout.ObjectField(simulationSettings.problem, typeof(TextAsset), true);
            EditorGUILayout.EndVertical();
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(simulationSettings,"Problem File Change");
                simulationSettings.problem = problem;
                EditorUtility.SetDirty(simulationSettings);
            }
            
        }
        EditorGUILayout.EndHorizontal();
    }

    private void DrawParseButton()
    {
        if (GUILayout.Button("Parse Domain And Problem"))
        {
            // parse elements
            simulationSettings.pddlElements = Parser.ParseDomainAndProblem(simulationSettings.domain.text, simulationSettings.problem.text);
            // instantiate game objects array
            simulationSettings.typesModels = new GameObject[simulationSettings.pddlElements.types.Count];
            EditorUtility.SetDirty(simulationSettings);
        }
    }


    private void SavePlan(List<PlanAction> actions)
    {
        var plan = (Plan) ScriptableObject.CreateInstance(typeof(Plan));
        plan.actions = actions;
        EditorUtility.SetDirty(plan);
    }

    private void WarnUnsolvedPlan()
    {
        EditorUtility.DisplayDialog("Plan not found!",
            "It was impossible to solve the plan, check the domain and problem files.", "Ok");
    }
    
    
    private void DrawModelTypes()
    {

        //SerializedProperty meshesList = serializedObject.FindProperty("typesMeshes");
        if (simulationSettings.typesModels != null)
        {
            EditorGUILayout.BeginVertical();
            {
                GUILayout.Label("Create Model for Object Types", EditorStyles.boldLabel);
                for (int i = 0; i < simulationSettings.pddlElements.types.Count; i++)
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
    }
    
    private void DrawModelInput(int index)
    {
        // DRAW LABEL OF TYPE NAME
        GUILayout.Label(simulationSettings.pddlElements.types[index].ToUpper(), EditorStyles.largeLabel);
        EditorGUI.BeginChangeCheck();
        // NEW MODEL BUTTON
        if (simulationSettings.typesModels[index] == null)
        {
            if (GUILayout.Button("New Model"))
            {
                simulationSettings.typesModels[index] = CreateNewTypeModel(simulationSettings.name, simulationSettings.pddlElements.types[index]);
            }
        }
        else
        {
            // GET PREFAB FIELD
            simulationSettings.typesModels[index] = (GameObject)EditorGUILayout.ObjectField(simulationSettings.typesModels[index], typeof(GameObject), false);

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

        Object originalPrefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/Prefabs/ObjectGeneric.prefab", typeof(GameObject));
        GameObject prefabInstance = PrefabUtility.InstantiatePrefab(originalPrefab, null) as GameObject;
        GameObject newModel = PrefabUtility.SaveAsPrefabAsset(prefabInstance, folderPath + "/" + typeName + ".prefab");
        GameObject.DestroyImmediate(prefabInstance);
        return newModel;
        //throw new UnityException();
    }
}