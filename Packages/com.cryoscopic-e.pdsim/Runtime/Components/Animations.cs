using GeTModel;
using System.Collections.Generic;
using UnityEngine;
using static PDSim.Components.FluentAnimation;

namespace PDSim.Components
{
    public class Animations : MonoBehaviour
    {
        // Singleton Instance
        // ------------------

        private static Animations _instance;
        public static Animations Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindAnyObjectByType<Animations>();
                return _instance;
            }
        }

        // Map fluent effect names to animations
        public Dictionary<string, FluentAnimation> effectToAnimations;

        // Cache for AnimationCheck
        private Dictionary<string, List<AnimationData>> _cache;

        private void Awake()
        {
            effectToAnimations = new Dictionary<string, FluentAnimation>();
            _cache = new Dictionary<string, List<AnimationData>>();

            // Find all FluentAnimation components in the scene
            var fluentAnimations = FindObjectsByType<FluentAnimation>(FindObjectsSortMode.None);
            foreach (var fluentAnimation in fluentAnimations)
            {
                effectToAnimations.Add(fluentAnimation.metaData.Name, fluentAnimation);
            }
        }

        public List<AnimationData> AnimationCheck(GeTStateVariable fluent)
        {
            if (!effectToAnimations.ContainsKey(fluent.Fluent.FluentName))
                return new List<AnimationData>(); // Return empty list instead of null for safety

            // Construct cache key
            var fluentParameters = fluent.GetParameters();
            var fluentParametersAsTypes = new List<string>();
            foreach (var parameter in fluentParameters)
            {
                var t = ProblemObjects.Instance.GetTypeOfObject(parameter);
                fluentParametersAsTypes.Add(t);
            }

            string cacheKey = fluent.Fluent.FluentName + ":" + string.Join(",", fluentParametersAsTypes);

            if (_cache.ContainsKey(cacheKey))
            {
                return _cache[cacheKey];
            }

            var returnList = new List<AnimationData>();

            // the vars until now looks like: located; [car1, loc1]; [car, location]
            // get the FluentAnimation list containing the animation data
            var fluentAnimation = effectToAnimations[fluent.Fluent.FluentName];
            var animationData = fluentAnimation.animationData;

            var fluentParamentersAnimation = fluentAnimation.metaData.ParametersTypes;// as in the editor: [movable, location] movable is the parent of car

            // all the animations defined with types: [[car, location], [movable, location], [plane, location]]
            var listOfAnimationTypesDefined = new List<List<string>>();
            foreach (var animation in animationData)
            {
                var animationParameters = animation.parameters;
                var animationParametersAsTypes = new List<string>();
                foreach (var parameter in animationParameters)
                {
                    animationParametersAsTypes.Add(parameter);
                }
                listOfAnimationTypesDefined.Add(animationParametersAsTypes);
            }
            var typeHierarchy = TypeHierarchy.Instance;
            // check if the fluent parameters match the animation parameters if the same or the defined types are parents of the fluent parameters
            for (int i = 0; i < listOfAnimationTypesDefined.Count; i++)
            {
                var animationParameters = listOfAnimationTypesDefined[i];
                var match = true;
                for (int j = 0; j < animationParameters.Count; j++)
                {
                    // Safety check for index out of range if definitions don't match
                    if (j >= fluentParametersAsTypes.Count)
                    {
                        match = false;
                        break;
                    }

                    if (animationParameters[j] != fluentParametersAsTypes[j])
                    {
                        if (!typeHierarchy.IsChildOf(fluentParametersAsTypes[j], animationParameters[j]))
                        {
                            match = false;
                            break;
                        }
                    }
                }
                if (match)
                {
                    returnList.Add(animationData[i]);
                }
            }

            _cache[cacheKey] = returnList;
            return returnList;

        }
        public void InitialiseComponent(List<GeTFluent> fluents)
        {
            foreach (var fluent in fluents)
            {
                var fluentAnimation = gameObject.AddComponent<FluentAnimation>();

                var parametersTypes = new List<string>();
                var parametersNames = new List<string>();
                foreach (var parameter in fluent.Parameters)
                {
                    parametersNames.Add(parameter.Name);
                    parametersTypes.Add(parameter.TypeName);
                }

                fluentAnimation.metaData = new FluentMetadata()
                {
                    Name = fluent.Name,
                    ParametersNames = parametersNames,
                    ParametersTypes = parametersTypes,
                    FluentValueType = fluent.FluentValueType
                };
                fluentAnimation.animationData = new List<AnimationData>();
            }
        }
    }
}
