using UnityEngine;
using UnityEngine.UI;

public enum SimStatus
{
    None,
    Init,
    PlanExecution
}

public class HudController : MonoBehaviour
{
    public Text simulationStatusText;

    public Text currentActionPredicateText;

    public void SetSimulationStatus(SimStatus status)
    {
        switch (status)
        {
            case SimStatus.Init:
                simulationStatusText.text = "Initialization Block";
                break;
            case SimStatus.PlanExecution:
                simulationStatusText.text = "Executing Plan";
                break;
            default:
                simulationStatusText.text = "Simulation Stopped";
                break;
        }
    }

    public void SetCurrentAction(string action, string predicate)
    {
        currentActionPredicateText.text = "Action: " + action + "  Predicate: " + predicate;
    }
}
