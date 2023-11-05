using UnityEditor;
using UnityEngine;

namespace Helpers
{
    /// <summary>
    /// Helper class to select a random point in an area or volume
    /// </summary>
    public class AreaPointSelect : MonoBehaviour
    {
        public Vector3 size;
        public Vector3 offset;

        public Vector3 GetRandomPoint()
        {
            // Random point in the area with offset
            var point = new Vector3(
                           Random.Range(0, size.x),
                           offset.y,
                           Random.Range(0, size.z));



            return transform.position + point;

        }
    }

    [CustomEditor(typeof(AreaPointSelect))]
    public class WireBoxExample : Editor
    {
        void OnSceneGUI()
        {
            Handles.color = Color.yellow;
            AreaPointSelect myObj = (AreaPointSelect)target;
            Handles.DrawWireCube(myObj.transform.position + myObj.offset, myObj.size);
        }
    }
}
