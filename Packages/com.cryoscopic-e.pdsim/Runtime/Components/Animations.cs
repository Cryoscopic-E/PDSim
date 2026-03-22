using GeTPlan.Core.Models;
using GeTPlan.Core.Logic;
using GeTPlan.Core.Models.Expressions;
using PDSimAPI;
using System.Collections.Generic;
using System.Linq;
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
                if (fluentAnimation.metaData != null)
                {
                    effectToAnimations.Add(fluentAnimation.metaData.Name, fluentAnimation);
                }
            }
        }

        public List<AnimationData> AnimationCheck((FluentExpression Fluent, object Value) fluent)
        {
            if (!effectToAnimations.ContainsKey(fluent.Fluent.Name))
                return new List<AnimationData>();

            // Construct cache key
            // Grounded fluents have ConstantExpression arguments.
            var fluentParameters = fluent.Fluent.Arguments
                .Select(a => a is ConstantExpression c ? c.Value.ToString() : a.ToString())
                .ToList();

            var fluentParametersAsTypes = new List<string>();
            foreach (var parameter in fluentParameters)
            {
                var t = ProblemObjects.Instance.GetTypeOfObject(parameter);
                if (t != null)
                    fluentParametersAsTypes.Add(t);
            }

            string cacheKey = fluent.Fluent.Name + ":" + string.Join(",", fluentParametersAsTypes);

            if (_cache.ContainsKey(cacheKey))
            {
                return _cache[cacheKey];
            }

            var returnList = new List<AnimationData>();

            var fluentAnimation = effectToAnimations[fluent.Fluent.Name];
            var animationData = fluentAnimation.animationData;

            var typeHierarchy = TypeHierarchy.Instance;
            
            foreach (var animation in animationData)
            {
                var animationParameters = animation.parameters;
                var match = true;
                
                if (animationParameters.Count != fluentParametersAsTypes.Count)
                {
                    match = false;
                }
                else
                {
                    for (int j = 0; j < animationParameters.Count; j++)
                    {
                        if (animationParameters[j] != fluentParametersAsTypes[j])
                        {
                            if (!typeHierarchy.IsChildOf(fluentParametersAsTypes[j], animationParameters[j]))
                            {
                                match = false;
                                break;
                            }
                        }
                    }
                }
                
                if (match)
                {
                    returnList.Add(animation);
                }
            }

            _cache[cacheKey] = returnList;
            return returnList;
        }

        public void InitialiseComponent(List<PredicateDefinition> fluents)
        {
            foreach (var fluent in fluents)
            {
                var fluentAnimation = gameObject.AddComponent<FluentAnimation>();

                var parametersTypes = fluent.ArgumentTypes.Select(t => t.Name).ToList();
                var parametersNames = fluent.ArgumentTypes.Select((t, i) => $"arg{i}").ToList();

                fluentAnimation.metaData = new FluentMetadata()
                {
                    Name = fluent.Name,
                    ParametersNames = parametersNames,
                    ParametersTypes = parametersTypes,
                    FluentValueType = fluent.ValueType
                };
                fluentAnimation.animationData = new List<AnimationData>();
            }
        }
    }
}
