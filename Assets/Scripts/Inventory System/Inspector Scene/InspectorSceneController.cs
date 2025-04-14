using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InspectorSceneController : MonoBehaviour
{
    public Camera inspectorCam;
    public Item objectToBeInspected;
    private Vector3 lastMousePosition;
    private RawImage rawImage;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private TextMeshProUGUI inspectionText;
    [Header("Conditions")]
    [SerializeField] private bool isPressed;
    PlayerController playerController => GameManager.instance.blackboard.TryGetValue("PlayerController",
        out PlayerController _controller) ? _controller : null;

    private void Start()
    {
        rawImage = GetComponent<RawImage>();
    }

    private void Update()
    {
        if (CastRayAndCheckIfInBoundaries(out Ray ray))
        {
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide) &&
                Input.GetMouseButtonDown(0))
            {
                Debug.Log($"Hitted object: {hit.transform.name}");
                if (hit.collider.transform.TryGetComponent<IInspectable>(out IInspectable interactedPart))
                {
                    interactedPart.OnInspect(inspectionText);
                }
            }
            if (Input.GetMouseButtonDown(1))
            {
                isPressed = true;
                lastMousePosition = Input.mousePosition;
               
            }

        }

        if (Input.GetMouseButtonUp(1))
            isPressed = false;

        if (isPressed && objectToBeInspected != null)
        {
            Vector3 deltaMousePos = (Input.mousePosition - lastMousePosition);

            objectToBeInspected.transform.Rotate(Vector3.up, -1 * deltaMousePos.x * rotationSpeed * Time.deltaTime, Space.World);
            objectToBeInspected.transform.Rotate(Vector3.right, deltaMousePos.y * rotationSpeed * Time.deltaTime, Space.World);

            lastMousePosition = Input.mousePosition;

        }

    }

    private bool CastRayAndCheckIfInBoundaries(out Ray ray)
    {
        RectTransform rectTransform = rawImage.rectTransform;

        bool isInBoundaries = RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, Input.mousePosition, null, out Vector2 localMousePos)
            && rectTransform.rect.Contains(localMousePos);

        Vector2 normalizedUV = new Vector2(
            Mathf.InverseLerp(-rectTransform.rect.width / 2, rectTransform.rect.width / 2, localMousePos.x),
            Mathf.InverseLerp(-rectTransform.rect.height / 2, rectTransform.rect.height / 2, localMousePos.y)
        );

        Vector3 viewportPoint = new Vector3(normalizedUV.x, normalizedUV.y, 0);
      
        ray = inspectorCam.ViewportPointToRay(viewportPoint);
      
        return isInBoundaries;
    }

    public void ClearInspectionText()
    {
        inspectionText.text = " ";
    }

    private void OnDisable()
    {
        if(objectToBeInspected != null)
        {
            inspectionText.text = " ";
            if(objectToBeInspected.TryGetComponent<IEquippable>(out IEquippable equippableItem) && playerController.equippedItem != null)
            {
                if(playerController.equippedItem == objectToBeInspected as IEquippable)
                    equippableItem.Equip(playerController);
                else
                    objectToBeInspected.gameObject.SetActive(false);
            }
                
            else
                objectToBeInspected.gameObject.SetActive(false);

            objectToBeInspected = null;
        }
       
    }
}
