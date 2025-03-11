using System;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class VariableReference<T>
{
    [SerializeField] protected EventSO<T> _event;
    

    [SerializeField] private bool _isConstant = false;
    [SerializeField] protected T _value;
    
    
    
    public EventSO<T> Event => _event;
    
    [ShowInInspector]
    public T Value
    {
        get
        {
            return _isConstant ? _value : _event.LastValueRaised;
        }
        set
        {
            _value = value;
            _event.Raise(value);
        }
    }
    
    
}