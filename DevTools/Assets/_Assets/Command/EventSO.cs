using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using CustomSceneReference;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;
using UnityEngine.SceneManagement;

public interface IEvent
{
    void Raise();
    void Subscribe(UnityEngine.Object origin, string methodName, UnityAction response);
    void UnsubscribeAll(UnityEngine.Object origin);
    void Unsubscribe(UnityEngine.Object origin, string methodName);
    
    bool HasRaised {get;}
}
public interface IEvent<T> : IEvent
{
    void Raise(T value);
    void Subscribe(UnityEngine.Object origin, string methodName, UnityAction<T> response);
    
}



public class BaseEventSubscriber
{
    [SerializeField] private Object _origin;
    [SerializeField] protected string _methodName;
    [SerializeField] protected UnityAction _response;
    
    
    public UnityAction Response => _response;

    public Object Origin
    {
        get => _origin;
        set => _origin = value;
    }

    public BaseEventSubscriber()
    {
        
    }
    
    public BaseEventSubscriber(Object origin, string methodName, UnityAction response)
    {
        Origin = origin;
        _methodName = methodName;
        _response = response;
    }
    
    
    
}

[ShowOdinSerializedPropertiesInInspector]
[Serializable]
public class EventSubscriber<T> : BaseEventSubscriber
{
    [OdinSerialize] private new UnityAction<T> _response;
    
    public EventSubscriber(Object origin, string methodName, UnityAction<T> response) 
    {
        base.Origin = origin;
        _methodName = methodName;
        this.response = new(response);
    }

    public UnityAction<T> response
    {
        get => _response;
        protected set => _response = value;
    }

    public Object Origin => base.Origin;
}

[CreateAssetMenu(fileName = "EventSO", menuName = "Scriptable Objects/EventSO")]
public abstract class BaseEventSO : SerializedScriptableObject, IEvent, ISceneLoadListener
{
    [SerializeReference] protected List<BaseEventSubscriber> _subscribers = new List<BaseEventSubscriber>();
    [SerializeField] protected bool persistThroughSceneChanges = false;
    public abstract void OnSceneUnload(Scene scene);
    
    [OdinSerialize]
    public SceneReference[] Scenes { get; protected set; }

    public virtual void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        
    }
    
    
    [Button]
    public abstract void Raise();


    public abstract void Subscribe(Object origin, string methodName, UnityAction response);


    public abstract void UnsubscribeAll(Object origin);

    public abstract void Unsubscribe(Object origin, string methodName);
    
    [OdinSerialize]
    public bool HasRaised { get; protected set; }
    

    protected void RemoveNullSubscribers()
    {
        for(int loop = _subscribers.Count - 1; loop >= 0; loop--)
        {
            if(_subscribers[loop] == null)
            {
                _subscribers.RemoveAt(loop);
            }
        }
    }

    
}



public abstract class EventSO<T> : BaseEventSO, IEvent<T>
{
    [SerializeField] private T _defaultValue;
    [SerializeReference] protected new List<EventSubscriber<T>> _subscribers = new List<EventSubscriber<T>>();
    
    
    
    [ShowInInspector]
    public T LastValueRaised {get; private set;}
    
    
    public virtual void Raise(T value)
    {
        
        RemoveNullSubscribers();
        for(int index = _subscribers.Count - 1; index >= 0; index--)
        {
             _subscribers[index].response?.Invoke(value);
        }
        this.HasRaised = true;
        LastValueRaised = value;

    }
    
    public override void Raise()
    {
        RemoveNullSubscribers();
        for(int index = _subscribers.Count - 1; index >= 0; index--)
        {
            _subscribers[index].response?.Invoke(_defaultValue);
        }
        HasRaised = true;
        LastValueRaised = _defaultValue;
    }
    
    
    
    public override void Subscribe(Object origin, string method, UnityAction response)
    {
        EventSubscriber<T> subscriber = new EventSubscriber<T>(origin, method, (value) => response());
        _subscribers.Add(subscriber);
    }

    public virtual void Subscribe(Object origin, string methodName, UnityAction<T> response)
    {
        EventSubscriber<T> subscriber = new EventSubscriber<T>(origin, methodName, response);
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


    public override void OnSceneUnload(Scene scene)
    {
        RemoveNullSubscribers();
        if(persistThroughSceneChanges) return;
        HasRaised = false;
        LastValueRaised = _defaultValue;
        _subscribers.Clear();
        
    }
}


public class EventSubscriber<T1, T2> : BaseEventSubscriber
{
    [OdinSerialize] private new UnityAction<T1, T2> _response;
    
    public EventSubscriber(Object origin, string methodName, UnityAction<T1, T2> response) 
    {
        base.Origin = origin;
        _methodName = methodName;
        this.response = new(response);
    }

    public UnityAction<T1, T2> response
    {
        get => _response;
        protected set => _response = value;
    }

    public Object Origin => base.Origin;
} 

public interface IEvent<T1, T2> : IEvent
{
    void Raise(T1 value1, T2 value2);
    void Subscribe(Object origin, string methodName, UnityAction<T1, T2> response);
}
public class EventSO<T1, T2> : BaseEventSO, IEvent<T1, T2>
{
    [SerializeReference, TableList] protected new List<EventSubscriber<T1, T2>> _subscribers = new();
    [SerializeField] protected T1 _defaultValue1;
    [SerializeField] protected T2 _defaultValue2;
    
    [SerializeField] private T1 _lastValue1;
    [SerializeField] private T2 _lastValue2;
    
    [SerializeField, ReadOnly] private bool hasRaised;
    
    
    public T1 LastValue1
    {
        get => _lastValue1;
        private set => _lastValue1 = value;
    }
    
    public T2 LastValue2
    {
        get => _lastValue2;
        private set => _lastValue2 = value;
    }
    public override void OnSceneUnload(Scene scene)
    {
        RemoveNullSubscribers();
        if(persistThroughSceneChanges) return;
        _subscribers.Clear();
        hasRaised = false;
        _lastValue1 = _defaultValue1;
        _lastValue2 = _defaultValue2;
    }

    public override void Raise()
    {
        RemoveNullSubscribers();
        for(int index = _subscribers.Count - 1; index >= 0; index--)
        {
            _subscribers[index].response?.Invoke(_defaultValue1, _defaultValue2);
        }
        _lastValue1 = _defaultValue1;
        _lastValue2 = _defaultValue2;
        hasRaised = true;
    }
    
    [Button]
    public virtual void Raise(T1 value1, T2 value2)
    {
        RemoveNullSubscribers();
        for(int index = _subscribers.Count - 1; index >= 0; index--)
        {
            _subscribers[index].response?.Invoke(value1, value2);
        }
        _lastValue1 = value1;
        _lastValue2 = value2;
        hasRaised = true;
    }

    public virtual void Subscribe(Object origin, string methodName, UnityAction<T1, T2> response)
    {
        EventSubscriber<T1, T2> subscriber = new EventSubscriber<T1, T2>(origin, methodName, response);
        _subscribers.Add(subscriber);
    }

    public override void Subscribe(Object origin, string methodName, UnityAction response)
    {
        EventSubscriber<T1, T2> subscriber = new EventSubscriber<T1, T2>(origin, methodName, (value1, value2) => response());
        _subscribers.Add(subscriber);
    }

    public override void UnsubscribeAll(Object origin)
    {
        
        //Remove all subscribers with the same origin
        for(int loop = _subscribers.Count - 1; loop >= 0; loop--)
        {
            if(_subscribers[loop].Origin == origin)
            {
                _subscribers[loop] = null;
            }
        } 
        RemoveNullSubscribers();
    }

    public override void Unsubscribe(Object origin, string methodName)
    {
        //Remove all subscribers with the same origin and method name
        for(int index = _subscribers.Count - 1; index >= 0; index--)
        {
            if(_subscribers[index].Origin == origin && _subscribers[index].Origin.name == methodName)
            {
                _subscribers[index] = null;
            }
        }
        RemoveNullSubscribers();
    }
}


public static class EventExtensions
{
    public static void Subscribe(this UnityEngine.Object origin, BaseEventSO eventSO, string methodName, UnityAction response)
    {
        eventSO.Subscribe(origin, methodName, response);
    }
    
    public static void Subscribe<T>(this UnityEngine.Object origin, EventSO<T> eventSO, string methodName, UnityAction<T> response)
    {
        eventSO.Subscribe(origin, methodName, response);
    }
}



