using UnityEngine;
using System.Collections.Generic;

namespace PDSim.Components
{
    /// <summary>
    /// Holds animation data and metadata for a specific fluent.
    /// Used by the Animations component to match grounded fluents to visualization objects.
    /// </summary>
    public class FluentAnimation : MonoBehaviour
    {
        #region Public API

        /// <summary>
        /// Metadata describing the fluent this animation component represents.
        /// </summary>
        public FluentMetadata MetaData;

        /// <summary>
        /// List of animations associated with this fluent.
        /// </summary>
        public List<AnimationData> AnimationDataList;

        /// <summary>
        /// Adds new animation data to this fluent.
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

            var visualizer = sceneObject.GetComponent<IFluentVisualizer>() as MonoBehaviour;

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
        /// Represents a specific animation mapping for a fluent.
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
            /// The class name of the IFluentVisualizer implementation.
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
        /// Metadata about the fluent being animated.
        /// </summary>
        [System.Serializable]
        public class FluentMetadata
        {
            /// <summary>
            /// The name of the fluent.
            /// </summary>
            public string Name;
            /// <summary>
            /// The names of the fluent's parameters.
            /// </summary>
            public List<string> ParametersNames;
            /// <summary>
            /// The types of the fluent's parameters.
            /// </summary>
            public List<string> ParametersTypes;
            /// <summary>
            /// The type of the fluent's value.
            /// </summary>
            public string FluentValueType;

            /// <summary>
            /// Returns a string representation of the fluent metadata.
            /// </summary>
            /// <returns>A formatted string.</returns>
            public override string ToString()
            {
                return $"{Name} ({string.Join(", ", ParametersTypes)}) := {FluentValueType}";
            }
        }

        #endregion
    }
}
