using UnityEngine;
using UnityEngine.Serialization;

namespace PDSim.Helpers
{
    /// <summary>
    /// Rotates the GameObject to face the main camera.
    /// Useful for world-space UI elements like nameplates or icons.
    /// </summary>
    public class Helper_BillboardEffect : MonoBehaviour
    {
        #region Inspector Fields

        [Header("Settings")]
        [Tooltip("The canvas to associate with the main camera.")]
        [SerializeField, FormerlySerializedAs("canvas")]
        private Canvas Canvas;

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            if (Canvas != null)
            {
                Canvas.worldCamera = Camera.main;
            }
        }

        private void Update()
        {
            if (Camera.main == null) return;

            // Billboard effect for object gui
            transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.forward,
                             Camera.main.transform.rotation * Vector3.up);
        }

        #endregion
    }
}

