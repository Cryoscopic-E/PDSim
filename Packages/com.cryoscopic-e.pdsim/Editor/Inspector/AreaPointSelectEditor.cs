using UnityEditor;
using UnityEngine;
using PDSim.Helpers.UI;

namespace PDSim.Editor.Inspector
{
    [CustomEditor(typeof(AreaPointSelect))]
    public class AreaPointSelectEditor : UnityEditor.Editor
    {
        void OnSceneGUI()
        {
            Handles.color = Color.yellow;
            AreaPointSelect myObj = (AreaPointSelect)target;

            Handles.color = Color.black;
            Handles.Label(myObj.transform.position + myObj.offset, "Area Point Select");
            Handles.DrawWireCube(myObj.transform.position + myObj.offset, myObj.size);
        }
    }
}
