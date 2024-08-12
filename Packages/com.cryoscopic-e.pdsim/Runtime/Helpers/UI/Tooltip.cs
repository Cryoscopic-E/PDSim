using UnityEngine;
using TMPro;

namespace PDSim.Helpers.UI
{
    [ExecuteAlways]
    public class Tooltip : MonoBehaviour
    {
        [SerializeField] Transform canvas;
        [SerializeField] Transform point;
        [SerializeField] LineRenderer line;
        [SerializeField] TMP_Text tmpText;

        private void Start()
        {
            line.positionCount = 2;
        }
        // Update is called once per frame
        void Update()
        {
            Vector3[] points = new Vector3[line.positionCount];
            points[0] = canvas.localPosition;
            points[1] = point.localPosition;
            line.SetPositions(points);
        }
        public void SetText(string text)
        {
            tmpText.text = text;
        }
    }
}
