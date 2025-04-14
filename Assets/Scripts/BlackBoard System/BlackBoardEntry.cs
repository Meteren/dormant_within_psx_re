using System;

public class BlackBoardEntry<T>
{
    public T Value { get; set; }
    public Type valType => Value.GetType();
    public BlackBoardKey Key { get; set; }

    public BlackBoardEntry(T value, BlackBoardKey key)
    {
        Value = value;
        this.Key = key;
    }
}
