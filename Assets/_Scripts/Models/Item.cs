using System;
using UnityEngine;
using UnityEngine.Serialization;

//TODO: Make this abstract in case it is needed as well as some other classes that inherit from this one
public abstract class Item: MonoBehaviour
{
    [SerializeField]
    [InspectorName("Item Data")]
    private ItemData _itemData;
    
    [SerializeField]
    [InspectorName("Quantity")]
    private int _quantity = 1;
    
    [SerializeField]
    [InspectorName("Floor collider")]
    private Collider _floorCollider;
    
    public ItemData ItemData => _itemData;
    
    public int Quantity => _quantity;
    
    public void Initialize(ItemData itemData, int quantity = 1)
    {
        _itemData = itemData;
        _quantity = quantity;
    }
    
    public SerializableItemData ToItemData()
    {
        return new SerializableItemData(_itemData.name, _itemData.quality, _itemData.category, _quantity);
    }
    
    public void Add(int quantity)
    {
        _quantity += quantity;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponentInParent<Player>().AddItem(this);
            Destroy(gameObject);
        }
    }

    //Use
    public abstract void Use();
}




// <-------------------This class is used to save the item int the DB as well as load it from there------------------->
[System.Serializable]
public class SerializableItemData
{
    public Items name;
    public ItemQuality quality;
    public int quantity;
    public ItemCategory category;
    
    public SerializableItemData(Items name, ItemQuality quality, ItemCategory category,int quantity)
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