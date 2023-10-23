using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Items/Item", order = 3)]
public class Item: UniqueScriptableObject
{
    [SerializeField]
    [InspectorName("Name")]
    private string _name;
    
    [SerializeField]
    [InspectorName("Rarity")]
    private ItemRarity _rarity;
    
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
    public int MaxQuantity => _maxQuantity;

    public override string ToString()
    {
        string itemString = "";
        itemString += "Name: " + _name + "\n";
        itemString += "Rarity: " + _rarity + "\n";
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

//Serializable class to save the data of the item
[System.Serializable]
public class ItemData
{
    public string id;
    public string name;
    public ItemRarity rarity;
    public int quantity;
    
    public ItemData(string name, ItemRarity rarity, int quantity, string id)
    {
        this.name = name;
        this.rarity = rarity;
        this.quantity = quantity;
        this.id = id;
    }
    
    public ItemData(string name, ItemRarity rarity, int quantity)
    {
        this.name = name;
        this.rarity = rarity;
        this.quantity = quantity;
    }
    
    public override string ToString()
    {
        string itemString = "";
        itemString += "Name: " + name + "\n";
        itemString += "Rarity: " + rarity + "\n";
        itemString += "Quantity: " + quantity + "\n";
        return itemString;
    }
}