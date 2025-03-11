using System.Collections.Generic;
using System.Linq;
using Unity.Collections.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine;

public static class ScriptableObjectManager
{
    private static Dictionary<string, ScriptableObject> _scriptableObjects =
        new Dictionary<string, ScriptableObject>();

    public static ScriptableObject[] SaveableScriptableObjects;


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
    
    
    public static ScriptableObject GetScriptableObjectByName(string name)
    {
        if (_scriptableObjects.ContainsKey(name))
        {
            return _scriptableObjects[name];
        }
        
        throw new KeyNotFoundException("ScriptableObject not found: " + name);
    }
    
    public static T GetScriptableObjectByName<T>(string name) where T : ScriptableObject
    {
        if (_scriptableObjects.ContainsKey(name) && _scriptableObjects[name] is T)
        {
            return _scriptableObjects[name] as T;
        }
        
        throw new KeyNotFoundException("ScriptableObject not found: " + name);
    }
    
    
    public static T GetScriptableObjectByID<T>(int id) where T : ScriptableObject
    {
        return _scriptableObjects.Values.FirstOrDefault(scriptableObject => scriptableObject.GetInstanceID() == id) as T;

    }

    public static ScriptableObject GetScriptableObjectByID(int id)
    {
        return _scriptableObjects.Values.FirstOrDefault(scriptableObject => scriptableObject.GetInstanceID() == id);
    }

    public static void AddScriptableObject(string key, ScriptableObject scriptableObject)
    {
        if (_scriptableObjects.ContainsKey(key))
        {
            Debug.LogError("ScriptableObject already exists: " + key);
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
    
    public static ISaveable[] GetSaveableScriptableObjects()
    {
        List<ISaveable> saveableScriptableObjects = new List<ISaveable>();
        
        foreach (var scriptableObject in SaveableScriptableObjects)
        {
            if (scriptableObject is ISaveable saveable)
            {
                saveableScriptableObjects.Add(saveable);
            }
            else
            {
                Debug.LogError("ScriptableObject is not ISaveable: " + scriptableObject.name);
            }
        }
        return saveableScriptableObjects.ToArray();
    }
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void LoadAllScriptableObjects()
    {
        List<ScriptableObject> saveableScriptableObjects = new List<ScriptableObject>();
        var scriptableObjects = Resources.LoadAll<ScriptableObject>("");
        foreach (var scriptableObject in scriptableObjects)
        {
            AddScriptableObject(scriptableObject.name, scriptableObject);
            if (scriptableObject is ISaveable)
            {
                saveableScriptableObjects.Add(scriptableObject);
            }
        }
        
        SaveableScriptableObjects = saveableScriptableObjects.ToArray();
    }


    
}