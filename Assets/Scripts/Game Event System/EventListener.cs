
using UnityEngine;

public class EventListener : MonoBehaviour
{
    public EventController eventController;
    public delegate void EventHandler();
    public event EventHandler eventHandler;

    public void AddEvent(EventHandler funcEvent) => eventHandler += funcEvent;
    public void RemoveEvent(EventHandler funcEvent) => eventHandler -= funcEvent;

    public void AddListener() => eventController.AddListener(this);
    public void RemoveListener() => eventController.RemoveListener(this);
    public void CallEvent() => eventHandler?.Invoke();

    private void OnEnable() => AddListener();

    private void OnDisable() => RemoveListener();
 

}
