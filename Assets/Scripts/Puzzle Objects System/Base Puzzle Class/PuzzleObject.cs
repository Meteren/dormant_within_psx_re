using System.Collections.Generic;
using UnityEngine;

public abstract class PuzzleObject : MonoBehaviour
{
    public HashSet<string> requiredItemNames = new HashSet<string>();
    [SerializeField] protected List<KeyItem> requiredItems = new List<KeyItem>();
    protected HashSet<string> inventory => GameManager.instance.blackboard.TryGetValue("PlayerController", 
        out PlayerController controller) ? controller.inventory : null;

    public List<object> itemEntries = new List<object>();

    private void Start()
    {
        ExtractItems();
    }
    private void ExtractItems()
    {
        foreach (var item in requiredItems)
            requiredItemNames.Add(item.ToString());

    }
    protected void AddItem(ItemRepresenter representer)
    {
        KeyItem item = representer.representedItem as KeyItem;
        ItemEntry<KeyItem> newEntry = new ItemEntry<KeyItem>(item);
        itemEntries.Add(newEntry);
    }

    protected bool ContainsItem(string name)
    {
        if (requiredItemNames.Contains(name)) return true;
        else return false;

    }

    protected void DeleteRepresenter(ItemRepresenter representer)
    {
        representer.attachedGrid.DetachRepresenter(); 
        Destroy(representer.gameObject);
    }

 
    public virtual void ApplyPuzzleLogic(ItemRepresenter representer)
    {
        Debug.Log("Don't have any sequence.");
    }

}
