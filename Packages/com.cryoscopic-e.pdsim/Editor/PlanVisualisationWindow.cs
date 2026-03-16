using GeTModel;
using PDSim.Components;
using PDSim.ScriptableObjects;
using PDSimAPI;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PDSim.Editor
{
    public class PlanVisualisationWindow : EditorWindow
    {
        [MenuItem("PDSim/Plan Visualiser")]
        public static void ShowWindow()
        {
            var wnd = GetWindow<PlanVisualisationWindow>();
            wnd.titleContent = new GUIContent("Plan Visualiser");
            wnd.minSize = new Vector2(420, 500);
        }

        // ScriptableObject references
        private PlanningProblem _planningProblem;
        private PlanGeneration _planGeneration;

        // Parsed model
        private Visualisation _visualisation;
        private string _parseError;

        // UI state
        private int _selectedTab = 0;
        private static readonly string[] TabLabels = { "Problem", "Plan" };

        // Scroll positions
        private Vector2 _problemScrollPos;
        private Vector2 _planScrollPos;

        // Foldout states
        private bool _showObjects = true;
        private bool _showFluents = true;
        private bool _showActions = true;
        private bool _showInitialState = true;
        private readonly Dictionary<string, bool> _typeFoldouts = new Dictionary<string, bool>();

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

            _selectedTab = GUILayout.Toolbar(_selectedTab, TabLabels);
            EditorGUILayout.Space(4);

            switch (_selectedTab)
            {
                case 0: DrawProblemTab(); break;
                case 1: DrawPlanTab(); break;
            }
        }

        // ─── HEADER ──────────────────────────────────────────────────────────────

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

        // ─── ASSET FIELDS & BUTTONS ──────────────────────────────────────────────

        private void DrawObjectFields()
        {
            EditorGUI.BeginChangeCheck();
            _planningProblem = (PlanningProblem)EditorGUILayout.ObjectField(
                "Planning Problem", _planningProblem, typeof(PlanningProblem), false);
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
                         && _planningProblem.proto != null
                         && _planGeneration.proto != null;

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
                _visualisation = new Visualisation(_planningProblem.proto, _planGeneration.proto);
                _typeFoldouts.Clear();
            }
            catch (Exception e)
            {
                _parseError = $"Parse failed: {e.Message}";
            }
        }

        private void TryAutoPopulate()
        {
            var controller = FindFirstObjectByType<Controller>();
            if (controller == null)
            {
                EditorUtility.DisplayDialog("Auto-populate", "No Controller found in the active scene.", "OK");
                return;
            }
            _planningProblem = controller.problem;
            _planGeneration = controller.planGeneration;
            _visualisation = null;
            _parseError = null;
        }

        // ─── PROBLEM TAB ─────────────────────────────────────────────────────────

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
                foreach (var kvp in _visualisation.FluentsDefinitions)
                    EditorGUILayout.LabelField(kvp.Value.ToString(), EditorStyles.wordWrappedLabel);
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
                foreach (var kvp in _visualisation.ActionsDefinitions)
                {
                    var a = kvp.Value;
                    string paramStr = a.parameters != null && a.parameters.Count > 0
                        ? string.Join(", ", a.parameters)
                        : string.Empty;
                    EditorGUILayout.LabelField($"{a.actionName}({paramStr})");
                }
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.Space(2);
        }

        private void DrawInitialStateSection()
        {
            _showInitialState = EditorGUILayout.BeginFoldoutHeaderGroup(
                _showInitialState, $"Initial State  ({_visualisation.CurrentWorldState.State.Count})");

            if (_showInitialState)
            {
                EditorGUI.indentLevel++;
                foreach (var sv in _visualisation.CurrentWorldState.State)
                    EditorGUILayout.LabelField(sv.ToString(), EditorStyles.wordWrappedLabel);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.Space(2);
        }

        // ─── PLAN TAB ─────────────────────────────────────────────────────────────

        private void DrawPlanTab()
        {
            var plan = _visualisation.PlanGeneration.Plan;

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

        // ─── HELPERS ─────────────────────────────────────────────────────────────

        private static void DrawSeparator()
        {
            var rect = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.3f));
            EditorGUILayout.Space(2);
        }
    }
}
