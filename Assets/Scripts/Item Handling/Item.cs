using System.Collections.Generic;
using TMPro;
using UnityEngine;
public abstract class Item : MonoBehaviour, ICollectible, IInspectable
{
    [HideInInspector] public float distanceFromCam = 2;

    [Header("Item Name")]
    [SerializeField] protected string itemName;

    [Header("Original Scale")]
    public Vector3 originalScale;

    [Header("Rotation Segment")]
    public float scaleOffset;
    [SerializeField] protected float rotationSpeed;

    [Header("Conditions")]
    [SerializeField] private bool onItemCollect;

    [Header("Texts")]
    [SerializeField] protected string onFoundToSay;
    [SerializeField] protected string onInspectToSay;

    [Header("Item Image")]
    public Sprite itemImage;

    [Header("Inventory Controller")]
    [SerializeField] private InventoryController inventoryController;

    [Header("Store Items To This Cam")]
    [SerializeField] private Camera inspectorCam;

    [HideInInspector] public Rigidbody rb;
    [HideInInspector] public float cameraFOVLookingAtObject;
    private Bounds bound;
    private HashSet<string> referenceToInventory;

    private Vector3 originalItemPlace;
    private Vector3 originalRotation;

    protected void Start()
    {
        rb = GetComponent<Rigidbody>();
        bound = GetBounds();
    }
    protected void Update()
    {
        if (onItemCollect)
        {
            transform.Rotate(0, Time.deltaTime * rotationSpeed, 0);
            if (Input.GetKeyDown(KeyCode.Q))
            {
               //change later to make it ask player if he/she wants to take the item or not
               referenceToInventory.Add(ToString());
               if (inventoryController.TryAttachCollectedItemToGrid(this))
               {
                   //indicate that item is collected via UIManager
                   UIManager.GetInstance.itemCollectedPanel.SetActive(false);
                   gameObject.transform.SetParent(inspectorCam.transform);
                   gameObject.SetActive(false);
                   onItemCollect = false;
               }
               else
               {
                    UIManager.instance.HandleIndicator("Inventory is full.", 2f);
                    onItemCollect = false;
                    UIManager.instance.itemCollectedPanel.SetActive(false);
                    transform.SetParent(null);
                    transform.position = originalItemPlace;
                    transform.rotation = Quaternion.Euler(originalRotation);
                    rb.angularVelocity = Vector3.zero;
                    transform.localScale = originalScale;
  
               }
                    
            }
        }
            
    }

    private Bounds GetBounds()
    {
        Renderer[] childrenRenderers = GetComponentsInChildren<Renderer>();

        Bounds bounds = childrenRenderers[0].bounds;

        foreach(var renderer in childrenRenderers)
        {
            Bounds childBound = renderer.bounds;
            bound.Encapsulate(childBound);
        }

        return bounds;

    }
    public void OnCollect(HashSet<string> inventory)
    {
        originalItemPlace = transform.position;
        originalRotation = transform.rotation.eulerAngles;
        referenceToInventory = inventory;
        AttachObjectToCamera();
        ScaleToFitCamera();
        InitRotation();
        HandleItemCheckPanel();
    }
    public void OnInspect(TextMeshProUGUI toSay)
    {
        toSay.text = onInspectToSay;
    }

    public void OnDrop()
    {
        PlayerController playerController = 
            GameManager.instance.blackboard.TryGetValue("PlayerController", 
            out PlayerController controller) ? controller : null;
        transform.SetParent(null);
        SetCollidersActive(true);
        SetRigidBodyKinematic(false);
        transform.position = new Vector3(playerController.transform.position.x + playerController.ForwardDirection.x,
            playerController.transform.position.y, playerController.transform.position.z + playerController.ForwardDirection.z);
        transform.localScale = originalScale;
        transform.rotation = Quaternion.identity;
        gameObject.SetActive(true);
        rb.useGravity = true;
        referenceToInventory.Remove(ToString());
        if (playerController.equippedItem != null)
            if (playerController.equippedItem == this as IEquippable)
                playerController.equippedItem = null;
    }

    private void ScaleToFitCamera()
    {

        Vector3 objectSizes = bound.max - bound.min;

        Debug.Log($"Object Sizes Vector: {objectSizes} - Bound Max: {bound.max} - Bound Min: {bound.min}");

        float objectSize = Mathf.Max(objectSizes.x, objectSizes.y, objectSizes.z);

        float cameraView = Mathf.Tan(0.5f * Mathf.Deg2Rad * Camera.main.fieldOfView);

        float distance = (objectSize * 0.5f) / cameraView;

        Debug.Log($"Object Distance: {distance} - Camera View: {cameraView} - Object Size: {objectSize}");

        float scaleFactor = distanceFromCam / distance;

        transform.localScale *= scaleFactor * scaleOffset;

    }
    private void AttachObjectToCamera()
    {
        rb.useGravity = false;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;  
        transform.parent = Camera.main.transform;
        cameraFOVLookingAtObject = Camera.main.fieldOfView;
        transform.localPosition = new Vector3(0, 0, distanceFromCam);
        transform.localRotation = Quaternion.identity;

    }
    private void InitRotation() => onItemCollect = true;

    public void HandleItemCheckPanel()
    {
        UIManager.GetInstance.itemCollectedPanel.GetComponentInChildren<TextMeshProUGUI>().text = onFoundToSay;
        UIManager.GetInstance.itemCollectedPanel.SetActive(true);
    }
    private void OnValidate()
    {
        scaleOffset = Mathf.Clamp01(scaleOffset);
    }
    public override string ToString() => itemName;

    public void ResetState()
    {
        transform.SetParent(null);
        transform.localScale = originalScale;
        transform.rotation = Quaternion.identity;
        if(!gameObject.activeSelf)
            gameObject.SetActive(true);
    }

    public void SetCollidersActive(bool set)
    {
        Collider[] colliders = GetComponentsInChildren<Collider>();

        foreach (var collider in colliders)
            collider.enabled = set;
    }

    public void SetRigidBodyKinematic(bool set)
    {
        if(set)
            rb.isKinematic = true;
        else
            rb.isKinematic = false;
    }
    

}
