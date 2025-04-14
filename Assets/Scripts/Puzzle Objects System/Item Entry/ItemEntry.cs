using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ItemEntry<T> where T : Component
{
    public T Value {  get; private set; }
    public Type ValType { get; private set; }

    public ItemEntry(T value)
    {
        Value = value;
        ValType = Value.GetType();
    }

}
