using UnityEditor;
using UnityEngine;

public class AreaPointSelect : MonoBehaviour
{
    public Vector3 size;
    public Vector3 offset;

    public Vector3 GetRandomPoint()
    {
        Vector3 point = new Vector3(
                       Random.Range(transform.position.x - size.x / 2, transform.position.x + size.x / 2),
                       Random.Range(transform.position.y - size.y / 2, transform.position.y + size.y / 2),
                       Random.Range(transform.position.z - size.z / 2, transform.position.z + size.z / 2)
                       );
        return point;
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
