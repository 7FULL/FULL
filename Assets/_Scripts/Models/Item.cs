using UnityEngine;
using UnityEngine.Serialization;

//TODO: Make this abstract in case it is needed as well as some other classes that inherit from this one
public class Item
{
    [SerializeField]
    [InspectorName("Name")]
    private Items _name;
    
    [SerializeField]
    [InspectorName("Quality")]
    private ItemQuality _quality;
    
    [SerializeField]
    [InspectorName("Max Quantity")]
    private int _maxQuantity;
    
    [SerializeField]
    [InspectorName("Quantity")]
    private int _quantity;

    [SerializeField]
    [InspectorName("Prefab")]
    private GameObject _prefab;
    
    [SerializeField]
    [InspectorName("Sprite")]
    private Sprite _sprite;
    
    [SerializeField]
    [InspectorName("Category")]
    private ItemCategory _category;
    
    // Getters, there are not setters because we don't want to change the values of the item to avoid cheating
    public string Name => _name.ToString();
    public GameObject Prefab => _prefab;
    public Sprite Sprite => _sprite;
    public ItemQuality Quality => _quality;
    public int MaxQuantity => _maxQuantity;
    
    public ItemCategory Category => _category;
    
    public int Quantity => _quantity;

    public override string ToString()
    {
        string itemString = "";
        itemString += "Name: " + _name + "\n";
        itemString += "Quality: " + _quality + "\n";
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
        _name = item._name;
        _quality = item.Quality;
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
    
    public ItemData ToItemData()
    {
        return new ItemData(_name, _quality, _category, _maxQuantity);
    }
    
    // Destroy the item
    public void Destroy()
    {
        ItemManager.Instance.DestroyItem(this);
    }
    
    //Contructor based on scriptable object
    public Item(ItemScriptableObject itemScriptableObject)
    {
        _name = itemScriptableObject.name;
        _quality = itemScriptableObject.quality;
        _maxQuantity = itemScriptableObject.maxQuantity;
        _prefab = itemScriptableObject.prefab;
        _sprite = itemScriptableObject.sprite;
        _category = itemScriptableObject.category;
        _quantity = 1;
    }
    
    //Constructor based on item data
    public Item(){}
}




// <-------------------This class is used to save the item int the DB as well as load it from there------------------->
[System.Serializable]
public class ItemData
{
    public Items name;
    public ItemQuality quality;
    public int quantity;
    public ItemCategory category;
    
    public ItemData(Items name, ItemQuality quality, ItemCategory category,int quantity)
    {
        this.name = name;
        this.quality = quality;
        this.quantity = quantity;
        this.category = category;
    }
    
    public override string ToString()
    {
        string itemString = "";
        itemString += "Name: " + name + "\n";
        itemString += "Quality: " + quality + "\n";
        itemString += "Quantity: " + quantity + "\n";
        itemString += "Category: " + category + "\n";
        return itemString;
    }
}