using System;

public class Condition : IStrategy
{
    Func<bool> condition;

    public Condition(Func<bool> condition)
    {
        this.condition = condition;
    }

    public Node.NodeStatus Evaluate()
    {
        if (condition.Invoke())
            return Node.NodeStatus.SUCCESS;
        else return Node.NodeStatus.FAILURE;
    }
}
