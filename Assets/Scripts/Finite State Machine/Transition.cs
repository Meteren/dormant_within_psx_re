public class Transition
{
    public IPredicate condition;
    public IState to;

    public Transition(IPredicate condition, IState to)
    {
        this.condition = condition;
        this.to = to;
    }
    
}
