using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyItem : Item, ISelectable, IHoverHandler
{
    [Header("Belonged PuzzleObject")]
    public PuzzleObject belongedPuzzleObject;
    public bool isSelected;
    protected Outline outLine;
    public virtual void ApplyBehaviour()
    {
        return;
    }

    public virtual void OnHoverEnter()
    {
        Debug.Log("Cannot Be Hovered");
    }

    public virtual void OnHoverExit()
    {
        Debug.Log("Cannot Be Hoverred");
    }

    public virtual void OnSelect()
    {
        Debug.Log("Cannot be selected");
    }
    public void DisableOutline() => outLine.enabled = false;
    public void EnableOutline() => outLine.enabled = true;
    public void ResetOutlineColor() => outLine.OutlineColor = Color.white;
}
