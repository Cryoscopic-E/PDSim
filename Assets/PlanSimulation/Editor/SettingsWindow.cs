using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

public class SettingsWindow : EditorWindow
{
    [SerializeField] private TextAsset domain;
    [SerializeField] private TextAsset problem;

    [SerializeField] private GameObject typeModels;

    [MenuItem("Plan Simulation/Settings")]
    public static void ShowWindow()
    {
        GetWindow<SettingsWindow>("Plan Settings");
    }

    private void OnGUI()
    {
        
        EditorGUILayout.BeginVertical();
        // Reset Button
        if(GUILayout.Button("Reset"))
        {
            domain = null;
            problem = null;
            Parser.Instance().Reset();
            typeModels = null;
            //Array.Clear(typeModels, 0, typeModels.Length);
        }

        // Domain and Problem inputs fields
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Domain File", EditorStyles.label);
        domain = (TextAsset) EditorGUILayout.ObjectField(domain, typeof(TextAsset), true);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Problem File", EditorStyles.label);
        problem = (TextAsset)EditorGUILayout.ObjectField(problem, typeof(TextAsset), true);
        EditorGUILayout.EndHorizontal();

        // Parsing button
        if(GUILayout.Button("Parse Domain And Problem"))
        {
            if(domain != null && problem != null)
            {
                Parser.Instance().ParseDomainProblem(domain.text, problem.text);
            }
        }

        EditorGUILayout.EndVertical();

        // check if parser has parsed the domain and problem files
        if (!Parser.Instance().IsParsed())
        {
            GUILayout.Label("Problem and Domain not Parsed!", EditorStyles.helpBox);
        }
        else
        {
            // Models input
            List<string> types = Parser.Instance().types;
            int modelCount = types.Count;
            GUILayout.Label("Types Model", EditorStyles.foldoutHeader);

            EditorGUILayout.BeginVertical();
            for (int i = 0; i < modelCount; i++)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(types[i], EditorStyles.boldLabel);
                typeModels = EditorGUILayout.ObjectField(typeModels, typeof(GameObject), true) as GameObject;
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();


            if(typeModels)
            {
                Transform objects = GameObject.Find("Objects").transform;
                if(GUILayout.Button("Create Scene"))
                {
                    foreach(string s in Parser.Instance().objects)
                    {
                        GameObject instance = (GameObject) PrefabUtility.InstantiatePrefab(typeModels);
                        instance.name = s;
                        instance.transform.parent = objects;
                    }
                }
            }
        }
    }
    protected void OnEnable()
    {
        string data = EditorPrefs.GetString("SimulationSettingsWindow", JsonUtility.ToJson(this, false));
        JsonUtility.FromJsonOverwrite(data, this);
    }
    protected void OnDisable()
    {
        string data = JsonUtility.ToJson(this, false);
        EditorPrefs.SetString("SimulationSettingsWindow", data);
    }
}
