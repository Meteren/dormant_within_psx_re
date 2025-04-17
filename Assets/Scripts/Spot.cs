using Cinemachine;
using System.Linq;
using UnityEngine;

public class Spot : MonoBehaviour
{
    PlayerController playerController => GameManager.instance.blackboard.
        TryGetValue("PlayerController", out PlayerController _controller) ? _controller : null;
    [SerializeField]private Camera cam;
    [Header("Conditions")]
    [SerializeField] bool canLookAt;
  
    private void Update()
    {
        if (canLookAt && cam != null)
        {
            Vector3 playerDirection = (playerController.transform.position - cam.transform.position).normalized;
            Quaternion lookAt = Quaternion.LookRotation(playerDirection);
            cam.transform.rotation = lookAt;
        }
        
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            cam.gameObject.SetActive(true);
            cam.tag = "MainCamera";
      
        }
           
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            cam.gameObject.SetActive(false);
            cam.tag = "Untagged";
        }
            
    }

   
}
