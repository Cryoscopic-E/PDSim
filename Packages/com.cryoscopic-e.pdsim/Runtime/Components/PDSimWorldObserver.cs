using System;
using System.Collections.Generic;
using UnityEngine;
using GeTPlan.Core.Logic;
using GeTPlan.Core.Models;
using GeTPlan.Core.Models.Expressions;
using PDSim.Components;

namespace PDSim.Runtime.Components
{
    /// <summary>
    /// Central observer for the PDSim world state, managing registries for objects, sensors, and actions.
    /// </summary>
    public class PDSimWorldObserver : MonoBehaviour
    {
        private static PDSimWorldObserver _instance;
        public static PDSimWorldObserver Instance
        {
            get
            {
                if (_instance == null)
                    _instance = UnityEngine.Object.FindAnyObjectByType<PDSimWorldObserver>();
                return _instance;
            }
        }

        // Event triggered when a fluent value is updated in the live state.
        public event Action<FluentExpression, object> OnStateChanged;

        private WorldState _liveState = new WorldState(new Dictionary<FluentExpression, object>());
        public WorldState LiveState => _liveState;

        private readonly List<PDSimMetadata> _objects = new();
        private readonly List<SemanticSensor> _sensors = new();
        private readonly List<LogicalAction> _actions = new();

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this);
                return;
            }
            _instance = this;
        }

        #region Registry Methods

        public void RegisterObject(PDSimMetadata obj) 
        {
            if (!_objects.Contains(obj)) _objects.Add(obj);
        }

        public void UnregisterObject(PDSimMetadata obj) => _objects.Remove(obj);

        public void RegisterSensor(SemanticSensor sensor)
        {
            if (!_sensors.Contains(sensor)) _sensors.Add(sensor);
        }

        public void UnregisterSensor(SemanticSensor sensor) => _sensors.Remove(sensor);

        public void RegisterAction(LogicalAction action)
        {
            if (!_actions.Contains(action)) _actions.Add(action);
        }

        public void UnregisterAction(LogicalAction action) => _actions.Remove(action);

        #endregion

        /// <summary>
        /// Updates a fluent in the internal WorldState and notifies listeners.
        /// </summary>
        public void UpdateFluent(FluentExpression fluent, object value)
        {
            _liveState = _liveState.WithFluent(fluent, value);
            OnStateChanged?.Invoke(fluent, value);
        }

        /// <summary>
        /// Aggregates all registered LogicalActions and sensors to generate a PlanningDomain.
        /// </summary>
        public PlanningDomain GenerateDomain()
        {
            var domain = new PlanningDomain("PDSimInteractiveDomain");
            // Note: Implementation details for aggregating sensors and actions 
            // will be refined in Tasks 3 and 4.
            return domain;
        }

        /// <summary>
        /// Aggregates all registered PDSimMetadata objects and current state to generate a PlanningProblem.
        /// </summary>
        public PlanningProblem GenerateProblem()
        {
            var domain = GenerateDomain();
            var objects = new List<PlanObject>();
            // Mapping logic for PDSimMetadata to PlanObject will be added here.
            
            return new PlanningProblem(
                "PDSimInteractiveProblem", 
                domain, 
                objects, 
                _liveState.Fluents, 
                new List<PlanExpression>()
            );
        }
    }
}
