using UnityEngine;
using System.Collections.Generic;
using GeTModel;

namespace PDSim.Components
{
    public class FluentAnimation : MonoBehaviour
    {
        public FluentMetadata metaData;

        public List<AnimationData> animationData;


        public bool AddAnimationData(string animationName, List<string> attributes, GameObject sceneObject, string scriptClassName)
        {
            foreach (var data in animationData)
            {
                if (data.name == animationName)
                {
                    return false;
                }
            }

            var visualizer = sceneObject.GetComponent<IFluentVisualizer>() as MonoBehaviour;

            animationData.Add(new AnimationData()
            {
                name = animationName,
                parameters = attributes,
                scriptClassName = scriptClassName,
                visualizer = visualizer,
                sceneObjectReference = sceneObject
            });
            return true;
        }


        [System.Serializable]
        public class AnimationData
        {
            public string name;
            public List<string> parameters;
            public string scriptClassName;
            public MonoBehaviour visualizer;
            public GameObject sceneObjectReference;
        }

        [System.Serializable]
        public class FluentMetadata
        {
            public string Name;
            public List<string> ParametersNames;
            public List<string> ParametersTypes;
            public ValueType FluentValueType;

            public override string ToString()
            {
                return $"{Name} ({string.Join(", ", ParametersTypes)}) := {FluentValueType}";
            }
        }
    }
}
