using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Cube : KeyItem
{
    [Header("Event Handling")]
    [SerializeField] private EventController eventController;
    [SerializeField] private EventListener listener;


    private new void Start()
    {
        base.Start();
        outLine = GetComponent<Outline>();

    }
  
    public override void OnSelect()
    {
        outLine.OutlineColor = Color.red;
        Debug.Log($"{itemName} selected");
    }

    public override void OnHoverEnter()
    {
        EnableOutline();

    }

    public override void OnHoverExit()
    {
        DisableOutline();
    }


}
