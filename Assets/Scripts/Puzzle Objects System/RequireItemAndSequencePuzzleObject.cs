using UnityEngine;

public class RequireItemAndSequencePuzzleObject : PuzzleObject
{
    [SerializeField] private PuzzleSequence puzzleSequence;
    public override void ApplyPuzzleLogic(ItemRepresenter representer)
    {
        if (!ContainsItem(representer.representedItem.ToString()))
        {
            Debug.Log(representer.representedItem.ToString());
            UIManager.instance.HandleIndicator("Can't use this item here.", 2f);
            return; 
        }
        AddItem(representer);
        DeleteRepresenter(representer);
        UIManager.instance.HandleInventory(false);
        puzzleSequence.TryInit(itemEntries,requiredItems);
    }
}
