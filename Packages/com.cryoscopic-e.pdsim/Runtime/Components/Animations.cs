using GeTPlan.Core.Models;
using GeTPlan.Core.Models.Expressions;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PDSim.Components
{
    /// <summary>
    /// Manages the mapping between fluent effects and their corresponding animations.
    /// Provides functionality to check and retrieve animation data for grounded fluents.
    /// </summary>
    public class Animations : MonoBehaviour
    {
        #region Public API

        /// <summary>
        /// Singleton instance of the Animations manager.
        /// </summary>
        public static Animations Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindAnyObjectByType<Animations>();
                return _instance;
            }
        }

        /// <summary>
        /// Maps fluent effect names to their corresponding animation component.
        /// </summary>
        public Dictionary<string, FluentAnimation> EffectToAnimations { get; private set; }

        /// <summary>
        /// Checks for available animations that match a given grounded fluent and its value.
        /// </summary>
        /// <param name="fluent">The grounded fluent and its current value.</param>
        /// <returns>A list of matching animation data.</returns>
        public List<FluentAnimation.AnimationData> AnimationCheck((FluentExpression Fluent, object Value) fluent)
        {
            if (!EffectToAnimations.ContainsKey(fluent.Fluent.Name))
                return new List<FluentAnimation.AnimationData>();

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

            var returnList = new List<FluentAnimation.AnimationData>();

            var fluentAnimation = EffectToAnimations[fluent.Fluent.Name];
            var animationData = fluentAnimation.AnimationDataList;

            var typeHierarchy = TypeHierarchy.Instance;

            foreach (var animation in animationData)
            {
                var animationParameters = animation.Parameters;
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

        /// <summary>
        /// Initializes the component with a list of predicate definitions from the domain.
        /// </summary>
        /// <param name="fluents">The list of fluents to initialize.</param>
        public void InitialiseComponent(List<PredicateDefinition> fluents)
        {
            foreach (var fluent in fluents)
            {
                var fluentAnimation = gameObject.AddComponent<FluentAnimation>();

                var parametersTypes = fluent.Parameters.Select(p => p.Type.Name).ToList();
                var parametersNames = fluent.Parameters.Select(p => p.Name).ToList();

                fluentAnimation.MetaData = new FluentAnimation.FluentMetadata()
                {
                    Name = fluent.Name,
                    ParametersNames = parametersNames,
                    ParametersTypes = parametersTypes,
                    FluentValueType = fluent.ValueType
                };
                fluentAnimation.AnimationDataList = new List<FluentAnimation.AnimationData>();
            }
        }

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            EffectToAnimations = new Dictionary<string, FluentAnimation>();
            _cache = new Dictionary<string, List<FluentAnimation.AnimationData>>();

            // Find all FluentAnimation components in the scene
            var fluentAnimations = FindObjectsByType<FluentAnimation>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (var fluentAnimation in fluentAnimations)
            {
                if (fluentAnimation.MetaData != null)
                {
                    EffectToAnimations.Add(fluentAnimation.MetaData.Name, fluentAnimation);
                }
            }
        }

        #endregion

        #region Private Internals

        private static Animations _instance;

        // Cache for AnimationCheck
        private Dictionary<string, List<FluentAnimation.AnimationData>> _cache;

        #endregion
    }
}
