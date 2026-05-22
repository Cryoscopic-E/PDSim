using UnityEngine;
using System.Collections.Generic;

namespace PDSim.Components
{
    /// <summary>
    /// Holds animation data and metadata for a specific action.
    /// Used by the ActionAnimations component to match grounded actions to visualization objects.
    /// </summary>
    public class ActionAnimation : MonoBehaviour
    {
        #region Public API

        /// <summary>
        /// Metadata describing the action this animation component represents.
        /// </summary>
        public ActionMetadata MetaData;

        /// <summary>
        /// List of animations associated with this action.
        /// </summary>
        public List<AnimationData> AnimationDataList;

        /// <summary>
        /// Adds new animation data to this action.
        /// </summary>
        /// <param name="animationName">The unique name of the animation.</param>
        /// <param name="attributes">The list of attribute types the animation matches.</param>
        /// <param name="sceneObject">The prefab or scene object reference for the animation.</param>
        /// <param name="scriptClassName">The optional script class name for custom visualization logic.</param>
        /// <returns>True if the animation was added, false if an animation with the same name already exists.</returns>
        public bool AddAnimationData(string animationName, List<string> attributes, GameObject sceneObject, string scriptClassName)
        {
            foreach (var data in AnimationDataList)
            {
                if (data.Name == animationName)
                {
                    return false;
                }
            }

            var visualizer = sceneObject.GetComponent<IActionVisualizer>() as MonoBehaviour;

            AnimationDataList.Add(new AnimationData()
            {
                Name = animationName,
                Parameters = attributes,
                ScriptClassName = scriptClassName,
                Visualizer = visualizer,
                SceneObjectReference = sceneObject
            });
            return true;
        }

        #endregion

        #region Data Classes

        /// <summary>
        /// Represents a specific animation mapping for an action.
        /// </summary>
        [System.Serializable]
        public class AnimationData
        {
            /// <summary>
            /// The name of the animation.
            /// </summary>
            public string Name;
            /// <summary>
            /// The parameter types this animation expects.
            /// </summary>
            public List<string> Parameters;
            /// <summary>
            /// The class name of the IActionVisualizer implementation.
            /// </summary>
            public string ScriptClassName;
            /// <summary>
            /// Reference to the visualizer component.
            /// </summary>
            public MonoBehaviour Visualizer;
            /// <summary>
            /// Reference to the scene object or prefab used for the animation.
            /// </summary>
            public GameObject SceneObjectReference;
        }

        /// <summary>
        /// Metadata about the action being animated.
        /// </summary>
        [System.Serializable]
        public class ActionMetadata
        {
            /// <summary>
            /// The name of the action.
            /// </summary>
            public string Name;
            /// <summary>
            /// The names of the action's parameters.
            /// </summary>
            public List<string> ParametersNames;
            /// <summary>
            /// The types of the action's parameters.
            /// </summary>
            public List<string> ParametersTypes;

            /// <summary>
            /// Returns a string representation of the action metadata.
            /// </summary>
            public override string ToString()
            {
                if (ParametersNames == null || ParametersTypes == null)
                    return Name ?? "";
                var paramList = new System.Collections.Generic.List<string>();
                for (int i = 0; i < ParametersNames.Count && i < ParametersTypes.Count; i++)
                    paramList.Add($"?{ParametersNames[i]}:{ParametersTypes[i]}");
                return $"{Name} ({string.Join(", ", paramList)})";
            }
        }

        #endregion
    }
}
