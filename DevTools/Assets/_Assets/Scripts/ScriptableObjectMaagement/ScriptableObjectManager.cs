using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ScriptableObjectManager
{
    private static Dictionary<string, ScriptableObject> _scriptableObjects =
        new Dictionary<string, ScriptableObject>();


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
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void LoadAllScriptableObjects()
    {
        var scriptableObjects = Resources.LoadAll<ScriptableObject>("");
        foreach (var scriptableObject in scriptableObjects)
        {
            AddScriptableObject(scriptableObject.name, scriptableObject);
        }
    }


    
}