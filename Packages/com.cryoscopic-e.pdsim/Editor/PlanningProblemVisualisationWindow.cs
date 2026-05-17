using PDSim.Components;
using PDSim.ScriptableObjects;
using PDSimAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PDSim.Editor
{
    /// <summary>
    /// Custom editor window for visualizing planning problems and plans.
    /// </summary>
    public class PlanningProblemVisualisationWindow : EditorWindow
    {
        #region Fields
        // ScriptableObject references
        private PDSim.ScriptableObjects.ParsedProblem _planningProblem;
        private PlanGeneration _planGeneration;

        // Parsed model
        private Visualisation _visualisation;
        private string _parseError;

        // UI state
        private int _selectedTab = 0;
        private static readonly string[] _tabLabels = { "Problem", "Plan" };

        // Scroll positions
        private Vector2 _problemScrollPos;
        private Vector2 _planScrollPos;

        // Foldout states
        private bool _showObjects = true;
        private bool _showFluents = true;
        private bool _showActions = true;
        private bool _showInitialState = true;
        private readonly Dictionary<string, bool> _typeFoldouts = new Dictionary<string, bool>();
        #endregion

        #region Public Methods
        /// <summary>
        /// Shows the PlanningProblemVisualisationWindow.
        /// </summary>
        [MenuItem("PDSim/Planning Problem Visualiser")]
        public static void ShowWindow()
        {
            var wnd = GetWindow<PlanningProblemVisualisationWindow>();
            wnd.titleContent = new GUIContent("Plan Visualiser");
            wnd.minSize = new Vector2(420, 500);
        }
        #endregion

        #region Unity Lifecycle
        private void OnGUI()
        {
            DrawHeader();
            DrawObjectFields();
            DrawButtons();
            DrawSeparator();

            if (_parseError != null)
            {
                EditorGUILayout.HelpBox(_parseError, MessageType.Error);
                return;
            }

            if (_visualisation == null)
            {
                EditorGUILayout.Space(8);
                EditorGUILayout.HelpBox(
                    "Assign a Planning Problem and Plan Generation asset, then click Parse.\nOr use \"Auto-populate from Scene\" to load from the scene Controller.",
                    MessageType.Info);
                return;
            }

            _selectedTab = GUILayout.Toolbar(_selectedTab, _tabLabels);
            EditorGUILayout.Space(4);

            switch (_selectedTab)
            {
                case 0: DrawProblemTab(); break;
                case 1: DrawPlanTab(); break;
            }
        }
        #endregion

        #region Private Methods

        // Rendering logic for the window header.
        private void DrawHeader()
        {
            var headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Plan Visualiser", headerStyle, GUILayout.Height(24));
            EditorGUILayout.Space(4);
            DrawSeparator();
        }

        // Logic for drawing asset selection fields and action buttons.
        private void DrawObjectFields()
        {
            EditorGUI.BeginChangeCheck();
            _planningProblem = (PDSim.ScriptableObjects.ParsedProblem)EditorGUILayout.ObjectField(
                "Planning Problem", _planningProblem, typeof(PDSim.ScriptableObjects.ParsedProblem), false);
            _planGeneration = (PlanGeneration)EditorGUILayout.ObjectField(
                "Plan Generation", _planGeneration, typeof(PlanGeneration), false);

            if (EditorGUI.EndChangeCheck())
            {
                _visualisation = null;
                _parseError = null;
            }
        }

        private void DrawButtons()
        {
            EditorGUILayout.Space(4);
            EditorGUILayout.BeginHorizontal();

            bool canParse = _planningProblem != null
                         && _planGeneration != null
                         && _planningProblem.Proto != null
                         && _planGeneration.Proto != null;

            GUI.enabled = canParse;
            if (GUILayout.Button("Parse", GUILayout.Height(26)))
                TryParse();
            GUI.enabled = true;

            if (GUILayout.Button("Auto-populate from Scene", GUILayout.Height(26)))
                TryAutoPopulate();

            EditorGUILayout.EndHorizontal();
        }

        private void TryParse()
        {
            _parseError = null;
            _visualisation = null;
            try
            {
                _visualisation = new Visualisation(_planningProblem.Proto, _planGeneration.Proto);
                _typeFoldouts.Clear();
            }
            catch (Exception e)
            {
                _parseError = $"Parse failed: {e.Message}";
            }
        }

        private void TryAutoPopulate()
        {
            var controller = FindAnyObjectByType<Controller>();
            if (controller == null)
            {
                EditorUtility.DisplayDialog("Auto-populate", "No Controller found in the active scene.", "OK");
                return;
            }
            _planningProblem = controller.Problem;
            _planGeneration = controller.PlanGeneration;
            _visualisation = null;
            _parseError = null;
        }

        // Logic for drawing the problem tab, which lists objects, fluents, and actions.
        private void DrawProblemTab()
        {
            _problemScrollPos = EditorGUILayout.BeginScrollView(_problemScrollPos);

            DrawObjectsSection();
            DrawFluentsSection();
            DrawActionsSection();
            DrawInitialStateSection();

            EditorGUILayout.EndScrollView();
        }

        private void DrawObjectsSection()
        {
            _showObjects = EditorGUILayout.BeginFoldoutHeaderGroup(
                _showObjects, $"Objects  ({_visualisation.Objects.Count})");

            if (_showObjects)
            {
                EditorGUI.indentLevel++;
                foreach (var kvp in _visualisation.TypeToObjects)
                {
                    string typeName = kvp.Key;
                    if (!_typeFoldouts.ContainsKey(typeName))
                        _typeFoldouts[typeName] = true;

                    _typeFoldouts[typeName] = EditorGUILayout.Foldout(
                        _typeFoldouts[typeName],
                        $"{typeName}  ({kvp.Value.Count})",
                        true,
                        EditorStyles.foldoutHeader);

                    if (_typeFoldouts[typeName])
                    {
                        EditorGUI.indentLevel++;
                        foreach (var obj in kvp.Value)
                            EditorGUILayout.LabelField(obj.Name);
                        EditorGUI.indentLevel--;
                    }
                }
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.Space(2);
        }

        private void DrawFluentsSection()
        {
            _showFluents = EditorGUILayout.BeginFoldoutHeaderGroup(
                _showFluents, $"Fluents  ({_visualisation.FluentsDefinitions.Count})");

            if (_showFluents)
            {
                EditorGUI.indentLevel++;
                foreach (var f in _visualisation.FluentsDefinitions)
                    EditorGUILayout.LabelField(f.ToString(), EditorStyles.wordWrappedLabel);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.Space(2);
        }

        private void DrawActionsSection()
        {
            _showActions = EditorGUILayout.BeginFoldoutHeaderGroup(
                _showActions, $"Actions  ({_visualisation.ActionsDefinitions.Count})");

            if (_showActions)
            {
                EditorGUI.indentLevel++;
                foreach (var a in _visualisation.ActionsDefinitions)
                {
                    string paramStr = a.Parameters != null && a.Parameters.Count > 0
                        ? string.Join(", ", a.Parameters.Select(p => $"{p.Name}: {p.Type.Name}"))
                        : string.Empty;
                    EditorGUILayout.LabelField($"{a.Name}({paramStr})");
                }
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.Space(2);
        }

        private void DrawInitialStateSection()
        {
            _showInitialState = EditorGUILayout.BeginFoldoutHeaderGroup(
                _showInitialState, $"Initial State  ({_visualisation.CurrentWorldState.State.Count()})");

            if (_showInitialState)
            {
                EditorGUI.indentLevel++;
                foreach (var sv in _visualisation.CurrentWorldState.State)
                    EditorGUILayout.LabelField($"{sv.Key} = {sv.Value}", EditorStyles.wordWrappedLabel);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.Space(2);
        }

        // Logic for drawing the plan tab, which lists the sequence of actions in the plan.
        private void DrawPlanTab()
        {
            var plan = _visualisation.PlanResult.Plan;
            if (plan == null)
            {
                EditorGUILayout.HelpBox("No plan available in PlanResult.", MessageType.Warning);
                return;
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(
                $"Actions: {plan.Actions.Count}",
                EditorStyles.boldLabel,
                GUILayout.Width(130));
            EditorGUILayout.LabelField(
                plan.IsTemporal ? "Temporal Plan" : "Sequential Plan",
                EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();

            DrawSeparator();

            _planScrollPos = EditorGUILayout.BeginScrollView(_planScrollPos);

            var labelStyle = new GUIStyle(EditorStyles.label) { wordWrap = true };
            var indexStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel) { stretchWidth = false };
            var evenBg = new GUIStyle(EditorStyles.helpBox);

            for (int i = 0; i < plan.Actions.Count; i++)
            {
                var action = plan.Actions[i];
                EditorGUILayout.BeginHorizontal(i % 2 == 0 ? evenBg : GUIStyle.none, GUILayout.MinHeight(20));
                EditorGUILayout.LabelField($"{i + 1}", indexStyle, GUILayout.Width(36));
                EditorGUILayout.LabelField(action.ToString(), labelStyle);
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }

        // Shared UI helper methods.
        private static void DrawSeparator()
        {
            var rect = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.3f));
            EditorGUILayout.Space(2);
        }
        #endregion
    }
}
