using UnityEngine;
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

// Interface for objects that can save/load their state.
public interface ISaveable
{
    // Provide a unique identifier for this saveable object.
    string GetUniqueIdentifier();

    // Get a data container representing the current state of this object.
    IDataContainer GetSaveData();

    // Load the provided save data into this object.
    void LoadSaveData(IDataContainer data);

    // Called after the object has been saved.
    void OnSave();

    // Called after the object has been loaded.
    void OnLoad();
}

// Marker interface for save data containers.
// This allows you to use either classes or structs as needed.
public interface IDataContainer
{
}


// This class represents a single save slot.
// It stores a dictionary mapping unique identifiers to their corresponding save data.
[System.Serializable]
public class SaveDataSet
{
    [OdinSerialize]
    public Dictionary<string, IDataContainer> dataContainers = new Dictionary<string, IDataContainer>();
}

// The SaveDataManager is a ScriptableObject that can manage multiple save slots.
// It uses Odin's serialization to handle complex data types.
[CreateAssetMenu(menuName = "Save/Save Data Manager")]
public class SaveDataManager : SerializedScriptableObject, ISceneLoadListener
{
    // Static events that notify when data is saved or loaded.
    public static event Action OnDataSaved;
    public static event Action OnDataLoaded;
    
    VariableReference<CommandManager> commandManager;
    
    [SerializeField] ISaveable[] availableSaveables;
    [SerializeField] private int slotKey = 0;
    // Dictionary to hold multiple save slots.
    // Each slot is identified by a unique key (for example, "slot1", "slot2", etc.).
    [OdinSerialize]
    public Dictionary<int, SaveDataSet> saveSlots = new();

    // Async method to save the state of all ISaveable objects in the scene for the specified slot.
    public async Task SaveAsync()
    {
        SaveDataSet currentSave;
        if (!saveSlots.TryGetValue(slotKey, out currentSave))
        {
            currentSave = new SaveDataSet();
            saveSlots[slotKey] = currentSave;
        }

        // Collect save data from each ISaveable.
        foreach (var saveable in availableSaveables)
        {
            string id = saveable.GetUniqueIdentifier();
            IDataContainer container = saveable.GetSaveData();
            currentSave.dataContainers[id] = container;
        }

        // Offload the serialization to a background thread.
        byte[] jsonBytes = SerializationUtility.SerializeValue(currentSave, DataFormat.JSON);
        // Convert the byte array to a UTF-8 string.
        string json = Encoding.UTF8.GetString(jsonBytes);

        string path = Path.Combine(Application.persistentDataPath, "save_" + slotKey + ".json");
        await File.WriteAllTextAsync(path, json);
        Debug.Log("Game saved to: " + path);

        // Notify each saveable that it has been saved.
        foreach (var saveable in availableSaveables)
        {
            saveable.OnSave();
        }

        // Trigger the static event.
        OnDataSaved?.Invoke();
    }
    
    [Button]
    public void TriggerSave()
    {
        AsyncCommand saveCommand = new AsyncCommand("SaveData", SaveAsync);
        CommandManager.ExecuteCommand(saveCommand);
    }
    [Button]
    public void TriggerLoad()
    {
        AsyncCommand loadCommand = new AsyncCommand("LoadData", LoadAsync);
        CommandManager.ExecuteCommand(loadCommand);
    }
    
    
    // Async method to load the state for a given slot.
    public async Task LoadAsync()
    {
        string path = Path.Combine(Application.persistentDataPath, "save_" + slotKey + ".json");
        if (File.Exists(path))
        {
            // Read the file asynchronously.
            string json = await File.ReadAllTextAsync(path);
            // Convert the string back to a byte array.
            byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
            // Offload the deserialization to a background thread.
            SaveDataSet loadedDataSet = null;
            try
            {
                loadedDataSet = SerializationUtility.DeserializeValue<SaveDataSet>(jsonBytes, DataFormat.JSON);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error deserializing data: {e.Message} + {e.StackTrace}");
                return;
            }
             

            // Apply the loaded data to each ISaveable in the scene.
            foreach (var saveable in availableSaveables)
            {
                string id = saveable.GetUniqueIdentifier();
                if (loadedDataSet.dataContainers.TryGetValue(id, out IDataContainer container))
                {
                    try
                    { 
                        saveable.LoadSaveData(container);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Error loading data for {saveable}: {e.Message} + {e.StackTrace}");
                    }
                    
                }
            }
            Debug.Log("Game loaded from: " + path);

            // Notify each saveable that it has been loaded.
            foreach (var saveable in availableSaveables)
            {
                saveable.OnLoad();
            }

            // Trigger the static event.
            OnDataLoaded?.Invoke();
        }
        else
        {
            Debug.LogWarning("No save file found at: " + path);
        }
    }


    private void LocateSaveables()
    {
        List<ISaveable> saveables = new List<ISaveable>();
        saveables.AddRange(ScriptableObjectManager.GetSaveableScriptableObjects());
        availableSaveables = saveables.ToArray();
    }

    public void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        
    }

    public void OnSceneUnload(Scene scene)
    {
        
    }

    public void OnStopwatchStart(Scene scene)
    {
        LocateSaveables();
    }

    public void OnStopwatchStop(Scene scene)
    {
        availableSaveables = null;
    }
}
