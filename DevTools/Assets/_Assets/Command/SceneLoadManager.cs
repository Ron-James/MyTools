using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using CustomSceneReference;


public interface ISceneLoadListener
{
    void OnSceneLoad(Scene scene, LoadSceneMode mode);
    void OnSceneUnload(Scene scene);
    
}
[CreateAssetMenu(fileName = "SceneLoadManager", menuName = "SceneLoadManager")]
public class SceneLoadManager : SerializedScriptableObject
{
    [SerializeField] private List<ISceneLoadListener> _sceneLoadListeners = new();
    [SerializeReference, TableList] private List<SceneReference> _scenes = new();
    
    
    
    
    
    private void OnEnable()
    {
        ScriptableObjectManager.LoadAllScriptableObjects();
        foreach (var so in ScriptableObjectManager.All)
        {
            if(so is ISceneLoadListener sceneLoadListener)
            {
                if(!_sceneLoadListeners.Contains(sceneLoadListener)) _sceneLoadListeners.Add(sceneLoadListener);
                SceneManager.sceneLoaded += sceneLoadListener.OnSceneLoad;
                SceneManager.sceneUnloaded += sceneLoadListener.OnSceneUnload;
                #if UNITY_EDITOR
                //subscirbe to playmode state change
                EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
                #endif
                
            }
        }
        
        
        
        #if UNITY_EDITOR
        _scenes.Clear();
        foreach (var scene in EditorBuildSettings.scenes)
        {
            var sceneReference = new SceneReference(scene.path);
            _scenes.Add(sceneReference);
        }
        #endif
    }
    #if UNITY_EDITOR
    private void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredEditMode)
        {
            foreach (var listener in _sceneLoadListeners)
            {
                Scene currentScene = SceneManager.GetActiveScene();
                listener.OnSceneUnload(currentScene);
            }
        }
    }
    
    #endif
    private void OnDisable()
    {
        foreach (var sceneLoadListener in _sceneLoadListeners)
        {
            SceneManager.sceneLoaded -= sceneLoadListener.OnSceneLoad;
            SceneManager.sceneUnloaded -= sceneLoadListener.OnSceneUnload;
            
            #if UNITY_EDITOR
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            #endif
        }
        
        
        _sceneLoadListeners.Clear();
        ScriptableObjectManager.Clear();
        
        #if UNITY_EDITOR
        _scenes.Clear();
        #endif
    }
}


public static class ScriptableObjectManager
{
    private static Dictionary<string, ScriptableObject> _scriptableObjects = new Dictionary<string, ScriptableObject>();
    
    
    
    public static T[] LoadScriptableObject<T>(string path = "") where T : ScriptableObject
    {
        var scriptableObjects = Resources.LoadAll<T>(path);
        return scriptableObjects;
    }
    
    
    public static ScriptableObject[] All => _scriptableObjects.Values.ToArray();
    public static void Clear()
    {
        _scriptableObjects.Clear();
    }
    public static void AddScriptableObject(string key, ScriptableObject scriptableObject)
    {
        if (_scriptableObjects.ContainsKey(key))
        {
            _scriptableObjects[key] = scriptableObject;
        }
        else
        {
            _scriptableObjects.Add(key, scriptableObject);
        }
    }
    
    public static T GetScriptableObject<T>(string key) where T : ScriptableObject
    {
        if (_scriptableObjects.ContainsKey(key))
        {
            return _scriptableObjects[key] as T;
        }
        else
        {
            return null;
        }
    }
    
    public static void LoadAllScriptableObjects()
    {
        var scriptableObjects = Resources.LoadAll<ScriptableObject>("");
        foreach (var scriptableObject in scriptableObjects)
        {
            AddScriptableObject(scriptableObject.name, scriptableObject);
        }
    }
    
}
