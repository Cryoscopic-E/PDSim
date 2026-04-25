using UnityEngine;
using TMPro;
using UnityEngine.Serialization;

namespace PDSim.Helpers
{
    /// <summary>
    /// A simple world-space tooltip that displays text and a line connecting it to a target point.
    /// </summary>
    [ExecuteAlways]
    public class Helper_Tooltip : MonoBehaviour
    {
        #region Inspector Fields

        [Header("References")]
        [Tooltip("The canvas transform containing the tooltip UI.")]
        [SerializeField, FormerlySerializedAs("canvas")]
        private Transform Canvas;

        [Tooltip("The target point the line should connect to.")]
        [SerializeField, FormerlySerializedAs("point")]
        private Transform Point;

        [Tooltip("The LineRenderer to draw the connection.")]
        [SerializeField, FormerlySerializedAs("line")]
        private LineRenderer Line;

        [Tooltip("The TextMeshPro component for the tooltip text.")]
        [SerializeField, FormerlySerializedAs("tmpText")]
        private TMP_Text TmpText;

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            if (Line != null)
            {
                Line.positionCount = 2;
            }
        }

        private void Update()
        {
            if (Line == null || Canvas == null || Point == null)
                return;

            Vector3[] points = new Vector3[Line.positionCount];
            points[0] = Canvas.localPosition;
            points[1] = Point.localPosition;
            Line.SetPositions(points);
        }

        #endregion

        #region Public API

        /// <summary>
        /// Sets the text displayed by the tooltip.
        /// </summary>
        /// <param name="text">The text to display.</param>
        public void SetText(string text)
        {
            if (TmpText != null)
            {
                TmpText.text = text;
            }
        }

        #endregion
    }
}
