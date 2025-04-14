using TMPro;
using UnityEngine;

public class GridMenu : MonoBehaviour
{
    public RectTransform rectTransform;
    [Header("Item")]
    public ItemRepresenter representer;
    [Header("Inspector Scene Controller")]
    public InspectorSceneController inspectorSceneController;

    public PlayerController playerController => GameManager.instance.blackboard.TryGetValue("PlayerController", 
        out PlayerController _controller) ? _controller : null;

    public void InitGridMenu(ItemRepresenter representer)
    {
        gameObject.SetActive(true);
        Vector2 center = representer.rectTransform.position;
        rectTransform.position = new Vector2(center.x + rectTransform.rect.width / 2, center.y - rectTransform.rect.height / 2);
        this.representer = representer;
    }

    public void OnPressEquipButton()
    {
        Debug.Log("Item Equipped");
        inspectorSceneController.ClearInspectionText();
        Item item = representer.representedItem;
        if(item.TryGetComponent<IEquippable>(out IEquippable equippableItem))
        {      
            if (item as IEquippable != playerController.equippedItem)
            {
                if(playerController.equippedItem != null)
                    playerController.equippedItem.Unequip();
                equippableItem.Equip(playerController);
                gameObject.SetActive(false);
            }
            else
                UIManager.instance.HandleIndicator("Already equipped.", 2f);
        }
        else
            UIManager.instance.HandleIndicator("Can't equip this item", 2f);

    }
    public void OnPressUseButton()
    {
        if (playerController != null)
            if (playerController.interactedPuzzleObject != null)
                playerController.interactedPuzzleObject.ApplyPuzzleLogic(representer);
            else
                UIManager.instance.HandleIndicator("Can't use this item here.",2f);
    }

    public void OnPressInspectButton()
    {
        Item gameObjectToBeInspected = inspectorSceneController.objectToBeInspected;
        if (gameObjectToBeInspected != null)
        {
            if (gameObjectToBeInspected.gameObject.activeSelf)
                gameObjectToBeInspected.gameObject.SetActive(false);
            if (gameObjectToBeInspected.TryGetComponent<IEquippable>(out IEquippable equippableItem))
                if(playerController.equippedItem != null)
                    equippableItem.Equip(playerController);
        }
              
        Item item = representer.representedItem;
        Debug.Log("Inspect button pressed");
        item.transform.SetParent(inspectorSceneController.inspectorCam.transform);
        item.transform.localPosition = new Vector3(0, 0, item.distanceFromCam);
        item.transform.localRotation = Quaternion.identity;
        item.transform.localScale = representer.itemInspectorCamScale;
        //item.rb.velocity = Vector3.zero;
        item.SetCollidersActive(true);
        inspectorSceneController.objectToBeInspected = item;
        inspectorSceneController.ClearInspectionText();
        item.gameObject.SetActive(true);
        inspectorSceneController.inspectorCam.fieldOfView = item.cameraFOVLookingAtObject;
        gameObject.SetActive(false);
    }
    public void OnPressDropButton()
    {
        inspectorSceneController.ClearInspectionText();
        inspectorSceneController.objectToBeInspected = null;
        representer.representedItem.OnDrop();
        representer.attachedGrid.DetachRepresenter();
        Destroy(representer.gameObject);
        gameObject.SetActive(false);

    }

    private void OnDisable()
    {
        gameObject.SetActive(false);
    }
}
