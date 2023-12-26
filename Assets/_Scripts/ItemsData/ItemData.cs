using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "Item", menuName = "Items/Item", order = 0)]
public abstract class ItemData: ScriptableObject
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
    
    //To string
    public override string ToString()
    {
        return "Name: " + name + "\nQuality: " + quality + "\nMax Quantity: " + maxQuantity + "\nPrefab: " + prefab + "\nSprite: " + sprite + "\nCategory: " + category;
    }
}