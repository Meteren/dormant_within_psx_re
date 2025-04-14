public class Leaf : Node
{
    IStrategy strategy;

    public Leaf(string nodeName, IStrategy strategy, int priority = 0) : base(priority,nodeName)
    {
        this.strategy = strategy;
    }
    public override NodeStatus Evaluate() => strategy.Evaluate();
}



