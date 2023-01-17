using PDSim.Types;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlanListUI : MonoBehaviour
{
    [SerializeField]
    VisualTreeAsset actionItemTemplate;

    ListView planList;
    PlanActionsListController characterListController;
    MovablePanel movablePanel;

    private void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        movablePanel = new(root);

        planList = root.Q<ListView>("PlanList");

        var action1 = new PDAction()
        {
            name = "Action 1",
            attributes = new Dictionary<string, string>()
            {
                { "obj0", "type1" },
                { "obj1", "type3" }
            },
            effects = new List<PDBooleanFluent>()
            {
                new PDBooleanFluent()
                {
                    name = "Fluent 1",
                    value = true
                },
                new PDBooleanFluent()
                {
                    name = "Fluent 2",
                    value = false
                }
            }
        };

        var action2 = new PDAction()
        {
            name = "Action 1",
            attributes = new Dictionary<string, string>()
            {
                { "obj2", "type1" },
                { "obj4", "type3" }
            },
            effects = new List<PDBooleanFluent>()
            {
                new PDBooleanFluent()
                {
                    name = "Fluent 1",
                    value = true
                },
                new PDBooleanFluent()
                {
                    name = "Fluent 2",
                    value = false
                }
            }
        };

        var list = new List<PDAction>() { action1, action2 };


        // Initialize the character list controller
        characterListController = new PlanActionsListController();
        characterListController.SetPlanActions(list);
        characterListController.InitializeActionList(root, actionItemTemplate);

        // USE TO HIGHLIGHT ACTION
        //HighlightCurrentAction(0);
    }

    public void ToggleVisibility()
    {
        gameObject.SetActive(!gameObject.activeSelf);
        movablePanel.ResetPosition();
    }

    public void HighlightCurrentAction(int index)
    {
        planList.AddToSelection(index);
    }
}
