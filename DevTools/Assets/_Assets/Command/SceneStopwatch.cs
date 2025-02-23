#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

[ExecuteAlways]
public class SceneStopwatch : SerializedMonoBehaviour
{
    // Static events for scene callbacks
    public static event Action OnStart;
    public static event Action OnStop;

    [ShowInInspector]
    private static bool _initialized;

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

    
    // This method is called after every scene is loaded.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Init()
    {
        if (!_initialized)
        {
            SceneManager.sceneLoaded += (scene, mode) => EnsureSceneStopwatchExists();
            _initialized = true;
        }
        // In case the scene was already loaded (as may happen in editor), do an immediate check.
        Scene activeScene = SceneManager.GetActiveScene();
        EnsureSceneStopwatchExists();
        
    }

    private static void EnsureSceneStopwatchExists()
    {
        // Try to find an existing SceneStopwatch in the scene.
        SceneStopwatch stopwatch = FindObjectOfType<SceneStopwatch>();
        if (stopwatch == null)
        {
            // If not found, create a new GameObject and add the component.
            GameObject go = new GameObject("SceneStopwatch");
            go.AddComponent<SceneStopwatch>();
        }
        else
        {
            Debug.Log("SceneStopwatch already exists in scene");
        }
    }
}


