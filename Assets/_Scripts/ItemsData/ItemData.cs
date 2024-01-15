using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "Item", menuName = "Items/Item", order = 0)]
public abstract class ItemData: ScriptableObject
{
    [TitleGroup("Item data")]
    [PreviewField(80, ObjectFieldAlignment.Left), HideLabel]
    [HorizontalGroup("Item data/Item", width:85)]
    public Sprite sprite;
    
    [VerticalGroup("Item data/Item/Right")]
    public Items name;

    [InspectorName("Max Quantity")]
    [VerticalGroup("Item data/Item/Right")]
    public int maxQuantity;

    [InspectorName("Prefab")]
    [VerticalGroup("Item data/Item/Right")]
    public GameObject prefab;
    
    [InspectorName("Category")]
    [VerticalGroup("Item data/Item/Right")]
    public ItemCategory category;
    
    [EnumToggleButtons]
    [Title("Quality")]
    [HideLabel]
    public ItemQuality quality;
    
    //To string
    public override string ToString()
    {
        return "Name: " + name + "\nQuality: " + quality + "\nMax Quantity: " + maxQuantity + "\nPrefab: " + prefab + "\nSprite: " + sprite + "\nCategory: " + category;
    }
}