using System.Collections;
using System.Collections.Generic;


public class Node 
{
    public int priority;
    protected string nodeName;

    public Node(int priority,string nodeName)
    {
        this.priority = priority;
        this.nodeName = nodeName;
    }
    public enum NodeStatus
    {
        FAILURE,
        SUCCESS,
        RUNNING,
        DEFAULT
    }

    protected int childIndex;
    protected List<Node> children = new List<Node>();

    public void AddChild(Node node) => children.Add(node);

    public virtual NodeStatus Evaluate() =>
         NodeStatus.DEFAULT;


    public void ResetChildIndex() => childIndex = 0;

}



