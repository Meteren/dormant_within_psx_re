public class SelectorNode : Node
{
    
    public SelectorNode(int priority, string nodeName) : base(priority, nodeName)
    {
        
    }

    public override NodeStatus Evaluate()
    {

        if(childIndex >= children.Count)
            ResetChildIndex();       
      
        switch (children[childIndex].Evaluate())
        {
            case NodeStatus.FAILURE:
                childIndex++;
                return childIndex < children.Count ? NodeStatus.RUNNING : NodeStatus.FAILURE;
            case NodeStatus.SUCCESS:
                 ResetChildIndex();
                 return NodeStatus.SUCCESS;
            case NodeStatus.RUNNING:
                 return NodeStatus.RUNNING;
        }
      
        return NodeStatus.DEFAULT; ;
    }
}



