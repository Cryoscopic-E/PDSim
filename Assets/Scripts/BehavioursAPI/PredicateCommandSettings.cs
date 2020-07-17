[System.Serializable]
public class PredicateCommandSettings
{
    public int predicateTypeIndex;
    public CommandBase commandBaseBehavior;
    public int orderOfExecution;

    public PredicateCommandSettings(CommandBase commandBaseBehavior,int predicateTypeIndex = 0, int orderOfExecution = 0)
    {
        this.predicateTypeIndex = predicateTypeIndex;
        this.commandBaseBehavior = commandBaseBehavior;
        this.orderOfExecution = orderOfExecution;
    }
}
