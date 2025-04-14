using System.Collections.Generic;
using System.Linq;

class SortedSelectorNode : SelectorNode
{
    public SortedSelectorNode(string nodeName, int priority = 0) : base(priority, nodeName)
    {
    }
    List<Node> sortedChildren => children.OrderBy(x => x.priority).ToList();

    public override NodeStatus Evaluate()
    {

        if (childIndex >= sortedChildren.Count)
            ResetChildIndex();

        switch (sortedChildren[childIndex].Evaluate())
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



