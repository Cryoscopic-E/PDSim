using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GeTPlan.Core.Logic;
using GeTPlan.Core.Models;
using GeTPlan.Core.Models.Expressions;
using PDSim.Runtime.Utils;

namespace PDSim.Components.Interactive
{
    /// <summary>
    /// Central observer for the PDSim world state, managing registries for objects, sensors, and actions.
    /// Orchestrates the interactive simulation state and domain generation.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class PDSimWorldObserver : MonoBehaviour
    {
        #region Public API

        /// <summary>
        /// Singleton instance of the PDSimWorldObserver.
        /// </summary>
        public static PDSimWorldObserver Instance
        {
            get
            {
                if (_instance == null)
                    _instance = UnityEngine.Object.FindAnyObjectByType<PDSimWorldObserver>();
                return _instance;
            }
        }

        /// <summary>
        /// Event triggered when a fluent value is updated in the live state.
        /// </summary>
        public event Action<FluentExpression, object> OnStateChanged;

        /// <summary>
        /// The current live state of the simulation.
        /// </summary>
        public WorldState LiveState => _liveState;

        /// <summary>
        /// Registers a problem object metadata component.
        /// </summary>
        public void RegisterObject(ProblemObjectMetaData obj)
        {
            if (!_objects.Contains(obj)) _objects.Add(obj);
        }

        /// <summary>
        /// Unregisters a problem object metadata component.
        /// </summary>
        public void UnregisterObject(ProblemObjectMetaData obj) => _objects.Remove(obj);

        /// <summary>
        /// Registers a semantic sensor.
        /// </summary>
        public void RegisterSensor(SemanticSensor sensor)
        {
            if (!_sensors.Contains(sensor)) _sensors.Add(sensor);
        }

        /// <summary>
        /// Unregisters a semantic sensor.
        /// </summary>
        public void UnregisterSensor(SemanticSensor sensor) => _sensors.Remove(sensor);

        /// <summary>
        /// Registers a logical action.
        /// </summary>
        public void RegisterAction(LogicalAction action)
        {
            if (!_actions.Contains(action)) _actions.Add(action);
        }

        /// <summary>
        /// Unregisters a logical action.
        /// </summary>
        public void UnregisterAction(LogicalAction action) => _actions.Remove(action);

        /// <summary>
        /// Updates a fluent in the internal WorldState and notifies listeners.
        /// </summary>
        /// <param name="fluent">The fluent expression to update.</param>
        /// <param name="value">The new value of the fluent.</param>
        public void UpdateFluent(FluentExpression fluent, object value)
        {
            _liveState = _liveState.WithFluent(fluent, value);
            OnStateChanged?.Invoke(fluent, value);
        }

        /// <summary>
        /// Aggregates all registered LogicalActions and sensors to generate a PlanningDomain.
        /// </summary>
        /// <returns>A new PlanningDomain instance based on the current scene state.</returns>
        public PlanningDomain GenerateDomain()
        {
            var domain = new PlanningDomain("PDSimInteractiveDomain");

            // 1. Collect types from registered objects
            var typeMap = new Dictionary<string, PlanType>();
            foreach (var obj in _objects)
            {
                var typeName = obj.PlanTypeName;
                if (!typeMap.ContainsKey(typeName))
                {
                    typeMap[typeName] = new PlanType(typeName);
                    domain.AddType(typeMap[typeName]);
                }
            }

            // 2. Infer predicates from live state fluents and action DSL strings
            var seenPredicates = new HashSet<string>();

            // 2a. From current live state
            foreach (var kvp in _liveState.Fluents)
            {
                var fluent = kvp.Key;
                if (!seenPredicates.Add(fluent.Name)) continue;

                var argTypes = InferArgumentTypes(fluent, typeMap);
                var valueType = kvp.Value is bool ? "bool" : "real";
                domain.AddPredicate(new PredicateDefinition(fluent.Name, valueType, argTypes.ToArray()));
            }

            // 2b. From action preconditions/effects DSL (covers predicates not yet in state)
            foreach (var action in _actions)
            {
                foreach (var dsl in action.Preconditions.Concat(action.Effects))
                {
                    var expr = DslParser.Parse(dsl, "self");
                    if (expr is FluentExpression fe && seenPredicates.Add(fe.Name))
                    {
                        var argTypes = fe.Arguments
                            .Select(_ => typeMap.GetValueOrDefault("object", new PlanType("object")))
                            .ToArray();
                        domain.AddPredicate(new PredicateDefinition(fe.Name, argTypes));
                    }
                }
            }

            // 3. Collect action definitions from registered LogicalActions
            foreach (var action in _actions)
            {
                domain.AddAction(action.ToActionDefinition(typeMap));
            }

            return domain;
        }

        /// <summary>
        /// Aggregates all registered PDSimMetadata objects and current state to generate a PlanningProblem.
        /// </summary>
        /// <returns>A new PlanningProblem instance based on the current scene state.</returns>
        public GeTPlan.Core.Models.PlanningProblem GenerateProblem()
        {
            var domain = GenerateDomain();
            var objects = _objects.Select(o => o.ToPlanObject()).ToList();

            return new GeTPlan.Core.Models.PlanningProblem(
                "PDSimInteractiveProblem",
                domain,
                objects,
                _liveState.Fluents,
                new List<PlanExpression>()
            );
        }

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this);
                return;
            }
            _instance = this;
        }

        #endregion

        #region Private Internals

        private static PDSimWorldObserver _instance;

        private WorldState _liveState = new WorldState(new Dictionary<FluentExpression, object>());
        private readonly List<ProblemObjectMetaData> _objects = new List<ProblemObjectMetaData>();
        private readonly List<SemanticSensor> _sensors = new List<SemanticSensor>();
        private readonly List<LogicalAction> _actions = new List<LogicalAction>();

        private PlanType[] InferArgumentTypes(FluentExpression fluent, Dictionary<string, PlanType> typeMap)
        {
            var argTypes = new List<PlanType>();
            foreach (var arg in fluent.Arguments)
            {
                var argName = arg.ToString();
                var matchingObj = _objects.FirstOrDefault(o => o.gameObject.name == argName);
                var argType = matchingObj != null
                    ? typeMap.GetValueOrDefault(matchingObj.PlanTypeName, new PlanType("object"))
                    : new PlanType("object");
                argTypes.Add(argType);
            }
            return argTypes.ToArray();
        }

        #endregion
    }
}
