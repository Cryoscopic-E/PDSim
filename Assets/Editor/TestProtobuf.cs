using PDSim.Connection;
using PDSim.Protobuf;
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

        var reader = new ProtobufReader();
        reader.Read(response);
        yield return null;

    }
}