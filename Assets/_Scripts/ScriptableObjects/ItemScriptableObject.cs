using UnityEngine;
using UnityEngine.Serialization;

public abstract class ItemScriptableObject: ScriptableObject
{
    [InspectorName("Name")]
    public Items name;

    [InspectorName("Quality")]
    public ItemQuality quality;

    [InspectorName("Max Quantity")]
    public int maxQuantity;

    [InspectorName("Prefab")]
    public GameObject prefab;

    [InspectorName("Sprite")]
    public Sprite sprite;
    
    [InspectorName("Category")]
    public ItemCategory category;

    public abstract Item ToModel();
}