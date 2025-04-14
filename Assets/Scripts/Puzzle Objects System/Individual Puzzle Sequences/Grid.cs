using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    [SerializeField] private KeyItem item;
    public void AssignItem(KeyItem item)
    {
        this.item = item;
    }
    public KeyItem GetItem() => item; 

}
