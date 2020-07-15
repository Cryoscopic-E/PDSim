using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Windows;

namespace Editor
{
    [CustomEditor(typeof(SimulationSettings))]
    public class SimulationSettingsEditor : UnityEditor.Editor
    {
        private SimulationSettings _simulationSettings;

        public override void OnInspectorGUI()
        {
            _simulationSettings = (SimulationSettings) target;
            GUILayout.Space(10);
            DrawDomainField();

            if (_simulationSettings.domain != null)
            {
                GUILayout.Space(15);
                DrawGenericObjectModels();
                GUILayout.Space(15);
                DrawPredicates();
            }
        }

        private void DrawDomainField()
        {
            // =========================
            // ======== DOMAIN =========
            // =========================
        
            // Check if changed
            EditorGUI.BeginChangeCheck();
            // Domain File 
            EditorGUILayout.BeginVertical();
            GUILayout.Label("Domain File", EditorStyles.boldLabel);
            GUILayout.Space(5);
            // Draw domain filed
            EditorGUILayout.BeginHorizontal();
            var domain = (TextAsset) EditorGUILayout.ObjectField(_simulationSettings.domain, typeof(TextAsset), true);
            if (GUILayout.Button("Clear"))
            {
                domain = null;
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_simulationSettings, "Domain File Change");
                _simulationSettings.domain = domain;

                if (domain != null)
                {
                    // parse elements
                    Parser.ParseDomain(
                        _simulationSettings.domain.text,
                        out _simulationSettings.types,
                        out _simulationSettings.predicates,
                        out  _simulationSettings.actions);
                }
                // Create list of types string to define from original list of types
                _simulationSettings.typesToDefine = new List<string>();
                foreach (var type in _simulationSettings.types)
                {
                    _simulationSettings.typesToDefine.Add(type.typeName);
                }
                // Create Empty list of defined types indexes(to help showing models in the inspector)
                _simulationSettings.typesDefined = new List<int>();
                // Create Empty list of game objects representing the models
                _simulationSettings.typesGameObject = new List<GameObject>();
                
                // save asset
                EditorUtility.SetDirty(_simulationSettings);
            }
            
            // ==============================
            // CHECK FILES AFTER MODIFICATION
            // ==============================
            if (domain != null) return;
            _simulationSettings.actions = null;
            _simulationSettings.predicates = null;
            _simulationSettings.types = null;
            _simulationSettings.typesDefined = null;
            _simulationSettings.typesToDefine = null;
            _simulationSettings.typesGameObject = null;
        }

        private void DrawGenericObjectModels()
        {
           GUILayout.Space( 5 );
           GUILayout.Label( "Models", EditorStyles.largeLabel);

           for( var i = 0; i < _simulationSettings.typesDefined.Count; ++i )
           {
               DrawModel( i );
           }
           // if all types have been defined don't shoe add button
           if (_simulationSettings.typesDefined.Count < _simulationSettings.typesToDefine.Count)
           { 
               DrawAddModelButton();
           }
        }

        private void DrawModel(int index)
        {
            // DRAW TYPE CHOICE
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            // popup to select types
            var selected = EditorGUILayout.Popup(_simulationSettings.typesDefined[index],
                _simulationSettings.typesToDefine.ToArray());
            // Drop down menu change
            if (EditorGUI.EndChangeCheck())
            {
                // check if defined type list contains the index
                if (_simulationSettings.typesDefined.Contains(selected))
                {
                    EditorApplication.Beep();
                    EditorUtility.DisplayDialog("Type already defined",
                        "The type " + _simulationSettings.typesToDefine[selected] + " is already defined", "Close");
                }
                else
                {
                    _simulationSettings.typesDefined[index] = selected;
                    EditorUtility.SetDirty(_simulationSettings);
                }
            }
            // DRAW MODEL FIELD OPTIONS
            EditorGUI.BeginChangeCheck();
            if (_simulationSettings.typesGameObject[index] == null)
            {
                // NEW MODEL BUTTON
                if (GUILayout.Button("New Model", GUILayout.ExpandWidth(false)))
                {
                    _simulationSettings.typesGameObject[index] = CreateNewTypeModel(_simulationSettings.typesToDefine[selected]);
                }
            }
            else
            {
                // GET PREFAB FIELD
                _simulationSettings.typesGameObject[index] =
                    (GameObject) EditorGUILayout.ObjectField(_simulationSettings.typesGameObject[index], typeof(GameObject),
                        false);

                // EDIT PREFAB BUTTON
                if (GUILayout.Button("Edit", GUILayout.ExpandWidth(false)))
                {
                    if (!AssetDatabase.OpenAsset(_simulationSettings.typesGameObject[index]))
                    {
                        throw new UnityException("Can't Open Prefab");
                    }
                }
            }

            // CLEAR MODEL FIELD BUTTON
            if (GUILayout.Button("Clear Field", GUILayout.ExpandWidth(false)))
            {
                _simulationSettings.typesGameObject[index] = null;
            }
            
            // REMOVE TYPE
            if (GUILayout.Button("-"))
            {
                _simulationSettings.typesDefined.RemoveAt(index);
                _simulationSettings.typesGameObject.RemoveAt(index);
                EditorUtility.SetDirty(_simulationSettings);
            }

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(_simulationSettings);
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawAddModelButton()
        {
            if( GUILayout.Button( "Add Model", GUILayout.Height( 30 ) ) )
            {
                // if empty use first index
                if (_simulationSettings.typesDefined.Count == 0)
                {
                    _simulationSettings.typesDefined.Add(0);
                    _simulationSettings.typesGameObject.Add(null);
                }
                else
                {
                    var indexToAdd = -1;
                    for (int i = 0; i < _simulationSettings.typesToDefine.Count; ++i)
                    {
                        if (_simulationSettings.typesDefined.Contains(i)) continue;
                        indexToAdd = i;
                        break;
                    }

                    if (indexToAdd != -1)
                    {
                        _simulationSettings.typesDefined.Add(indexToAdd);
                        _simulationSettings.typesGameObject.Add(null);
                    }
                }
                // save settings
                EditorUtility.SetDirty( _simulationSettings );
            }
        }
        
        private void DrawPredicates()
        {
            EditorGUILayout.BeginVertical();
            
            GUILayout.Label("PDDL Predicates", EditorStyles.largeLabel);
            GUILayout.Space( 10 );
            for (int i = 0; i < _simulationSettings.predicates.Count; i++)
            {
                // DRAW LABEL OF TYPE NAME
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(_simulationSettings.predicates[i].predicateName.ToUpper(), EditorStyles.boldLabel,GUILayout.ExpandWidth(false));
                GUILayout.Label(_simulationSettings.predicates[i].parametersDescription,EditorStyles.miniLabel, GUILayout.ExpandWidth(false));
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginVertical();
                DrawBehavioursInput(i);
                EditorGUILayout.EndVertical();
                // ADD NEW BEHAVIOR BUTTON
                if (GUILayout.Button("Add New Behavior"))
                {
                    _simulationSettings.predicates[i].predicateCommandSettings.Add(new PredicateCommandSettings(null));
                }
                GUILayout.Space( 15 );
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawBehavioursInput(int index)
        {
            // DRAW PDDL DOMAIN PREDICATE SETTINGS FIELD
            
                for (int i = 0; i < _simulationSettings.predicates[index].predicateCommandSettings.Count; i++)
                {
                    var commandSettings = _simulationSettings.predicates[index].predicateCommandSettings[i];
                    EditorGUILayout.BeginHorizontal();
                    // TYPE
                    EditorGUI.BeginChangeCheck();
                    var selectedParameterTypeIndex = EditorGUILayout.Popup(commandSettings.predicateTypeIndex,
                        _simulationSettings.predicates[index].parametersTypes.ToArray());
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(_simulationSettings, "Predicate Command Change");
                        commandSettings.predicateTypeIndex = selectedParameterTypeIndex;
                        EditorUtility.SetDirty(_simulationSettings);
                    }
                    
                    // COMMAND BEHAVIOUR
                    EditorGUI.BeginChangeCheck();
                    var behavior = (PredicateCommand)EditorGUILayout.ObjectField(commandSettings.commandBehavior, typeof(PredicateCommand),
                        false, GUILayout.ExpandWidth(false));
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(_simulationSettings, "Predicate Command Change");
                        commandSettings.commandBehavior = behavior;
                        EditorUtility.SetDirty(_simulationSettings);
                    }
                    
                    // EXECUTION ORDER
                    EditorGUI.BeginChangeCheck();
                    var order = EditorGUILayout.IntField(commandSettings.orderOfExecution, GUILayout.ExpandWidth(false));
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(_simulationSettings, "Predicate Command Execution Order Change");
                        commandSettings.orderOfExecution = order;
                        EditorUtility.SetDirty(_simulationSettings);
                    }
                    if (GUILayout.Button("-"))
                    {
                        _simulationSettings.predicates[index].predicateCommandSettings.RemoveAt(i);
                        EditorUtility.SetDirty(_simulationSettings);
                    }
                    EditorGUILayout.EndHorizontal();
                }
                
            
            
        }

        private GameObject CreateNewTypeModel(string typeName)
        {
            var folderPath = NewSimulationWindow.SIMULATIONS_BASE_DIR + _simulationSettings.simulationName + "/Types Models";
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