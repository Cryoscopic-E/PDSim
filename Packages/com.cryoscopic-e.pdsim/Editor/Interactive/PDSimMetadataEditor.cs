using UnityEditor;
using UnityEngine;
using PDSim.Components;
using PDSim.Interactive;
using System.Linq;

namespace PDSim.Editor.Interactive
{
    public static class PDSimMetadataMenuItems
    {
        // --- HIERARCHY / GAMEOBJECT MENU ---

        [MenuItem("GameObject/PDSim Tag/Register as Anchor", false, 10)]
        public static void RegisterAsAnchorGO(MenuCommand menuCommand)
        {
            RegisterAsAnchor(menuCommand.context as GameObject);
        }

        [MenuItem("GameObject/PDSim Tag/Register as Render", false, 11)]
        public static void RegisterAsRenderGO(MenuCommand menuCommand)
        {
            RegisterAsRender(menuCommand.context as GameObject);
        }

        [MenuItem("GameObject/PDSim Tag/Register as UI", false, 12)]
        public static void RegisterAsUIGO(MenuCommand menuCommand)
        {
            RegisterAsUI(menuCommand.context as GameObject);
        }

        [MenuItem("GameObject/PDSim Tag/Register as Attribute", false, 13)]
        public static void RegisterAsAttributeGO(MenuCommand menuCommand)
        {
            RegisterAsAttribute(menuCommand);
        }

        // --- COMPONENT CONTEXT MENUS ---

        [MenuItem("CONTEXT/Transform/PDSim Tag: Register as Anchor")]
        public static void RegisterAsAnchorCtx(MenuCommand menuCommand)
        {
            RegisterAsAnchor((menuCommand.context as Transform).gameObject);
        }

        [MenuItem("CONTEXT/Renderer/PDSim Tag: Register as Render")]
        public static void RegisterAsRenderCtx(MenuCommand menuCommand)
        {
            RegisterAsRender((menuCommand.context as Renderer).gameObject);
        }

        // Specific TextMeshPro context menus
        [MenuItem("CONTEXT/TextMeshPro/PDSim Tag: Register as UI")]
        [MenuItem("CONTEXT/TextMeshProUGUI/PDSim Tag: Register as UI")]
        [MenuItem("CONTEXT/UnityEngine.UIElements.UIDocument/PDSim Tag: Register as UI")]
        [MenuItem("CONTEXT/UnityEngine.UI.Text/PDSim Tag: Register as UI")]
        public static void RegisterAsUICtx(MenuCommand menuCommand)
        {
            RegisterAsUI((menuCommand.context as Component).gameObject);
        }

        // --- VALIDATION METHODS ---

        [MenuItem("GameObject/PDSim Tag/Register as Anchor", true)]
        [MenuItem("GameObject/PDSim Tag/Register as Render", true)]
        [MenuItem("GameObject/PDSim Tag/Register as UI", true)]
        [MenuItem("GameObject/PDSim Tag/Register as Attribute", true)]
        [MenuItem("CONTEXT/Transform/PDSim Tag: Register as Anchor", true)]
        [MenuItem("CONTEXT/Renderer/PDSim Tag: Register as Render", true)]
        [MenuItem("CONTEXT/TextMeshPro/PDSim Tag: Register as UI", true)]
        [MenuItem("CONTEXT/TextMeshProUGUI/PDSim Tag: Register as UI", true)]
        [MenuItem("CONTEXT/UnityEngine.UIElements.UIDocument/PDSim Tag: Register as UI", true)]
        [MenuItem("CONTEXT/UnityEngine.UI.Text/PDSim Tag: Register as UI", true)]
        public static bool PDSimMenuValidation(MenuCommand menuCommand)
        {
            GameObject go = null;
            if (menuCommand.context is GameObject g) go = g;
            else if (menuCommand.context is Component c) go = c.gameObject;

            if (go == null) return false;
            
            // Only show if the selected object is part of a PDSim Prefab hierarchy
            var current = go.transform;
            while (current != null)
            {
                if (current.GetComponent<VisualisationObject>() != null) return true;
                current = current.parent;
            }
            return false;
        }

        // --- SHARED LOGIC ---

        public static void RegisterAsAnchor(GameObject selected)
        {
            if (selected == null) return;
            var root = FindPDSimRoot(selected);
            if (root == null) return;

            var metadata = GetOrAddMetadata(root);
            Undo.RecordObject(metadata, "Register PDSim Anchor");
            metadata.anchors.Add(new PDSimMetadata.Entry<Transform> { name = GenerateName(root, selected), reference = selected.transform });
            EditorUtility.SetDirty(metadata);
            Debug.Log($"[PDSim] Registered '{selected.name}' as Anchor on '{root.name}'");
        }

        public static void RegisterAsRender(GameObject selected)
        {
            if (selected == null) return;
            var renderer = selected.GetComponent<Renderer>();
            if (renderer == null)
            {
                EditorUtility.DisplayDialog("PDSim Error", "Object must have a Renderer component to be registered as a Render.", "OK");
                return;
            }

            var root = FindPDSimRoot(selected);
            if (root == null) return;

            var metadata = GetOrAddMetadata(root);
            Undo.RecordObject(metadata, "Register PDSim Render");
            metadata.renders.Add(new PDSimMetadata.Entry<Renderer> { name = GenerateName(root, selected), reference = renderer });
            EditorUtility.SetDirty(metadata);
            Debug.Log($"[PDSim] Registered '{selected.name}' as Render on '{root.name}'");
        }

        public static void RegisterAsUI(GameObject selected)
        {
            if (selected == null) return;
            // Support multiple UI types via string lookup to avoid assembly dependencies
            Component uiComp = selected.GetComponent("TMPro.TMP_Text");
            if (uiComp == null) uiComp = selected.GetComponent<UnityEngine.UIElements.UIDocument>();
            if (uiComp == null) uiComp = selected.GetComponent("UnityEngine.UI.Text");
            
            if (uiComp == null)
            {
                EditorUtility.DisplayDialog("PDSim Error", "Object must have a TextMeshPro, UI Text, or UIDocument component to be registered as UI.", "OK");
                return;
            }

            var root = FindPDSimRoot(selected);
            if (root == null) return;

            var metadata = GetOrAddMetadata(root);
            Undo.RecordObject(metadata, "Register PDSim UI");
            metadata.ui.Add(new PDSimMetadata.Entry<Component> { name = GenerateName(root, selected), reference = uiComp });
            EditorUtility.SetDirty(metadata);
            Debug.Log($"[PDSim] Registered '{selected.name}' as UI on '{root.name}'");
        }

        public static void RegisterAsAttribute(MenuCommand menuCommand)
        {
            var selected = menuCommand.context as GameObject;
            if (selected == null) return;
            var root = FindPDSimRoot(selected);
            if (root == null) return;

            var metadata = GetOrAddMetadata(root);
            Undo.RecordObject(metadata, "Register PDSim Attribute");
            metadata.attributes.Add(new PDSimMetadata.Entry<string> { name = GenerateName(root, selected), reference = "" });
            EditorUtility.SetDirty(metadata);
            Debug.Log($"[PDSim] Registered '{selected.name}' as Attribute on '{root.name}'");
        }

        private static GameObject FindPDSimRoot(GameObject child)
        {
            var current = child.transform;
            while (current != null)
            {
                if (current.GetComponent<VisualisationObject>() != null) return current.gameObject;
                current = current.parent;
            }
            return null;
        }

        private static PDSimMetadata GetOrAddMetadata(GameObject root)
        {
            var metadata = root.GetComponent<PDSimMetadata>();
            if (metadata == null) metadata = Undo.AddComponent<PDSimMetadata>(root);
            return metadata;
        }

        private static string GenerateName(GameObject root, GameObject selected)
        {
            if (root == selected) return "Root";
            return selected.name;
        }
    }
}
