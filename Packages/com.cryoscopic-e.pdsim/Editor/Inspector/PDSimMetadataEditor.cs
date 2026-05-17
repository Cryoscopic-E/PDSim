using UnityEditor;
using UnityEngine;
using PDSim.Components;

namespace PDSim.Editor.Inspector
{
    /// <summary>
    /// Static class containing menu items for PDSim metadata registration.
    /// </summary>
    public static class PDSimMetadataMenuItems
    {
        #region Hierarchy / GameObject Menu
        /// <summary>
        /// Registers the selected GameObject as an anchor.
        /// </summary>
        /// <param name="menuCommand">The menu command context.</param>
        [MenuItem("GameObject/PDSim Tag/Register as Anchor", false, 10)]
        public static void RegisterAsAnchorGO(MenuCommand menuCommand)
        {
            RegisterAsAnchor(menuCommand.context as GameObject);
        }

        /// <summary>
        /// Registers the selected GameObject as a render object.
        /// </summary>
        /// <param name="menuCommand">The menu command context.</param>
        [MenuItem("GameObject/PDSim Tag/Register as Render", false, 11)]
        public static void RegisterAsRenderGO(MenuCommand menuCommand)
        {
            RegisterAsRender(menuCommand.context as GameObject);
        }

        /// <summary>
        /// Registers the selected GameObject as a UI element.
        /// </summary>
        /// <param name="menuCommand">The menu command context.</param>
        [MenuItem("GameObject/PDSim Tag/Register as UI", false, 12)]
        public static void RegisterAsUIGO(MenuCommand menuCommand)
        {
            RegisterAsUI(menuCommand.context as GameObject);
        }

        /// <summary>
        /// Registers the selected GameObject as an attribute.
        /// </summary>
        /// <param name="menuCommand">The menu command context.</param>
        [MenuItem("GameObject/PDSim Tag/Register as Attribute", false, 13)]
        public static void RegisterAsAttributeGO(MenuCommand menuCommand)
        {
            RegisterAsAttribute(menuCommand);
        }
        #endregion

        #region Component Context Menus
        /// <summary>
        /// Context menu item to register a Transform as an anchor.
        /// </summary>
        /// <param name="menuCommand">The menu command context.</param>
        [MenuItem("CONTEXT/Transform/PDSim Tag: Register as Anchor")]
        public static void RegisterAsAnchorCtx(MenuCommand menuCommand)
        {
            RegisterAsAnchor((menuCommand.context as Transform).gameObject);
        }

        /// <summary>
        /// Context menu item to register a Renderer as a render object.
        /// </summary>
        /// <param name="menuCommand">The menu command context.</param>
        [MenuItem("CONTEXT/Renderer/PDSim Tag: Register as Render")]
        public static void RegisterAsRenderCtx(MenuCommand menuCommand)
        {
            RegisterAsRender((menuCommand.context as Renderer).gameObject);
        }

        /// <summary>
        /// Context menu item to register a UI component as a PDSim UI element.
        /// </summary>
        /// <param name="menuCommand">The menu command context.</param>
        [MenuItem("CONTEXT/TextMeshPro/PDSim Tag: Register as UI")]
        [MenuItem("CONTEXT/TextMeshProUGUI/PDSim Tag: Register as UI")]
        [MenuItem("CONTEXT/Text/PDSim Tag: Register as UI")]
        public static void RegisterAsUICtx(MenuCommand menuCommand)
        {
            RegisterAsUI((menuCommand.context as Component).gameObject);
        }
        #endregion

        #region Validation Methods
        /// <summary>
        /// Validates if the PDSim menu items should be shown for the current selection.
        /// </summary>
        /// <param name="menuCommand">The menu command context.</param>
        /// <returns>True if the menu item should be shown.</returns>
        [MenuItem("GameObject/PDSim Tag/Register as Anchor", true)]
        [MenuItem("GameObject/PDSim Tag/Register as Render", true)]
        [MenuItem("GameObject/PDSim Tag/Register as UI", true)]
        [MenuItem("GameObject/PDSim Tag/Register as Attribute", true)]
        [MenuItem("CONTEXT/Transform/PDSim Tag: Register as Anchor", true)]
        [MenuItem("CONTEXT/Renderer/PDSim Tag: Register as Render", true)]
        [MenuItem("CONTEXT/TextMeshPro/PDSim Tag: Register as UI", true)]
        [MenuItem("CONTEXT/TextMeshProUGUI/PDSim Tag: Register as UI", true)]
        [MenuItem("CONTEXT/UnityEngine.UIElements.UIDocument/PDSim Tag: Register as UI", true)]
        [MenuItem("CONTEXT/Text/PDSim Tag: Register as UI", true)]
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
        #endregion

        #region Shared Logic
        /// <summary>
        /// Registers a GameObject as an anchor in the PDSim root metadata.
        /// </summary>
        /// <param name="selected">The GameObject to register.</param>
        public static void RegisterAsAnchor(GameObject selected)
        {
            if (selected == null) return;
            var root = FindPDSimRoot(selected);
            if (root == null) return;

            var metadata = GetOrAddMetadata(root);
            Undo.RecordObject(metadata, "Register PDSim Anchor");
            metadata.Anchors.Add(new ProblemObjectMetaData.Entry<Transform> { Name = GenerateName(root, selected), Reference = selected.transform });
            EditorUtility.SetDirty(metadata);
            Debug.Log($"[PDSim] Registered '{selected.name}' as Anchor on '{root.name}'");
        }

        /// <summary>
        /// Registers a GameObject as a render object in the PDSim root metadata.
        /// </summary>
        /// <param name="selected">The GameObject to register.</param>
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
            metadata.Renders.Add(new ProblemObjectMetaData.Entry<Renderer> { Name = GenerateName(root, selected), Reference = renderer });
            EditorUtility.SetDirty(metadata);
            Debug.Log($"[PDSim] Registered '{selected.name}' as Render on '{root.name}'");
        }

        /// <summary>
        /// Registers a GameObject as a UI element in the PDSim root metadata.
        /// </summary>
        /// <param name="selected">The GameObject to register.</param>
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
            metadata.UI.Add(new ProblemObjectMetaData.Entry<Component> { Name = GenerateName(root, selected), Reference = uiComp });
            EditorUtility.SetDirty(metadata);
            Debug.Log($"[PDSim] Registered '{selected.name}' as UI on '{root.name}'");
        }

        /// <summary>
        /// Registers a GameObject as an attribute in the PDSim root metadata.
        /// </summary>
        /// <param name="menuCommand">The menu command context.</param>
        public static void RegisterAsAttribute(MenuCommand menuCommand)
        {
            var selected = menuCommand.context as GameObject;
            if (selected == null) return;
            var root = FindPDSimRoot(selected);
            if (root == null) return;

            var metadata = GetOrAddMetadata(root);
            Undo.RecordObject(metadata, "Register PDSim Attribute");
            metadata.Attributes.Add(new ProblemObjectMetaData.Entry<string> { Name = GenerateName(root, selected), Reference = "" });
            EditorUtility.SetDirty(metadata);
            Debug.Log($"[PDSim] Registered '{selected.name}' as Attribute on '{root.name}'");
        }
        #endregion

        #region Private Methods
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

        private static ProblemObjectMetaData GetOrAddMetadata(GameObject root)
        {
            var metadata = root.GetComponent<ProblemObjectMetaData>();
            if (metadata == null) metadata = Undo.AddComponent<ProblemObjectMetaData>(root);
            return metadata;
        }

        private static string GenerateName(GameObject root, GameObject selected)
        {
            if (root == selected) return "Root";
            return selected.name;
        }
        #endregion
    }
}
