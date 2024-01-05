using System;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

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
    
    [SerializeField]
    [InspectorName("Is floor item")]
    private bool _isFloorColliderActive = false;
    
    public ItemData ItemData => _itemData;
    
    public int Quantity => _quantity;

    private void Awake()
    {
        if (_floorCollider != null)
        {
            _floorCollider.enabled = _isFloorColliderActive;
        }
    }
    
    public void ConfigureFloorCollider(bool active)
    {
        _isFloorColliderActive = active;
        _floorCollider.enabled = active;
        
        //TODO: Rotation animation
    }

    public void Drop()
    {
        ConfigureFloorCollider(true);
        
        //We add a rigidbody to the item
        Rigidbody rigidbody = gameObject.AddComponent<Rigidbody>();
        
        //We block the rotation of the item
        rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        
        float fuerzaX = Random.Range(-5f, 5f);
        float fuerzaZ = Random.Range(-5f, 5f);
        
        float fuerzaY = Mathf.Abs(Random.Range(1f, 5f));
        
        Vector3 fuerza = new Vector3(fuerzaX, fuerzaY, fuerzaZ);
        
        rigidbody.AddForce(fuerza, ForceMode.Impulse);
         
    }

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
        if (_isFloorColliderActive && other.CompareTag("Player"))
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