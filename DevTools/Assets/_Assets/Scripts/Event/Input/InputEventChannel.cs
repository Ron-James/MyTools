using System;
using System.Collections.Generic;
using System.Linq;
using CustomSceneReference;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "InputEventChannel", menuName = "Event Channel/InputEventChannel")]
public class InputEventChannel : SerializedScriptableObject, ISceneLoadListener
{
    [SerializeReference] private List<BaseInput> _inputEvents = new();

    [OdinSerialize] public SceneReference[] Scenes { get; protected set; }

    private void Reset()
    {
        // Optionally reset or initialize values here.
    }

    public void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        if (Scenes != null && Scenes.Any(x => x.ScenePath == scene.path))
        {
            foreach (var inputEvent in _inputEvents)
            {
                inputEvent.Setup();
            }
        }
    }
    

    public void OnSceneUnload(Scene scene)
    {
        foreach (var inputEvent in _inputEvents)
        {
            inputEvent.Dispose();
        }
    }

    public void OnStopwatchStart(Scene scene)
    {
        
    }

    public void OnStopwatchStop(Scene scene)
    {
        
    }
}


[Serializable]
public abstract class BaseInput
{
    // The found reference to the InputAction (set at runtime).
    [SerializeField] private InputActionReference _inputAction;
    //Bool to indicate if it is currently functional
    [SerializeField, ReadOnly] protected bool isInitialized;
    
    public InputAction InputAction => _inputAction;
    

    /// <summary>
    /// Call this method at runtime to find and subscribe to the desired input action.
    /// </summary>
    public void Setup()
    {
        _inputAction.asset.Enable();
        // Subscribe to events.
        _inputAction.action.performed += OnPerformed;
        _inputAction.action.started += OnStarted;

        // Ensure the action is enabled.
        _inputAction.action.Enable();

        isInitialized = true;
    }

    /// <summary>
    /// Call this when the input action is no longer needed.
    /// </summary>
    public void Dispose()
    {
        if (_inputAction != null)
        {
            _inputAction.action.performed -= OnPerformed;
            _inputAction.action.started -= OnStarted;
            _inputAction.action.Disable();
            isInitialized = false;
        }
    }

    protected abstract void OnPerformed(InputAction.CallbackContext context);

    protected abstract void OnStarted(InputAction.CallbackContext context);
}

[Serializable]
public class InputEvent : BaseInput
{
    [SerializeField] private BaseEventSO _event;

    protected override void OnPerformed(InputAction.CallbackContext obj)
    {
        Debug.Log("InputEvent OnPerformed " + _event.name);
        _event.Raise();
    }

    protected override void OnStarted(InputAction.CallbackContext obj)
    {
        Debug.Log("InputEvent OnStarted " + _event.name);
        _event.Raise();
    }
}

[Serializable]
public class InputEvent<T> : InputEvent where T : struct
{
    [SerializeField] protected new EventSO<T> _event;

    protected override void OnPerformed(InputAction.CallbackContext obj)
    {
        if (obj.valueType == typeof(T))
        {
            _event.Raise(obj.ReadValue<T>());
        }
        else
        {
            _event.Raise();
        }
    }

    protected override void OnStarted(InputAction.CallbackContext obj)
    {
        if (obj.valueType == typeof(T))
        {
            _event.Raise(obj.ReadValue<T>());
        }
        else
        {
            _event.Raise();
        }
    }
}