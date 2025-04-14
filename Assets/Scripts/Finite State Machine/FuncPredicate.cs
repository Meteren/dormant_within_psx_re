using System;

public class FuncPredicate : IPredicate
{
    Func<bool> condition;

    public FuncPredicate(Func<bool> condition)
    {
        this.condition = condition;
    }

    public bool Evaluate()
    {
        return condition.Invoke();
    }
}
