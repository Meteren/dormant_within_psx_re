using System;

public class BlackBoardKey : IEquatable<BlackBoardKey>
{ 
    public string Name { get; set; }
    public BlackBoardKey(string key)
    {
        Name = key;
    }

    public override string ToString() => this.Name;

    public bool Equals(BlackBoardKey other) => other == this;

    public override bool Equals(object obj) => obj is BlackBoardKey passedVal && Equals(passedVal);

    public override int GetHashCode() => Name.GetHashCode();    
    public static bool operator ==(BlackBoardKey a, BlackBoardKey b) => a.Name == b.Name;
    public static bool operator !=(BlackBoardKey a, BlackBoardKey b) => !Equals(a, b);

}