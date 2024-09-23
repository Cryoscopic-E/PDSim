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
            Handles.DrawWireCube(myObj.transform.position + myObj.offset, myObj.size);
        }
    }
}
