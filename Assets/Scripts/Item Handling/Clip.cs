using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clip : Item, ICombinable
{
    [Header("Clip Amount")]
    public int currentAmount;
    public int maxAmount;

    [Header("Conditions")]
    public bool isAttached;
    public bool isEmpty;

    private new void Update()
    {    
        if (isAttached)
            GetComponent<Collider>().enabled = false;
        else
            base.Update();

    }
    public void OnTryCombine(ItemRepresenter representer)
    {
        Debug.Log("Clip combination");
        Clip clip = representer.representedItem as Clip;
        if (!IsFull())
        {
            if (clip == null)
            {
                UIManager.instance.HandleIndicator($"Can't combine {ToString()} with {representer.representedItem.ToString()}", 2f);
                return;
            }
            if (clip.isEmpty)
                UIManager.instance.HandleIndicator("Clip is empty. Can't combine.", 2f);
            else
                IncreaseAmount(clip);
        }else
            UIManager.instance.HandleIndicator("Can't combine. Clip is full.", 2f);

    }

    public void IncreaseAmount(Clip clip)
    {     
        currentAmount += clip.currentAmount;
        int remainedAmount = currentAmount - maxAmount > 0 ? currentAmount - maxAmount : 0;
        Debug.Log(currentAmount);

        if(currentAmount >= maxAmount)      
            currentAmount = maxAmount;

        clip.currentAmount = remainedAmount;

        if (currentAmount <= 0)
            isEmpty = true;
        else
            isEmpty = false;

        if (clip.currentAmount <= 0)
            clip.isEmpty = true;
        else
            clip.isEmpty = false;

    }
    public void DecreaseAmount(int decreaseValue)
    {
        currentAmount -= decreaseValue;
        if(currentAmount <= 0)
        {
            currentAmount = 0;
            isEmpty = true;
        }
                 
    }

    public bool IsFull() => currentAmount == maxAmount;

}
