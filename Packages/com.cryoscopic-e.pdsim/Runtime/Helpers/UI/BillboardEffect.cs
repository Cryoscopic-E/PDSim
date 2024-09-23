using UnityEngine;

namespace PDSim.Helpers.UI
{
    public class BillboardEffect : MonoBehaviour
    {
        public Canvas canvas;
        private void Start()
        {
            canvas.worldCamera = Camera.main;
        }
        void Update()
        {
            // Billboard effect for object gui
            transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.forward,
                             canvas.worldCamera.transform.rotation * Vector3.up);
        }
    }
}

