using GeTPlan.Core.Models;
using GeTPlan.Core.Logic;
using GeTPlan.Core.Models.Expressions;
using PDSim.Utils;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PDSim.Components
{
    public class ProblemObjects : MonoBehaviour
    {
        //Singleton
        private static ProblemObjects _instance;
        public static ProblemObjects Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindAnyObjectByType<ProblemObjects>();
                return _instance;
            }
        }

        public delegate void VisualisationObjectHovered(VisualisationObject @object);
        public event VisualisationObjectHovered OnVisualisationObjectHovered;

        public delegate void VisualisationObjectUnhovered();
        public event VisualisationObjectUnhovered OnVisualisationObjectUnhovered;


        [SerializeField]
        public List<VisualisationObject> prefabs;

        private Dictionary<string, VisualisationObject> _objectDictionary;

        private Dictionary<string, List<string>> _typeToObjects;

        public List<string> GetObjectsOfType(string type)
        {
            if (_typeToObjects != null && _typeToObjects.ContainsKey(type))
                return _typeToObjects[type];
            return new List<string>();
        }


        private Dictionary<string, string> __objectToTypes;

        public string GetTypeOfObject(string objectName)
        {
            if (__objectToTypes != null && __objectToTypes.ContainsKey(objectName))
                return __objectToTypes[objectName];
            return null;
        }


        private void Awake()
        {
            _objectDictionary = new Dictionary<string, VisualisationObject>();
            _typeToObjects = new Dictionary<string, List<string>>();
            __objectToTypes = new Dictionary<string, string>();
        }

        private void Start()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                if (child.TryGetComponent(out VisualisationObject visualisationObject))
                {
                    if (!_objectDictionary.ContainsKey(child.name))
                        _objectDictionary.Add(child.name, visualisationObject);
                    
                    if (_typeToObjects.ContainsKey(visualisationObject.objectType))
                    {
                        _typeToObjects[visualisationObject.objectType].Add(child.name);
                    }
                    else
                    {
                        _typeToObjects.Add(visualisationObject.objectType, new List<string> { child.name });
                    }

                    if (!__objectToTypes.ContainsKey(child.name))
                        __objectToTypes.Add(child.name, visualisationObject.objectType);
                }
            }
        }

        public VisualisationObject GetObjectInScene(string objectName)
        {
            if (_objectDictionary != null && _objectDictionary.ContainsKey(objectName))
                return _objectDictionary[objectName];
            return null;
        }

        public void HoverObject(VisualisationObject @object)
        {
            OnVisualisationObjectHovered?.Invoke(@object);
        }

        public void ClearHover()
        {
            OnVisualisationObjectUnhovered?.Invoke();
        }


# if UNITY_EDITOR
        public void InitialiseComponent(List<PlanObject> objectDeclarations, List<string> allLeafType)
        {
            foreach (var type in allLeafType)
            {
                var folderPath = AssetUtils.GetSimulationObjectsPath(SceneManager.GetActiveScene().name);
                Object originalPrefab = (GameObject)AssetDatabase.LoadAssetAtPath(CommonPaths.PDSIM_OBJECT_PREFAB, typeof(GameObject));
                var prefabInstance = PrefabUtility.InstantiatePrefab(originalPrefab, null) as GameObject;
                prefabInstance.GetComponent<VisualisationObject>().objectType = type;
                var newModel = PrefabUtility.SaveAsPrefabAsset(prefabInstance, folderPath + "/" + type + ".prefab");

                prefabs.Add(newModel.GetComponent<VisualisationObject>());

                if (newModel.GetComponent<PDSimMetadata>() == null)
                {
                    newModel.AddComponent<PDSimMetadata>();
                }

                var sceneName = SceneManager.GetActiveScene().name;
                var sanitizedSceneName = System.Text.RegularExpressions.Regex.Replace(sceneName, @"[^a-zA-Z0-9_]", "");
                var behaviorFolder = AssetUtils.GetSimulationBehaviorsPath(sceneName);
                if (!System.IO.Directory.Exists(behaviorFolder))
                {
                    System.IO.Directory.CreateDirectory(behaviorFolder);
                }

                var className = $"{PDSimAPI.Generators.ActionScriptGenerator.ToPascalCase(type)}Behavior";
                var scriptPath = $"{behaviorFolder}/{className}.cs";

                if (!System.IO.File.Exists(scriptPath))
                {
                    var scriptContent = PDSimAPI.Generators.ObjectBehaviorGenerator.Generate(type, sanitizedSceneName);
                    System.IO.File.WriteAllText(scriptPath, scriptContent);
                    Debug.Log($"[PDSim] Generated behavior script for type '{type}': {scriptPath}");
                }

                DestroyImmediate(prefabInstance);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            foreach (var obj in objectDeclarations)
            {
                var type = obj.Type.Name;
                var prefabPath = $"{AssetUtils.GetSimulationObjectsPath(SceneManager.GetActiveScene().name)}/{type}.prefab";
                var prefab = AssetDatabase.LoadAssetAtPath<VisualisationObject>(prefabPath);

                var instance = PrefabUtility.InstantiatePrefab(prefab, transform) as VisualisationObject;
                instance.gameObject.name = obj.Name;
            }
        }
#endif
    }
}
