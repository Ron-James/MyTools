using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;


[CreateAssetMenu(fileName = "VoidEventSO", menuName = "Custom Events/VoidEventSO")]
public class VoidEventSO : BaseEventSO
{
    [SerializeReference] private List<BaseEventSubscriber> _subscribers = new List<BaseEventSubscriber>();
    
    public override void OnSceneUnload(Scene scene)
    {
        _subscribers.Clear();
        if(persistThroughSceneChanges) return;
        HasRaised = false;
    }

    public override void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        base.OnSceneLoad(scene, mode);
        
    }

    public override void Raise()
    {
        for(int index = _subscribers.Count - 1; index >= 0; index--)
        {
            _subscribers[index].Response?.Invoke();
        }
        HasRaised = true;
        
    }

    public override void Subscribe(Object origin, string methodName, UnityAction response)
    {
        BaseEventSubscriber subscriber = new BaseEventSubscriber(origin, methodName, response);
        _subscribers.Add(subscriber);
    }

    public override void UnsubscribeAll(Object origin)
    {
        for(int loop = _subscribers.Count - 1; loop >= 0; loop--)
        {
            if(_subscribers[loop].Origin == origin)
            {
                _subscribers[loop] = null;
            }
        }
    }

    public override void Unsubscribe(Object origin, string methodName)
    {
        for(int index = _subscribers.Count - 1; index >= 0; index--)
        {
            if(_subscribers[index].Origin == origin && _subscribers[index].Origin.name == methodName)
            {
                _subscribers[index] = null;
            }
        }
    }
}