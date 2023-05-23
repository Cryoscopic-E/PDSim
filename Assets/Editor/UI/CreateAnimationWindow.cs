using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

namespace Editor.UI
{
    public class CreateAnimationWindow : EditorWindow
    {
        public static void ShowAsModal()
        {
            var wnd = GetWindow<CreateAnimationWindow>();
            wnd.titleContent = new GUIContent("Create New Animation");
            wnd.ShowModal();
        }

        public void CreateGUI()
        {
            // Set Window not resizable
            this.minSize = new Vector2(365, 325);
            this.maxSize = this.minSize;
            
            // Each editor window contains a root VisualElement object
            var root = rootVisualElement;
            
            // Import UXML
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/UI Toolkit/EditorUI/CreateAnimationDialog.uxml");
            var fromUxml = visualTree.Instantiate();
            root.Add(fromUxml);
        }


    }

}
