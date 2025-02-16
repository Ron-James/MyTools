using UnityEngine;


    [CreateAssetMenu(fileName = "Item", menuName = "Scriptable Objects/Item")]
    public class Item : ScriptableObject
    {
        public int value;
        public Sprite sprite;
        public string description;
    }