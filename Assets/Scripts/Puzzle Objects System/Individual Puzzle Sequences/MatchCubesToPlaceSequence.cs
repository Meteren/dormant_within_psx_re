using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MatchCubesToPlaceSequence : PuzzleSequence
{
    [SerializeField] private EventListener listener;

    int entryIndex = 0;
    [Header("Needed Items")]
    [SerializeField] private Cube cubeOne;
    [SerializeField] private Cube cubeTwo;
    [SerializeField] private Cube cubeThree;
    [SerializeField] private Cube cubeFour;

    [Header("Placemenets")]
    [SerializeField] private List<Grid> placements;

    [Header("Text")]
    [SerializeField] private string onNotActivatedToSay;

    private List<Cube> assignedItems = new List<Cube>();

    private GridManager gridManager;

    [Header("MovePlace")]
    [SerializeField] private Transform movePlace;

    float slideSpeed = 7f;
    private void Start()
    {     
        gridManager = GetComponent<GridManager>();
        listener.AddEvent(SequenceLogic);
    }
    private void Update()
    {
      
        if (onInpsect && Input.GetKeyDown(KeyCode.Q))
        {
            SequenceCamHandler(0);
            onInpsect = false;
            if (gridManager != null)
                gridManager.ResetGrid();
        }

        if (sequenceResolved)
        {
            OnSequenceResolved();
        }     
        
    }
    public override void TryInit(List<object> itemEntries, List<KeyItem> requiredItems)
    {
        KeyItem itemToBePlaced = null;
        InitItems(itemEntries, requiredItems);
        if (itemEntries.Count != entryIndex)
        {
            itemToBePlaced = AssignItemsReturnIfNeeded();
        }   
        if (itemToBePlaced != null)
        {
            StartCoroutine(PlaceItem(placements[entryIndex], itemToBePlaced));
        }
            
        if (CanBeInit())
        {
            Debug.Log("Puzzle Started");
            SequenceCamHandler(2);
            onInpsect = true;
            sequenceCanBeActivated = true;
            gridManager.Init(placements,(int)Mathf.Sqrt(placements.Count), (int)Mathf.Sqrt(placements.Count));
            //init the puzzle sequence write the logic of this puzzle
        }
    }
    public override void SequenceLogic()
    {
        foreach(var grid in placements)
        {
            int itemNumber = Convert.ToInt32(grid.GetItem().GetComponentInChildren<TextMeshProUGUI>().text);
            int gridNumber = Convert.ToInt32(grid.GetComponentInChildren<TextMeshProUGUI>().text);
            if (itemNumber != gridNumber)
                return;
        }
        sequenceResolved = true;
    }

    public override void OnSequenceResolved()
    {
        SequenceCamHandler(0);
        onInpsect = false;
        sequenceCanBeActivated = false;
        gridManager.ResetGrid();
        transform.parent.position = Vector3.MoveTowards(transform.parent.position, movePlace.position, Time.deltaTime * slideSpeed);
        if (!puzzleSolved)
            return;
        UIManager.instance.HandleIndicator("Puzzle Solved",2f);
        puzzleSolved = true;

    }

    public override void OnInteract()
    {
        if (!sequenceResolved)
        {
            if (!sequenceCanBeActivated)
            {
                onInpsect = true;
                SequenceCamHandler(2);
                UIManager.instance.HandleIndicator(onNotActivatedToSay, 2f);
            }
            else
                TryInit(itemEntries, requiredItems);
        }
       
    }

    public override KeyItem AssignItemsReturnIfNeeded()
    {
        KeyItem reference;
        if (!assignedItems.Contains(cubeOne))
        {
            cubeOne = GetKeyItemAs<Cube>(entryIndex).Value as Cube;
            assignedItems.Add(cubeOne);
            reference = cubeOne;
        }
        else if (!assignedItems.Contains(cubeTwo))
        {
            cubeTwo = GetKeyItemAs<Cube>(entryIndex).Value as Cube;
            assignedItems.Add(cubeTwo);
            reference = cubeTwo;
        }
        else if (!assignedItems.Contains(cubeThree))
        {
            cubeThree = GetKeyItemAs<Cube>(entryIndex).Value as Cube;
            assignedItems.Add(cubeThree);
            reference = cubeThree;
        }
        else if (!assignedItems.Contains(cubeFour))
        {
            cubeFour = GetKeyItemAs<Cube>(entryIndex).Value as Cube;
            assignedItems.Add(cubeFour);
            reference = cubeFour;
        }
        else
        {
            reference = null;
        }

        placements[entryIndex].AssignItem(reference);
         
        return reference;

    }
    public IEnumerator PlaceItem(Grid grid, KeyItem itemToBePlaced)
    {     
        SequenceCamHandler(2);
        itemToBePlaced.transform.position = grid.transform.position;
        itemToBePlaced.ResetState();
        float y = sequenceCam.transform.eulerAngles.y;
        HandlePuzzleItemPhysics(itemToBePlaced);
        SetItemRotation(itemToBePlaced);
        entryIndex++;
        UIManager.instance.HandleIndicator("Item Placed",1.5f); 
        yield return new WaitForSecondsRealtime(1.5f);
        if(!sequenceCanBeActivated)
            SequenceCamHandler(0);
        itemToBePlaced.transform.SetParent(transform);
    }

    private void OnDisable()
    {
        playerController.interactedPuzzleObject = null;

    }

}
