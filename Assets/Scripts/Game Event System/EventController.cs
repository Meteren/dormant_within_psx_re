using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(menuName = "EventController")]
public class EventController : ScriptableObject
{

    List<EventListener> eventListeners = new List<EventListener>();
    
    public void AddListener(EventListener listener) => eventListeners.Add(listener);
    public void RemoveListener(EventListener listener) => eventListeners.Remove(listener);

    public void ExecuteListeners()
    {
        foreach (var listener in eventListeners)
            listener.CallEvent();
    }



}
