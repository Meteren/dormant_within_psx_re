using System.Collections.Generic;
using UnityEngine;

public class BlackBoard
{
    private Dictionary<string, BlackBoardKey> keys = new(); 
    private Dictionary<BlackBoardKey, object> blackBoardEntries = new();

    public void SetValue<T>(string name, T value)
    {
        BlackBoardKey key = GetOrAddKey(name);

        blackBoardEntries[key] = new BlackBoardEntry<T>(value, key);
    }

    public bool TryGetValue<T>(string name, out T value)
    {
        BlackBoardKey key = GetKey(name); 
        if(key != null)
        {
            if(blackBoardEntries.TryGetValue(key, out object entry) && entry is BlackBoardEntry<T> passedEntry)
            {
                blackBoardEntries[key] = passedEntry;
                value = passedEntry.Value;
                return true;
            }
           
        }
        value = default;
        return false;
    }

    private BlackBoardKey GetOrAddKey(string name)
    {
        if(keys.TryGetValue(name,out BlackBoardKey dictKey))
        {
            return dictKey;
        }
        else
        {
            BlackBoardKey newKey = new BlackBoardKey(name);
            keys[name] = newKey;
            return newKey;
        }
    }

    private BlackBoardKey GetKey(string name)
    {
        if (!keys.ContainsKey(name))
        {
            return null;
        }
        return keys[name];
    }
}
