using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class PlanningEditorWindow : EditorWindow
    {
        private WebViewHook _webView;
        
        [ MenuItem( "PDDL Simulation/PDDL editor" ) ]
        public static void Init()
        {
            var window = (PlanningEditorWindow)GetWindow(typeof(PlanningEditorWindow));
            window.titleContent = new GUIContent("PDDL Editor");
            window.maxSize = new Vector2(1920,1080);
            window.Show();
            window.Focus();
        }

        private void OnEnable()
        {
            if (!_webView)
            {
                // create webView
                _webView = CreateInstance<WebViewHook>();
            }
        }

        public void OnDisable()
        {
            if (_webView)
            {
                // signal the browser to unhook
                _webView.Detach();
            }
        }
        private void OnDestroy()
        {
            //Destroy web view
            DestroyImmediate(_webView);
        }

        private void OnGUI()
        {
            var ev = Event.current;
            // hook to this window
            if (_webView.Hook(this))
                // do the first thing to do
                _webView.LoadURL("https://web-planner.herokuapp.com/");
            if (ev.type == EventType.Repaint)
            {
                // keep the browser aware with resize
                _webView.OnGUI(new Rect(Vector2.zero, this.position.size));
            }
        }

    }
}
   
