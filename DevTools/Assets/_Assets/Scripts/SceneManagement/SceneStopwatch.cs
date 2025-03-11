#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using Sirenix.OdinInspector;
using Sirenix.Serialization;


public abstract class PersistentMonoBehaviour<T> : SerializedMonoBehaviour where T : MonoBehaviour
{
    
    protected static void Initialize()
    {
        SceneManager.sceneLoaded += (scene, mode) => EnsureExists();
        EnsureExists();
    }

    private static void EnsureExists()
    {
        if (FindFirstObjectByType<T>() == null)
        {
            Type type = typeof(T);
            string typeName = type.Name;
            GameObject go = new GameObject(typeName);
            go.AddComponent<T>();
        }
    }
}

[ExecuteAlways]
public class SceneStopwatch : PersistentMonoBehaviour<SceneStopwatch>
{
    // Static events for scene callbacks
    public static event Action OnStart;
    public static event Action OnStop;
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Init()
    {
        Initialize();
    }

    void Awake()
    {
        // (Optional) Additional setup if needed.
    }

    void OnEnable()
    {
        OnStart?.Invoke();
        
    }

    void OnDisable()
    {
        
        OnStop?.Invoke();
    }

    
}


