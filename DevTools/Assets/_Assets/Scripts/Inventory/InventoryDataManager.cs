using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "InventoryDataManager", menuName = "InventoryDataManager")]
public class InventoryDataManager : SerializedScriptableObject, ISaveable, ISceneLoadListener, IDataContainer
{
    [SerializeField] private Dictionary<ScriptableObjectReference<Item>, int> items = new();
    [SerializeField] Item[] allItems;

    public Dictionary<ScriptableObjectReference<Item>, int> Items => items;

    [Button]
    public void AddItem(Item item, int amount)
    {
        ScriptableObjectReference<Item> itemReference = new(item);
        if (!Items.TryAdd(itemReference, amount))
        {
            Items[itemReference] += amount;
        }
    }
    
    
    public int GetQuantity(Item item)
    {
        ScriptableObjectReference<Item> itemReference = new(item);
        if (Items.ContainsKey(itemReference))
        {
            return Items[itemReference];
        }
        else
        {
            return 0;
        }
    }
    
    public bool ValidateQuantity(Item item, int amount)
    {
        ScriptableObjectReference<Item> itemReference = new(item);
        if (Items.ContainsKey(itemReference))
        {
            return Items[itemReference] >= amount;
        }

        return false;
    }

    public string GetUniqueIdentifier()
    {
        return GetInstanceID().ToString();
    }

    public IDataContainer GetSaveData()
    {
        return this;
    }

    public void LoadSaveData(IDataContainer data)
    {
        if(data is InventoryDataManager inventoryData)
        {
            foreach (var kvp in inventoryData.Items)
            {
                Item item = kvp.Key.Value;
                int quantity = kvp.Value;
                AddItem(item, quantity);
            }
        }
        else
        {
            throw new System.InvalidCastException("Data is not of type InventorySaveData");
        }
    }

    public void OnSave()
    {
        
    }

    public void OnLoad()
    {
        
    }
    
    private void Initialize()
    {
        allItems = ScriptableObjectManager.LoadScriptableObject<Item>();
        items = new();
        foreach (var item in allItems)
        {
            AddItem(item, 0);
        }
    }


    public void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        
    }

    public void OnSceneUnload(Scene scene)
    {
        
    }

    public void OnStopwatchStart(Scene scene)
    {
        Initialize();
    }

    public void OnStopwatchStop(Scene scene)
    {
        items = null;
    }
}


public struct InventorySaveData : IDataContainer, IEnumerable<InventorySaveData.ItemData>
{
    public ItemData[] items;

    public InventorySaveData(InventoryDataManager dataManager)
    {
        List<ItemData> itemDataList = new List<ItemData>();
        foreach (var kvp in dataManager.Items)
        {
            Item key = kvp.Key.Value;
            int value = kvp.Value;
            ItemData itemData = new ItemData(key, value);
            itemDataList.Add(itemData);
        }
        items = itemDataList.ToArray();
    }
    public struct ItemData
    {
        public ScriptableObjectReference<Item> itemID;
        public int quantity;
        
        public ItemData(Item item, int quantity)
        {
            this.itemID = new(item);
            this.quantity = quantity;
        }
    }

    public IEnumerator<ItemData> GetEnumerator()
    {
        return ((IEnumerable<ItemData>) items).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}


[Serializable]
public struct ScriptableObjectReference<T> : IEquatable<ScriptableObjectReference<T>>, IEquatable<T> where T : ScriptableObject
{
    private string name;
    
    [ShowInInspector]
    public T Value => ScriptableObjectManager.GetScriptableObjectByName<T>(name);

    public ScriptableObjectReference(T value)
    {
        name = value.name;
    }


    public bool Equals(ScriptableObjectReference<T> other)
    {
        return name == other.name;
    }

    public override bool Equals(object obj)
    {
        return obj is ScriptableObjectReference<T> other && Equals(other);
    }
    
    public bool Equals(T other)
    {
        return Value == other;
    }

    public override int GetHashCode()
    {
        return (name != null ? name.GetHashCode() : 0);
    }
}