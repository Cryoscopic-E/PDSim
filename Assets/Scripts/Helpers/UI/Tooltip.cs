using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Helpers.UI
{
    [ExecuteAlways]
    public class Tooltip : MonoBehaviour
    {
        [SerializeField] Transform canvas;
        [SerializeField] Transform point;
        [SerializeField] LineRenderer line;

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
    }
}
