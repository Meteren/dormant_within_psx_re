public class BehaviourTree : Node
{
    public BehaviourTree(int priority = 0, string nodeName = null) : base(priority, nodeName)
    {
    }

    public override NodeStatus Evaluate()
    {
        
        switch (children[childIndex].Evaluate())
        {
            case NodeStatus.FAILURE:
                ResetChildIndex();
                return NodeStatus.FAILURE;
            case NodeStatus.SUCCESS:
                return NodeStatus.SUCCESS;
            case NodeStatus.RUNNING:
                return NodeStatus.RUNNING;
        }
        
        return NodeStatus.DEFAULT;
    }
}



