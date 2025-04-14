using TMPro;
using UnityEngine;

public class LockedObject : PuzzleObject, IInteractable
{
    [SerializeField] private string onInteractText;
    private bool isItUnlocked = false;
    public override void ApplyPuzzleLogic(ItemRepresenter representer)
    {
        if (!ContainsItem(representer.representedItem.ToString()))
        {
            UIManager.instance.HandleIndicator("Can't use this item here",2f);
            return;
        }
        UIManager.instance.HandleInventory(false);
        UIManager.instance.HandleIndicator("Item unlocked the door.", 2f);
        DeleteRepresenter(representer);
        isItUnlocked = true;
    }

    public void OnInteract()
    {
        if (!isItUnlocked)
            UIManager.instance.HandleIndicator(onInteractText,2f);
        else
        {
            Debug.Log("Door opened and passed to the new area.");
            gameObject.SetActive(false);
        }
            
    }
}
