using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "ScriptableObjects/Item", order = 1)]
public abstract class Item: ScriptableObject
{
    
    [SerializeField]
    [InspectorName("Name")]
    private string _name;
    
    [SerializeField]
    [InspectorName("Rarity")]
    private ItemRarity _rarity;
    
    [SerializeField]
    [InspectorName("Quantity")]
    private int _quantity;
    
    [SerializeField]
    [InspectorName("Max Quantity")]
    private int _maxQuantity;

    [SerializeField]
    [InspectorName("Prefab")]
    private GameObject _prefab;
    
    [SerializeField]
    [InspectorName("Sprite")]
    private Sprite _sprite;
    
    // Getters, there are not setters because we don't want to change the values of the item to avoid cheating
    public string Name => _name;
    public GameObject Prefab => _prefab;
    public Sprite Sprite => _sprite;
    public ItemRarity Rarity => _rarity;
    public int Quantity => _quantity;
    public int MaxQuantity => _maxQuantity;

    public override string ToString()
    {
        string itemString = "";
        itemString += "Name: " + _name + "\n";
        itemString += "Rarity: " + _rarity + "\n";
        itemString += "Quantity: " + _quantity + "\n";
        itemString += "Max Quantity: " + _maxQuantity + "\n";
        return itemString;
    }
    
    // Upgrade the item based on another item
    public bool Change(Item item)
    {
        if (item == this)
        {
            Debug.Log("The object trying to change is the same as the object to change");
            
            return false;
        }
        
        // We change the object
        _name = item.Name;
        _rarity = item.Rarity;
        _quantity = item.Quantity;
        _maxQuantity = item.MaxQuantity;
        _prefab = item.Prefab;
        _sprite = item.Sprite;
        
        return true;
    }
    
    // Drop the item
    public void Drop(Vector3 position)
    {
        ItemManager.Instance.DropItem(this, position);
    }
    
    // Destroy the item
    public void Destroy()
    {
        ItemManager.Instance.DestroyItem(this);
    }
}

[System.Serializable]
public struct Display
{
    [SerializeField]
    [InspectorName("Prefab")]
    private GameObject _prefab;
    
    [SerializeField]
    [InspectorName("Sprite")]
    private Sprite _sprite;
    
    public GameObject Prefab => _prefab;
    public Sprite Sprite => _sprite;
}