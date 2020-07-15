[System.Serializable]
public class PredicateCommandSettings
{
    public int predicateTypeIndex;
    public PredicateCommand commandBehavior;
    public int orderOfExecution;

    public PredicateCommandSettings(PredicateCommand commandBehavior,int predicateTypeIndex = 0, int orderOfExecution = 0)
    {
        this.predicateTypeIndex = predicateTypeIndex;
        this.commandBehavior = commandBehavior;
        this.orderOfExecution = orderOfExecution;
    }
}
