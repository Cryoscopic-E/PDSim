using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Unity.VisualScripting;
using System.Linq;

public class MenuComponents
{
    [MenuItem("CONTEXT/Component/Add To Variables")]
    public static void AddComponentToVariables(MenuCommand menuCommand)
    {
        // Get Component
        Component component = menuCommand.context as Component;

        // Check if prefab mode
        if (PrefabStageUtility.GetCurrentPrefabStage() is null)
        {
            // Add Component to SceneVariables
            if (AddToSceneVariables(component) is false)
            {
                EditorUtility.DisplayDialog("Error", "Error adding Component in SceneVariables", "OK");
                return;
            }
        }
        else
        {
            // Add Component to ObjectVariables
            if (AddToObjectVariables(component) is false)
            {
                EditorUtility.DisplayDialog("Error", "Error adding Component in ObjectVariables", "OK");
                return;
            }
        }
    }
    [MenuItem("GameObject/Add To Variables", false, 0)]
    static void AddGameObjectToVariables(MenuCommand menuCommand)
    {
        // Get GameObject
        GameObject go = menuCommand.context as GameObject;

        if (PrefabStageUtility.GetCurrentPrefabStage() is null)
        {
            
            // Add GameObject to SceneVariables
            if (AddToSceneVariables(go) is false)
            {
                EditorUtility.DisplayDialog("Error", "Error adding GameObject in SceneVariables", "OK");
                return;
            }
            
        }
        else
        {
            // Add GameObject to ObjectVariables
            if (AddToObjectVariables(go) is false)
            {
                EditorUtility.DisplayDialog("Error", "Error adding GameObject in ObjectVariables", "OK");
                return;
            }
        }
    }

    public static bool AddToSceneVariables(Component component)
    {
        // Get SceneVariables from scene
        SceneVariables sceneVariables = GameObject.FindObjectOfType<SceneVariables>();

        // Check if Component exists
        if (component is null)
        {
            Debug.LogError("Component is null");
            return false;
        }

        if (sceneVariables.variables.declarations.IsDefined(component.name) is false)
        {
            // Add Component to SceneVariables
            var componentType = component.GetType();
            var composedName = component.name + "-" + componentType.Name;
            sceneVariables.variables.declarations.Set(composedName, component);
            EditorUtility.SetDirty(sceneVariables);
        }
        else
        {
            Debug.LogWarning("Component is already in SceneVariables");
            return false;
        }

        return true;
    }
    public static bool AddToSceneVariables(GameObject go)

    {
        // Get SceneVariables from scene
        SceneVariables sceneVariables = GameObject.FindObjectOfType<SceneVariables>();

        // Check if GameObject exists
        if (go is null)
        {
            Debug.LogError("GameObject is null");
            return false;
        }

        if (sceneVariables.variables.declarations.IsDefined(go.name) is false)
        {
            // Add GameObject to SceneVariables
            sceneVariables.variables.declarations.Set(go.name, go);
            EditorUtility.SetDirty(sceneVariables);
        }
        else
        {
            Debug.LogWarning("GameObject is already in SceneVariables");
            return false;
        }

        return true;

    }
    public static bool AddToObjectVariables(Component component)
    {
        // Get ObjectVariables from scene
        Variables objectVariables = component.transform.root.GetComponent<Variables>();

        // Check if Component exists
        if (component is null)
        {
            Debug.LogError("Component is null");
            return false;
        }

        if (objectVariables.declarations.IsDefined(component.name) is false)
        {
            // Add Component to ObjectVariables
            var componentType = component.GetType();
            var composedName = component.name + "-" + componentType.Name;
            objectVariables.declarations.Set(composedName, component);
            EditorUtility.SetDirty(objectVariables);
        }
        else
        {
            Debug.LogWarning("Component is already in ObjectVariables");
            return false;
        }

        return true;
    }
    public static bool AddToObjectVariables(GameObject go)
    {
        // Get ObjectVariables from scene
        Variables objectVariables = go.transform.root.GetComponent<Variables>();

        // Check if GameObject exists
        if (go is null)
        {
            Debug.LogError("GameObject is null");
            return false;
        }

        if (objectVariables.declarations.IsDefined(go.name) is false)
        {
            // Add GameObject to ObjectVariables
            objectVariables.declarations.Set(go.name, go);
            EditorUtility.SetDirty(objectVariables);
        }
        else
        {
            Debug.LogWarning("GameObject is already in ObjectVariables");
            return false;
        }

        return true;
    }
}
