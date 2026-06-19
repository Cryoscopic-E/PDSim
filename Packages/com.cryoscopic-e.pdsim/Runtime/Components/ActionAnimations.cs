using GeTPlan.Core.Models;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PDSim.Components
{
    /// <summary>
    /// Manages the mapping between action names and their corresponding animations.
    /// Provides functionality to check and retrieve animation data for grounded actions.
    /// </summary>
    public class ActionAnimations : MonoBehaviour
    {
        #region Public API

        /// <summary>
        /// Singleton instance of the ActionAnimations manager.
        /// </summary>
        public static ActionAnimations Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindAnyObjectByType<ActionAnimations>();
                return _instance;
            }
        }

        /// <summary>
        /// Maps action names to their corresponding animation component.
        /// </summary>
        public Dictionary<string, ActionAnimation> ActionToAnimation { get; private set; }

        /// <summary>
        /// Checks for available animations that match a given grounded action.
        /// </summary>
        /// <param name="action">The grounded action to check.</param>
        /// <returns>A list of matching animation data (shared empty list on no match — do not mutate).</returns>
        public List<ActionAnimation.AnimationData> AnimationCheck(GroundedAction action)
        {
            if (!ActionToAnimation.TryGetValue(action.ActionName, out var actionAnimation))
                return _emptyResult;

            var paramTypes = action.Objects
                .Select(o => ProblemObjects.Instance.GetTypeOfObject(o.Name))
                .Where(t => t != null)
                .ToList();

            string cacheKey = action.ActionName + ":" + string.Join(",", paramTypes);

            if (_cache.TryGetValue(cacheKey, out var cached))
                return cached;

            var returnList = new List<ActionAnimation.AnimationData>();
            var typeHierarchy = TypeHierarchy.Instance;

            foreach (var animData in actionAnimation.AnimationDataList)
            {
                var animParams = animData.Parameters;
                var match = true;

                if (animParams.Count != paramTypes.Count)
                {
                    match = false;
                }
                else
                {
                    for (int j = 0; j < animParams.Count; j++)
                    {
                        if (animParams[j] != paramTypes[j])
                        {
                            if (!typeHierarchy.IsChildOf(paramTypes[j], animParams[j]))
                            {
                                match = false;
                                break;
                            }
                        }
                    }
                }

                if (match)
                    returnList.Add(animData);
            }

            _cache[cacheKey] = returnList;
            return returnList;
        }

        /// <summary>
        /// Initializes the component with a list of action definitions from the domain.
        /// </summary>
        /// <param name="actions">The list of actions to initialize.</param>
        public void InitialiseComponent(List<ActionDefinition> actions)
        {
            foreach (var action in actions)
            {
                var actionAnimation = gameObject.AddComponent<ActionAnimation>();

                var parametersTypes = action.Parameters.Select(p => p.Type.Name).ToList();
                var parametersNames = action.Parameters.Select(p => p.Name).ToList();

                actionAnimation.MetaData = new ActionAnimation.ActionMetadata()
                {
                    Name = action.Name,
                    ParametersNames = parametersNames,
                    ParametersTypes = parametersTypes
                };
                actionAnimation.AnimationDataList = new List<ActionAnimation.AnimationData>();
            }
        }

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            ActionToAnimation = new Dictionary<string, ActionAnimation>();
            _cache = new Dictionary<string, List<ActionAnimation.AnimationData>>();

            var actionAnimations = FindObjectsByType<ActionAnimation>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (var aa in actionAnimations)
            {
                if (aa.MetaData != null)
                    ActionToAnimation.Add(aa.MetaData.Name, aa);
            }
        }

        #endregion

        #region Private Internals

        private static ActionAnimations _instance;

        // Shared empty result returned when no animation matches — callers must not mutate it.
        private static readonly List<ActionAnimation.AnimationData> _emptyResult
            = new List<ActionAnimation.AnimationData>();

        private Dictionary<string, List<ActionAnimation.AnimationData>> _cache;

        #endregion
    }
}
