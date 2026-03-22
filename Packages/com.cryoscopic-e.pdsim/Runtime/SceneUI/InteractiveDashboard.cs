using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using GeTPlan.Core.Models;
using GeTPlan.Core.Logic;
using GeTPlan.Protobuf.Client;
using PDSim.Runtime.Components;
using PDSim.Components;
using GeTPlan.Core.Models.Expressions;
using PDSim.Runtime.Utils;

namespace PDSim.Runtime.SceneUI
{
    public class InteractiveDashboard : MonoBehaviour
    {
        private UIDocument _uiDocument;
        private ScrollView _fluentList;
        private TextField _goalInput;
        private Button _solveButton;

        private void OnEnable()
        {
            _uiDocument = GetComponent<UIDocument>();
            _uiDocument.visualTreeAsset = null;

            var root = _uiDocument.rootVisualElement;

            // Build UI programmatically
            var container = new VisualElement { name = "DashboardRoot" };
            container.style.flexGrow = 1;
            container.style.paddingLeft = container.style.paddingRight = container.style.paddingTop = container.style.paddingBottom = 10;
            root.Add(container);

            _fluentList = new ScrollView { name = "FluentList" };
            _fluentList.style.flexGrow = 1;
            _fluentList.style.marginBottom = 10;
            container.Add(_fluentList);

            _goalInput = new TextField("Goal") { name = "GoalInput" };
            _goalInput.style.marginBottom = 10;
            container.Add(_goalInput);

            _solveButton = new Button { name = "SolveButton", text = "Solve" };
            container.Add(_solveButton);

            _solveButton.clicked += OnSolveClicked;

            if (PDSimWorldObserver.Instance != null)
                PDSimWorldObserver.Instance.OnStateChanged += UpdateFluentList;
        }

        private void OnDisable()
        {
            if (PDSimWorldObserver.Instance != null)
                PDSimWorldObserver.Instance.OnStateChanged -= UpdateFluentList;
        }

        private void UpdateFluentList(FluentExpression fluent, object value)
        {
            // Simple refresh of the whole list for now
            _fluentList.Clear();
            var state = PDSimWorldObserver.Instance.LiveState;
            foreach (var kvp in state.Fluents)
            {
                var label = new Label($"{kvp.Key} = {kvp.Value}");
                _fluentList.Add(label);
            }
        }

        private void OnSolveClicked()
        {
            var observer = PDSimWorldObserver.Instance;
            if (observer == null) return;

            // 1. Generate Domain and Problem
            var domain = observer.GenerateDomain();
            var problem = observer.GenerateProblem();

            // 2. Parse Goal from UI
            string goalStr = _goalInput.value;
            if (!string.IsNullOrEmpty(goalStr))
            {
                var goalExpr = DslParser.Parse(goalStr);
                if (goalExpr != null) problem.Goals.Add(goalExpr);
            }

            // 3. Solve
            using var client = new PlanningClient();
            var result = client.DoPlan(problem);

            if (result.Status == PlanGenStatus.SolvedSatisficing || result.Status == PlanGenStatus.SolvedOptimally)
            {
                Debug.Log($"[PDSim] Plan Found! Actions: {result.Plan.Actions.Count}");

                // 4. Handoff to Controller for animation
                var controller = Controller.Instance;
                if (controller != null)
                {
                    // Manually inject the results since we bypassed the ScriptableObject loading
                    controller.visualisation = new PDSimAPI.Visualisation(new byte[0], new byte[0]); // Dummy bytes
                    // We'd need a cleaner way to 'inject' a live result into Controller.
                    // For now, let's just log.
                }
            }
            else
            {
                Debug.LogError($"[PDSim] Planning failed: {result.Status}");
            }
        }
    }
}
