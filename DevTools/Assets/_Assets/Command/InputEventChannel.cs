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
    [SerializeReference] private List<BaseInputEvent> _inputEvents = new();

    [OdinSerialize]
    public SceneReference[] Scenes { get; protected set; }

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

    [Serializable]
    public class BaseInputEvent
    {
        [SerializeField] protected BaseEventSO _event;

        [SerializeField] protected InputAction _inputAction;
        
        public InputAction InputAction => _inputAction;
        public BaseEventSO Event => _event;

        // Constructors without calling Setup().
        public BaseInputEvent() { }
        
        public BaseInputEvent(InputAction inputAction, BaseEventSO eventSO)
        {
            _inputAction = inputAction;
            _event = eventSO;
        }
        


        [Button]
        public void Setup()
        {
            Debug.LogError("Setup for " + InputAction.name);
            try{
                _inputAction.Enable();
                _inputAction.performed += OnPerformed;
                _inputAction.started   += OnStarted;
                
            }
            catch (Exception e)
            {
                Debug.LogError($"Error in Setup: {e}");
            }
        }

        [Button]
        public void Dispose()
        {
            try
            {
                if (_inputAction != null)
                {
                    _inputAction.performed -= OnPerformed;
                    _inputAction.started -= OnStarted;
                    _inputAction.Disable();
                    
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error in Dispose: {e}");
            }
        }

        protected virtual void OnPerformed(InputAction.CallbackContext obj)
        {
            Debug.LogError("Performed");
            _event?.Raise();
        }

        protected virtual void OnStarted(InputAction.CallbackContext obj)
        {
            Debug.LogError("Started");
            _event?.Raise();
        }
    }

    [Serializable]
    public class InputEvent<T> : BaseInputEvent where T : struct
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
}
