using UnityEngine;

namespace PDSim.Helpers.UI
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
            // Get a random point in the area or volume defined by size and offset
            Vector3 point = new Vector3(
                Random.Range(transform.position.x - size.x / 2 + offset.x, transform.position.x + size.x / 2 + offset.x),
                Random.Range(transform.position.y - size.y / 2 + offset.y, transform.position.y + size.y / 2 + offset.y),
                Random.Range(transform.position.z - size.z / 2 + offset.z, transform.position.z + size.z / 2 + offset.z)
            );
            return point;

        }
    }
}
