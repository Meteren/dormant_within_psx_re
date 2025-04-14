using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
   
    private PuzzleSequence sequence;
    public int X {  get; private set; }
    public int Y { get; private set; }

    [Header("Grid")]
    public Grid[,] grids;

    [HideInInspector]
    public int[] cursor = new int[2] {0,0};
    public Grid CurrentHoveredGrid { get; private set; }
    public Grid PrevHoveredGrid { get; private set; }

    public Grid SelectedGrid { get; private set; }

    bool waitForSet;

    [Header("Event Handling")]
    [SerializeField] private EventController eventController;
    [SerializeField] private EventListener listener;

    private void Awake()
    {
        listener.AddEvent(HandlePositionAndState);
    }
    private void Start()
    {
        sequence = GetComponent<PuzzleSequence>();
    }

    private void Update()
    {
        if (sequence.onInpsect && sequence.sequenceCanBeActivated)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if(SelectedGrid == null)
                {
                    CurrentHoveredGrid.GetItem().OnSelect();
                    SelectedGrid = CurrentHoveredGrid;
                }
                else
                {
                    eventController.ExecuteListeners();
                }
                
            }
                      
            PrevHoveredGrid = CurrentHoveredGrid;

            if (!waitForSet)
            {
                StartCoroutine(SetCursor());
            }
            CurrentHoveredGrid = grids[cursor[0], cursor[1]];
            if (CurrentHoveredGrid != null && CurrentHoveredGrid != PrevHoveredGrid)
            {
                if(PrevHoveredGrid != null && PrevHoveredGrid != SelectedGrid)
                    PrevHoveredGrid.GetItem().OnHoverExit();
                CurrentHoveredGrid.GetItem().OnHoverEnter();
            }

                
        }
    }
    public void Init(List<Grid> gridList, int x, int y)
    {
        X = x;
        Y = y;
        grids = new Grid[X, Y];
        ExtractGridList(gridList);
    }

    private void ExtractGridList(List<Grid> gridList)
    {
        int index = 0;
        for(int i = 0; i < grids.GetLength(0); i++)
        {
            for(int j = 0; j < grids.GetLength(1); j++)
            {
                grids[i, j] = gridList[index];
                index++;
            }
        }
    }
    
    private IEnumerator SetCursor()
    {
        (int, int) input = GetInput();
        cursor[0] += input.Item1;
        cursor[1] += input.Item2;
        ClampCursor();
        waitForSet = true;
        yield return new WaitForSecondsRealtime(0.06f);
        waitForSet = false;
        
    }
    private (int,int) GetInput()
    {
        int x = (int)Input.GetAxisRaw("Vertical") * -1;
        int y = (int)Input.GetAxisRaw("Horizontal");
        return (x, y); 
    }

    private void ClampCursor()
    {
        cursor[1] = cursor[1] < grids.GetLength(0) && cursor[1] >= 0 ? cursor[1]
            : (cursor[1] > grids.GetLength(0) - 1 ? grids.GetLength(0) - 1 : 0);
        cursor[0] = cursor[0] < grids.GetLength(1) && cursor[0] >= 0 ? cursor[0] 
            : (cursor[0] > grids.GetLength(1) - 1 ? grids.GetLength(1) - 1 : 0);
    }

    private void HandlePositionAndState()
    {
        KeyItem selectedItem = SelectedGrid.GetItem();
        Vector3 selectedPosition = selectedItem.transform.position; 
        selectedItem.transform.position = CurrentHoveredGrid.GetItem().transform.position;
        CurrentHoveredGrid.GetItem().transform.position = selectedPosition;
        SelectedGrid.AssignItem(CurrentHoveredGrid.GetItem());
        CurrentHoveredGrid.AssignItem(selectedItem);
        CurrentHoveredGrid.GetItem().ResetOutlineColor();
        if(SelectedGrid.GetItem() != CurrentHoveredGrid.GetItem())
            SelectedGrid.GetItem().DisableOutline();
        SelectedGrid = null;
    }
    public void ResetGrid()
    {
        if(CurrentHoveredGrid != null)
        {
            CurrentHoveredGrid.GetItem().ResetOutlineColor();
            CurrentHoveredGrid.GetItem().DisableOutline();
            CurrentHoveredGrid = null;
        }

        PrevHoveredGrid = null;

        if(SelectedGrid != null)
        {
            SelectedGrid.GetItem().ResetOutlineColor();
            SelectedGrid.GetItem().DisableOutline();
            SelectedGrid = null;
        }       
       
    }
}
