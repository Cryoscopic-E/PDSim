using PDSim.Connection;
using System.Collections;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;


public class TestProtobuf : EditorWindow
{
    [MenuItem("PDSim/Testprotobuf")]
    public static void ShowWindow()
    {
        var wnd = GetWindow<TestProtobuf>();
        wnd.titleContent = new GUIContent("Test Protobuf");
    }

    public void CreateGUI()
    {
        // Set Window not resizable
        this.minSize = new Vector2(365, 325);
        this.maxSize = this.minSize;
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Test"))
        {
            EditorCoroutineUtility.StartCoroutine(TestConnection(), this);
        }
    }

    private IEnumerator TestConnection()
    {
        var request = new ProtobufRequest();
        var response = request.Connect();

        var problem = Problem.Parser.ParseFrom(response);

        var eff = problem.Actions[0].Effects[0];

        Debug.Log(eff.Effect_.Fluent);

        yield return null;

    }
}