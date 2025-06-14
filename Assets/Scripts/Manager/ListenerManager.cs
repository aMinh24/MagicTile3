using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public enum EventID
{
    None,
    OnTileInteract,
    OnTileToEnd,
    SetupGameScreen,
    TouchWrong,
}
public class ListenerManager : BaseManager<ListenerManager>
{
    private Dictionary<EventID, Action<object>> listeners = new Dictionary<EventID, Action<object>>();

    #region Register, Unregister, broadcast
    public void Register(EventID id, Action<object> action)
    {
        if (action == null) { return; }
        if (listeners.ContainsKey(id))
        {
            if (listeners[id] != null)
                if (!listeners[id].GetInvocationList().Contains(action))
                    listeners[id] += action;
        }
        else
        {
            listeners.Add(id, (obj) => { });
            listeners[id] += action;
        }
    }
    public void Unregister(EventID id, Action<object> action)
    {

        if (listeners.ContainsKey(id) && action != null)
        {
            if (listeners[id].GetInvocationList().Contains(action))
                listeners[id] -= action;
        }
    }
    public void UnregisterAll(EventID id)
    {
        if (listeners.ContainsKey(id))
        {
            listeners.Remove(id);
        }
    }
    public void Broadcast(EventID id, object data = null)
    {
        if (listeners.ContainsKey(id))
        {
            listeners[id].Invoke(data);
        }
    }
    public void DelayBroadcast(EventID id, float time, object data = null)
    {
        Debug.Log("delay time");
        StartCoroutine(DelayBroadcastCoroutine(id, time, data));
    }
    private IEnumerator DelayBroadcastCoroutine(EventID id, float time, object data = null)
    {
        yield return new WaitForSeconds(time);
        Instance.Broadcast(id, data);
    }
    #endregion
}
public static class ListenerManagerExtension
{
    public static void Register(this MonoBehaviour listener, EventID id, Action<object> action)
    {
        if (ListenerManager.HasInstance)
        {
            ListenerManager.Instance.Register(id, action);
        }
    }
    public static void Unregister(this MonoBehaviour listener, EventID id, Action<object> action)
    {
        if (ListenerManager.HasInstance)
        {
            ListenerManager.Instance.Unregister(id, action);
        }
    }
    public static void UnregisterAll(this MonoBehaviour listener, EventID id)
    {
        if (ListenerManager.HasInstance)
        {
            ListenerManager.Instance.UnregisterAll(id);
        }
    }
    public static void Broadcast(this MonoBehaviour listener, EventID id)
    {
        if (ListenerManager.HasInstance)
        {
            //Debug.Log(id.ToString());
            ListenerManager.Instance.Broadcast(id, null);
        }
    }
    public static void Broadcast(this MonoBehaviour listener, EventID id, object data)
    {
        if (ListenerManager.HasInstance)
        {
            ListenerManager.Instance.Broadcast(id, data);
        }
    }
    public static void DelayBroadcast(this MonoBehaviour listener, EventID id, float time, object data = null)
    {
        if (ListenerManager.HasInstance)
        {
            ListenerManager.Instance.DelayBroadcast(id, time, data);
        }
    }
}
