using Unity.Netcode;
using UnityEngine;


public class PlayerNetworkDataController : NetworkBehaviour
{
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

public class VariableReference<T>
{
    [SerializeField] protected EventSO<T> _event;
    [SerializeField] protected T _value;
    
    public EventSO<T> Event => _event;
    
}
public class NetworkedVariableReference <T> : VariableReference<T>
{
    
}
