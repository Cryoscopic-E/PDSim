using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Windows;

namespace Editor
{
    [CustomEditor(typeof(SimulationManager)), CanEditMultipleObjects]
    public class SimulationManagerEditor : UnityEditor.Editor
    {
        // private SimulationManager _simulationManager;
        //
        // public override void OnInspectorGUI()
        // {
        //     _simulationManager = (SimulationManager) target;
        //
        //     EditorGUILayout.BeginVertical();
        //     GUILayout.Label("Simulation Environment:");
        //     EditorGUI.BeginChangeCheck();
        //     var env = (SimulationEnvironment) EditorGUILayout.ObjectField(_simulationManager.simulationEnvironment, typeof(SimulationEnvironment), false);
        //     if (EditorGUI.EndChangeCheck())
        //     {
        //         if (env != null)
        //             _simulationManager.simulationEnvironment = env;
        //         EditorUtility.SetDirty(_simulationManager);
        //     }
        //     EditorGUILayout.EndVertical();
        //
        //     if (_simulationManager.simulationEnvironment != null)
        //     {
        //         if (GUILayout.Button("Generate Scene"))
        //         {
        //             _simulationManager.SetHolders();
        //             // all environment types defined
        //             var listDefined = _simulationManager.typesDefined.Select(t => _simulationManager.typesToDefine[t]).ToList();
        //             var typesEnvironment = _simulationManager.simulationEnvironment.types;
        //             var allTypesDefined = typesEnvironment.Intersect(listDefined).Count() == typesEnvironment.Count();
        //             if (allTypesDefined)
        //             {
        //                 if (!(_simulationManager.GetHolder(_simulationManager.SIM_OBJECT_HOLDER).childCount > 0))
        //                 {
        //                     _simulationManager.GenerateScene();
        //                 }
        //             }
        //             else
        //             {
        //                 EditorApplication.Beep();
        //                 EditorUtility.DisplayDialog("Types Models Missing","Please define all types Models", "Close");
        //             }
        //             
        //         }
        //         if (GUILayout.Button("Save Environment"))
        //         {
        //             _simulationManager.SaveEnvironment();
        //         }
        //     }
        //     else
        //     {
        //         EditorGUILayout.HelpBox("Set a simulation environment first!", MessageType.Warning);
        //     }
        //     
        //     GUILayout.Space(10);
        //     DrawDomainField();
        //     if (_simulationManager.domain == null) return;
        //     GUILayout.Space(15);
        //     DrawGenericObjectModels();
        //     GUILayout.Space(15);
        //     DrawPredicates();
        //
        // }
        //
        //  private void DrawDomainField()
        // {
        //     // =========================
        //     // ======== DOMAIN =========
        //     // =========================
        //
        //     // Check if changed
        //     EditorGUI.BeginChangeCheck();
        //     // Domain File 
        //     EditorGUILayout.BeginVertical();
        //     GUILayout.Label("Domain File", EditorStyles.boldLabel);
        //     GUILayout.Space(5);
        //     // Draw domain filed
        //     EditorGUILayout.BeginHorizontal();
        //     var domain = (TextAsset) EditorGUILayout.ObjectField(_simulationManager.domain, typeof(TextAsset), false);
        //     if (GUILayout.Button("Clear"))
        //     {
        //         domain = null;
        //     }
        //     EditorGUILayout.EndHorizontal();
        //     EditorGUILayout.EndVertical();
        //     if (EditorGUI.EndChangeCheck())
        //     {
        //         Undo.RecordObject(_simulationManager, "Domain File Change");
        //         _simulationManager.domain = domain;
        //         
        //         // save asset
        //         EditorUtility.SetDirty(_simulationManager);
        //     }
        // }
        //
        // private void DrawGenericObjectModels()
        // {
        //    GUILayout.Space( 5 );
        //    GUILayout.Label( "Models", EditorStyles.largeLabel);
        //
        //    for( var i = 0; i < _simulationManager.typesDefined.Count; ++i )
        //    {
        //        DrawModel( i );
        //    }
        //    // if all types have been defined don't shoe add button
        //    if (_simulationManager.typesDefined.Count < _simulationManager.typesToDefine.Count)
        //    { 
        //        DrawAddModelButton();
        //    }
        // }
        //
        // private void DrawModel(int index)
        // {
        //     // DRAW TYPE CHOICE
        //     EditorGUILayout.BeginHorizontal();
        //     EditorGUI.BeginChangeCheck();
        //     // popup to select types
        //     var selected = EditorGUILayout.Popup(_simulationManager.typesDefined[index],
        //         _simulationManager.typesToDefine.ToArray());
        //     // Drop down menu change
        //     if (EditorGUI.EndChangeCheck())
        //     {
        //         // check if defined type list contains the index
        //         if (_simulationManager.typesDefined.Contains(selected))
        //         {
        //             EditorApplication.Beep();
        //             EditorUtility.DisplayDialog("Type already defined",
        //                 "The type " + _simulationManager.typesToDefine[selected] + " is already defined", "Close");
        //         }
        //         else
        //         {
        //             _simulationManager.typesDefined[index] = selected;
        //             EditorUtility.SetDirty(_simulationManager);
        //         }
        //     }
        //     // DRAW MODEL FIELD OPTIONS
        //     EditorGUI.BeginChangeCheck();
        //     if (_simulationManager.typesGameObject[index] == null)
        //     {
        //         // NEW MODEL BUTTON
        //         if (GUILayout.Button("New Model", GUILayout.ExpandWidth(false)))
        //         {
        //             _simulationManager.typesGameObject[index] = CreateNewTypeModel(_simulationManager.typesToDefine[selected]);
        //         }
        //     }
        //     else
        //     {
        //         // GET PREFAB FIELD
        //         _simulationManager.typesGameObject[index] =
        //             (GameObject) EditorGUILayout.ObjectField(_simulationManager.typesGameObject[index], typeof(GameObject),
        //                 false);
        //
        //         // EDIT PREFAB BUTTON
        //         if (GUILayout.Button("Edit", GUILayout.ExpandWidth(false)))
        //         {
        //             if (!AssetDatabase.OpenAsset(_simulationManager.typesGameObject[index]))
        //             {
        //                 throw new UnityException("Can't Open Prefab");
        //             }
        //         }
        //     }
        //
        //     // CLEAR MODEL FIELD BUTTON
        //     if (GUILayout.Button("Clear Field", GUILayout.ExpandWidth(false)))
        //     {
        //         _simulationManager.typesGameObject[index] = null;
        //     }
        //     
        //     // REMOVE TYPE
        //     if (GUILayout.Button("-"))
        //     {
        //         _simulationManager.typesDefined.RemoveAt(index);
        //         _simulationManager.typesGameObject.RemoveAt(index);
        //         EditorUtility.SetDirty(_simulationManager);
        //     }
        //
        //     if (EditorGUI.EndChangeCheck())
        //     {
        //         EditorUtility.SetDirty(_simulationManager);
        //     }
        //     EditorGUILayout.EndHorizontal();
        // }
        //
        // private void DrawAddModelButton()
        // {
        //     if( GUILayout.Button( "Add Model", GUILayout.Height( 30 ) ) )
        //     {
        //         // if empty use first index
        //         if (_simulationManager.typesDefined.Count == 0)
        //         {
        //             _simulationManager.typesDefined.Add(0);
        //             _simulationManager.typesGameObject.Add(null);
        //         }
        //         else
        //         {
        //             var indexToAdd = -1;
        //             for (int i = 0; i < _simulationManager.typesToDefine.Count; ++i)
        //             {
        //                 if (_simulationManager.typesDefined.Contains(i)) continue;
        //                 indexToAdd = i;
        //                 break;
        //             }
        //
        //             if (indexToAdd != -1)
        //             {
        //                 _simulationManager.typesDefined.Add(indexToAdd);
        //                 _simulationManager.typesGameObject.Add(null);
        //             }
        //         }
        //         // save settings
        //         EditorUtility.SetDirty( _simulationManager );
        //     }
        // }
        //
        // private void DrawPredicates()
        // {
        //     EditorGUILayout.BeginVertical();
        //     
        //     GUILayout.Label("PDDL Predicates", EditorStyles.largeLabel);
        //     GUILayout.Space( 10 );
        //     for (int i = 0; i < _simulationManager.predicates.Count; i++)
        //     {
        //         // DRAW LABEL OF TYPE NAME
        //         EditorGUILayout.BeginHorizontal();
        //         GUILayout.Label(_simulationManager.predicates[i].predicateName.ToUpper(), EditorStyles.boldLabel,GUILayout.ExpandWidth(false));
        //         GUILayout.Label(_simulationManager.predicates[i].parametersDescription,EditorStyles.miniLabel, GUILayout.ExpandWidth(false));
        //         EditorGUILayout.EndHorizontal();
        //         EditorGUILayout.BeginVertical();
        //         DrawBehavioursInput(i);
        //         EditorGUILayout.EndVertical();
        //         // ADD NEW BEHAVIOR BUTTON
        //         if (GUILayout.Button("Add New Behavior"))
        //         {
        //             _simulationManager.predicates[i].predicateCommandSettings.Add(new PredicateCommandSettings(null));
        //         }
        //         GUILayout.Space( 15 );
        //     }
        //     EditorGUILayout.EndVertical();
        // }
        //
        // private void DrawBehavioursInput(int index)
        // {
        //     // DRAW PDDL DOMAIN PREDICATE SETTINGS FIELD
        //     
        //         for (int i = 0; i < _simulationManager.predicates[index].predicateCommandSettings.Count; i++)
        //         {
        //             var commandSettings = _simulationManager.predicates[index].predicateCommandSettings[i];
        //             EditorGUILayout.BeginHorizontal();
        //             // TYPE
        //             EditorGUI.BeginChangeCheck();
        //             
        //             var selectedParameterTypeIndex = EditorGUILayout.Popup(commandSettings.predicateTypeIndex,
        //                 _simulationManager.predicates[index].parametersTypes.ToArray());
        //             if (EditorGUI.EndChangeCheck())
        //             {
        //                 Undo.RecordObject(_simulationManager, "Predicate Command Change");
        //                 commandSettings.predicateTypeIndex = selectedParameterTypeIndex;
        //                 EditorUtility.SetDirty(_simulationManager);
        //             }
        //             
        //             // COMMAND BEHAVIOUR
        //             EditorGUI.BeginChangeCheck();
        //             var behavior = (CommandBase)EditorGUILayout.ObjectField(commandSettings.commandBaseBehavior, typeof(CommandBase),
        //                 false, GUILayout.ExpandWidth(false));
        //             if (EditorGUI.EndChangeCheck())
        //             {
        //                 if (behavior != null)
        //                 {
        //                     var behaviourBaseType = behavior.GetType().BaseType;
        //                     var expectedParametersCount = _simulationManager.predicates[index].parameters.Count;
        //                     if (behaviourBaseType != null)
        //                     {
        //                         switch (behaviourBaseType.Name)
        //                         {
        //                             case "OneAttributeCommand":
        //                                 if(!CheckNumberAttributesCommand(expectedParametersCount, 1))
        //                                     behavior = null;
        //                                 break;
        //                             case "TwoAttributeCommand":
        //                                 if(!CheckNumberAttributesCommand(expectedParametersCount, 2))
        //                                     behavior = null;
        //                                 break;
        //                             case "ThreeAttributeCommand":
        //                                 if(!CheckNumberAttributesCommand(expectedParametersCount, 3))
        //                                     behavior = null;
        //                                 break;
        //                             case "CommandBase":
        //                                 Alert("Change Base class to N_AttributeCommand");
        //                                 behavior = null;
        //                                 break;
        //                         }
        //                     }
        //                 }
        //                 
        //                 Undo.RecordObject(_simulationManager, "Predicate Command Change");
        //                 commandSettings.commandBaseBehavior = behavior;
        //                 EditorUtility.SetDirty(_simulationManager);
        //             }
        //             
        //             // EXECUTION ORDER
        //             EditorGUI.BeginChangeCheck();
        //             var order = EditorGUILayout.IntField(commandSettings.orderOfExecution, GUILayout.ExpandWidth(false));
        //             if (EditorGUI.EndChangeCheck())
        //             {
        //                 Undo.RecordObject(_simulationManager, "Predicate Command Execution Order Change");
        //                 commandSettings.orderOfExecution = order;
        //                 EditorUtility.SetDirty(_simulationManager);
        //             }
        //             if (GUILayout.Button("-"))
        //             {
        //                 _simulationManager.predicates[index].predicateCommandSettings.RemoveAt(i);
        //                 EditorUtility.SetDirty(_simulationManager);
        //             }
        //             EditorGUILayout.EndHorizontal();
        //         }
        //         
        //     
        //     
        // }
        //
        // private bool CheckNumberAttributesCommand(int expected, int provided)
        // {
        //     if (expected == provided) return true;
        //     Alert("Wrong number of attributes in defined command behaviour, expected: " + expected + ", found: " +provided);
        //     return false;
        // }
        // private static void Alert(string message)
        // {
        //     EditorApplication.Beep();
        //     EditorUtility.DisplayDialog("Wrong Command",message, "Close");
        // }
        // private GameObject CreateNewTypeModel(string typeName)
        // {
        //     var folderPath = NewSimulationWindow.SimulationsBaseDir + _simulationManager.simulationName + "/Types Models";
        //     if (!Directory.Exists(folderPath))
        //     {
        //         Directory.CreateDirectory(folderPath);
        //     }
        //     // Get the generic object prefab
        //     Object originalPrefab = (GameObject) AssetDatabase.LoadAssetAtPath("Assets/Prefabs/ObjectGeneric.prefab", typeof(GameObject));
        //     // Create instance of generic object
        //     var prefabInstance = PrefabUtility.InstantiatePrefab(originalPrefab, null) as GameObject;
        //     // Save new model
        //     var newModel = PrefabUtility.SaveAsPrefabAsset(prefabInstance, folderPath + "/" + typeName + ".prefab");
        //     // Remove from scene
        //     DestroyImmediate(prefabInstance);
        //     
        //     return newModel;
        // }
    }
}