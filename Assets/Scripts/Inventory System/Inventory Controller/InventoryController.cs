using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryController : MonoBehaviour
{
    [SerializeField] private List<InventoryGrid> inventoryGrids;
    public GridMenu gridMenu;

    private void Update()
    {
        if(Input.GetMouseButtonDown(0) && !CheckIfMouseInsideMenu())
            gridMenu.gameObject.SetActive(false);
    }

    private bool CheckIfMouseInsideMenu()
    {
        bool isInside = 
            RectTransformUtility.ScreenPointToLocalPointInRectangle(gridMenu.rectTransform, 
            Input.mousePosition, null, out Vector2 localPoint);
        return gridMenu.rectTransform.rect.Contains(localPoint) && isInside;
    }
    public bool TryGetGrid(ItemRepresenter representer, out InventoryGrid gridToGet)
    {
        Vector2 representerScreenPoint = representer.rectTransform.position;
        foreach(var grid in inventoryGrids)
        {
            RectTransform gridRect = grid.GetComponent<RectTransform>();
            if(RectTransformUtility.ScreenPointToLocalPointInRectangle(gridRect, representerScreenPoint, null, out Vector2 localPoint) 
                && gridRect.rect.Contains(localPoint))
            {
                gridToGet = grid;
                return true ;
            }
            
        }
        gridToGet = default;

        return false;
    }

    public bool TryAttachCollectedItemToGrid(Item item)
    {
        if(TryGetEmptyGrid(out InventoryGrid grid))
        {
            GameObject itemRepresenter = new GameObject(item.name);
            itemRepresenter.AddComponent<CanvasRenderer>();
            itemRepresenter.AddComponent<RectTransform>();
            itemRepresenter.AddComponent<Image>();
            ItemRepresenter representer = itemRepresenter.AddComponent<ItemRepresenter>();
            representer.InitRepresenter(item,grid,this,item.cameraFOVLookingAtObject);
            return true;
        }
        else
             return false;
    
    }

    public bool TryGetEmptyGrid(out InventoryGrid emptyGrid)
    {
        foreach(var grid in inventoryGrids)
        {
            if(grid.Representer == null)
            {
                emptyGrid = grid;
                return true;
            }
                
        }
        emptyGrid = default;
        return false;
    }
}
