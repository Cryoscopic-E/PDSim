using GeTPlan.Core.Models;
using PDSim.Utils;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PDSim.Components
{
    /// <summary>
    /// Manages the collection of objects in the planning problem and their mapping to scene GameObjects.
    /// Provides functionality for object lookup and prefab management.
    /// </summary>
    public class ProblemObjects : MonoBehaviour
    {
        #region Public API

        /// <summary>
        /// Singleton instance of the ProblemObjects manager.
        /// </summary>
        public static ProblemObjects Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindAnyObjectByType<ProblemObjects>();
                return _instance;
            }
        }

        /// <summary>
        /// Delegate for object hover events.
        /// </summary>
        public delegate void VisualisationObjectHovered(VisualisationObject @object);
        /// <summary>
        /// Event fired when a visualization object is hovered.
        /// </summary>
        public event VisualisationObjectHovered OnVisualisationObjectHovered;

        /// <summary>
        /// Delegate for object unhover events.
        /// </summary>
        public delegate void VisualisationObjectUnhovered();
        /// <summary>
        /// Event fired when a visualization object is no longer hovered.
        /// </summary>
        public event VisualisationObjectUnhovered OnVisualisationObjectUnhovered;

        /// <summary>
        /// The list of visualization object prefabs available for instantiation.
        /// </summary>
        [SerializeField]
        [Tooltip("The list of visualization object prefabs.")]
        public List<VisualisationObject> Prefabs;

        /// <summary>
        /// Retrieves all object names of a given type.
        /// </summary>
        /// <param name="type">The type name.</param>
        /// <returns>A list of object names.</returns>
        public List<string> GetObjectsOfType(string type)
        {
            if (_typeToObjects != null && _typeToObjects.TryGetValue(type, out var list))
                return list;
            return new List<string>();
        }

        /// <summary>
        /// Retrieves the type of a specific object by its name.
        /// </summary>
        /// <param name="objectName">The name of the object.</param>
        /// <returns>The type name, or null if not found.</returns>
        public string GetTypeOfObject(string objectName)
        {
            if (_objectToTypes != null && _objectToTypes.TryGetValue(objectName, out var type))
                return type;
            return null;
        }

        /// <summary>
        /// Retrieves a visualization object in the scene by its name.
        /// </summary>
        /// <param name="objectName">The name of the object.</param>
        /// <returns>The VisualisationObject component, or null if not found.</returns>
        public VisualisationObject GetObjectInScene(string objectName)
        {
            if (_objectDictionary != null && _objectDictionary.TryGetValue(objectName, out var obj))
                return obj;
            return null;
        }

        /// <summary>
        /// Triggers the hover event for an object.
        /// </summary>
        /// <param name="object">The object being hovered.</param>
        public void HoverObject(VisualisationObject @object)
        {
            OnVisualisationObjectHovered?.Invoke(@object);
        }

        /// <summary>
        /// Triggers the unhover event.
        /// </summary>
        public void ClearHover()
        {
            OnVisualisationObjectUnhovered?.Invoke();
        }

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _objectDictionary = new Dictionary<string, VisualisationObject>();
            _typeToObjects = new Dictionary<string, List<string>>();
            _objectToTypes = new Dictionary<string, string>();
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

                    if (_typeToObjects.ContainsKey(visualisationObject.ObjectType))
                    {
                        _typeToObjects[visualisationObject.ObjectType].Add(child.name);
                    }
                    else
                    {
                        _typeToObjects.Add(visualisationObject.ObjectType, new List<string> { child.name });
                    }

                    if (!_objectToTypes.ContainsKey(child.name))
                        _objectToTypes.Add(child.name, visualisationObject.ObjectType);
                }
            }
        }

        #endregion

        #region Private Internals

        private static ProblemObjects _instance;

        private Dictionary<string, VisualisationObject> _objectDictionary;
        private Dictionary<string, List<string>> _typeToObjects;
        private Dictionary<string, string> _objectToTypes;

        #endregion

        #region Editor Support

#if UNITY_EDITOR
        /// <summary>
        /// Initializes the component in the editor, generating prefabs and scripts for each type.
        /// </summary>
        public void InitialiseComponent(List<PlanObject> objectDeclarations, List<string> allLeafType)
        {
            foreach (var type in allLeafType)
            {
                var folderPath = AssetUtils.GetSimulationObjectsPath(SceneManager.GetActiveScene().name);
                Object originalPrefab = (GameObject)AssetDatabase.LoadAssetAtPath(CommonPaths.PdsimObjectPrefab, typeof(GameObject));
                var prefabInstance = PrefabUtility.InstantiatePrefab(originalPrefab, null) as GameObject;
                prefabInstance.GetComponent<VisualisationObject>().ObjectType = type;
                var newModel = PrefabUtility.SaveAsPrefabAsset(prefabInstance, folderPath + "/" + type + ".prefab");

                Prefabs.Add(newModel.GetComponent<VisualisationObject>());

                if (newModel.GetComponent<ProblemObjectMetaData>() == null)
                {
                    newModel.AddComponent<ProblemObjectMetaData>();
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

        #endregion
    }
}
