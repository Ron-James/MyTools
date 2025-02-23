using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using CustomSceneReference;
using JetBrains.Annotations;
using Sirenix.Serialization;


public interface ISceneLoadListener
{
    void OnSceneLoad(Scene scene, LoadSceneMode mode);
    void OnSceneUnload(Scene scene);

    void OnStopwatchStart(Scene scene);
    void OnStopwatchStop(Scene scene);
}


public interface IRequireInitialization<T> where T : MonoBehaviour
{
    void Initialize(T initializer);
    void Dispose();
}

[CreateAssetMenu(fileName = "SceneLoadManager", menuName = "SceneLoadManager")]
public class SceneLoadManager : SerializedScriptableObject
{
    [ShowInInspector, ReadOnly] private static List<ISceneLoadListener> _sceneLoadListeners = new();
    [SerializeReference, TableList] private List<SceneReference> _scenes = new();


    [SerializeField, ReadOnly] private SceneStopwatch _sceneStopwatch;


    private void OnEnable()
    {
#if UNITY_EDITOR
        _scenes.Clear();
        foreach (var scene in EditorBuildSettings.scenes)
        {
            var sceneReference = new SceneReference(scene.path);
            _scenes.Add(sceneReference);
        }
#endif
    }
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void InitSceneLoadListeners()
    {
        Debug.Log(ScriptableObjectManager.All.Length + " scriptable objects found");
        foreach (var so in ScriptableObjectManager.All)
        {
            if(so is ISceneLoadListener listener)
            {
                _sceneLoadListeners.Add(listener);
            }
        }
        SceneStopwatch.OnStart += OnSceneStopWatchStart;
        SceneStopwatch.OnStop += OnSceneStopWatchStop;
        SceneManager.sceneLoaded += OnSceneLoad;
        SceneManager.sceneUnloaded += OnSceneUnload;
    }
    
    
    private void OnDisable()
    {
        
        SceneManager.sceneLoaded -= OnSceneLoad;
        SceneManager.sceneUnloaded -= OnSceneUnload;
        SceneStopwatch.OnStart -= OnSceneStopWatchStart;
        SceneStopwatch.OnStop -= OnSceneStopWatchStop;


        _sceneLoadListeners.Clear();
        ScriptableObjectManager.Clear();

#if UNITY_EDITOR
        _scenes.Clear();
#endif
    }


    static void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("Scene loaded from scene manager: " + scene.name);
        foreach (var listener in _sceneLoadListeners)
        {
            listener.OnSceneLoad(scene, mode);
        }
    }

    static void OnSceneUnload(Scene scene)
    {
        Debug.Log("Scene unloaded from scene manager: " + scene.name);
        foreach (var listener in _sceneLoadListeners)
        {
            listener.OnSceneUnload(scene);
        }
    }


    static void OnSceneStopWatchStart()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        Debug.Log("Stopwatch started from scene manager, calling on " + _sceneLoadListeners.Count + " listeners");
        foreach (var listener in _sceneLoadListeners)
        {
            listener.OnStopwatchStart(activeScene);
        }
        
    }


    static void OnSceneStopWatchStop()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        Debug.Log("Stopwatch stopped from scene manager");
        foreach (var listener in _sceneLoadListeners)
        {
            listener.OnStopwatchStop(activeScene);
        }
        _sceneLoadListeners.Clear();
    }

    


    

    
}