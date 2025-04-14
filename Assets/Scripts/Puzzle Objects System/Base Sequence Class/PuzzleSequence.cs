using Cinemachine;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PuzzleSequence : MonoBehaviour, IInteractable
{
    public enum Player
    {
        activate,
        disable
    }
    protected PlayerController playerController => GameManager.instance.blackboard.TryGetValue("PlayerController",
      out PlayerController _controller) ? _controller : null;
    //jump to this cam to show player the sequence on OnInteract() or init this sequence on Init()
    [Header("Camera")]
    [SerializeField] protected CinemachineVirtualCamera sequenceCam;
    [SerializeField] private float playerOffsetFromCam;
    protected List<object> itemEntries;
    protected List<KeyItem> requiredItems;
    [Header("Sequence State")]
    public bool sequenceCanBeActivated = false;
    public bool onInpsect = false;
    public bool sequenceResolved = false;
    public bool puzzleSolved;
   
    protected Vector3 originalPlayerPosition = Vector3.zero;
    protected CinemachineVirtualCamera activeCamBeforeSequenceCam;
    public virtual void TryInit(List<object> itemEntries, List<KeyItem> requiredItems)
    {
        return;
    }

    public virtual void SequenceLogic()
    {
        return;
    }
    
    public virtual void OnInteract()
    {
        return;
    } 

    public virtual void OnSequenceResolved()
    {
        return;
    }
    public virtual KeyItem AssignItemsReturnIfNeeded()
    {
        return null;
    }
    public virtual void ResetSequence()
    {
        return;
    }
    public ItemEntry<KeyItem> GetKeyItemAs<T>() where T : KeyItem
    {
        var selectedEntry = 
            itemEntries.Where(x => x is ItemEntry<KeyItem> entry && 
            entry.ValType == typeof(T)).FirstOrDefault() as ItemEntry<KeyItem>; 

        return selectedEntry;
    }

    public ItemEntry<KeyItem> GetKeyItemAs<T>(int index) where T : KeyItem
    {
        ItemEntry<KeyItem> selectedEntry = itemEntries[index] is ItemEntry<KeyItem> entry 
            && entry.ValType == typeof(T) ? entry : null;
        return selectedEntry;
    }
    protected void InitItems(List<object> itemEntries, List<KeyItem> requiredItems)
    {
        this.itemEntries = itemEntries;
        this.requiredItems = requiredItems;
    }

    protected bool CanBeInit()
    {
        if(itemEntries.Count == requiredItems.Count) return true;
        else return false;
    } 
    protected void SequenceCamHandler(int priority)
    {   
        int clampedVal = Mathf.Clamp(priority,0, 2);
        clampedVal = clampedVal == 2 ? 1 : 0;;
        SetPlayerPosition(clampedVal);
        sequenceCam.Priority = priority;
    }

    protected void HandlePuzzleItemPhysics(KeyItem item)
    {
        item.GetComponent<Collider>().enabled = false;

    }

    protected void SetItemRotation(KeyItem item)
    {
        float y = sequenceCam.transform.eulerAngles.y;
        item.transform.rotation = Quaternion.Euler(0, y, 0);
    }

    protected void SetPlayerPosition(int position)
    {
      
        if (position == (int)Player.activate)
        {
            Debug.Log("back to place");
            playerController.gameObject.SetActive(true);


        }else if(position == (int)Player.disable)
        {
            Debug.Log("Away from cam");
            playerController.gameObject.SetActive(false);
           
        }
       
    }

}
