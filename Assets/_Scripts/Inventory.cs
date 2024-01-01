using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MenuUtils
{
    [InspectorName("Conocidos container")]
    [SerializeField]
    private GameObject conocidosContainer;
    
    private bool isOpen = false;
    
    [InspectorName("Contact Prefab")]
    [SerializeField]
    private GameObject contactPrefab;
    
    public bool IsOpen => isOpen;
    
    private List<Item> items = new List<Item>();
    
    [SerializeField]
    [InspectorName("Inventory container")]
    private GameObject inventoryContainer;
    
    [SerializeField]
    [InspectorName("Inventory slots")]
    private ItemDisplay[] inventorySlots;
    
    [SerializeField]
    [InspectorName("Principal inventory slots")]
    private ItemDisplay[] principalInventorySlots;
    
    private Item currentItem;
    
    private int currentSlot = 0;
        
    private void Awake()
    {
        HasAnimation = true;
    }
    
    public void CloseOnCLick()
    {
        MenuManager.Instance.CloseMenu();
    }
    
    public override void OpenAnimation()
    {
        base.OpenAnimation();
        
        loadContacts();
        
        isOpen = true;
    }
    
    public override void CloseAnimation()
    {
        base.CloseAnimation();
        
        isOpen = false;
    }

    private void loadContacts()
    {
        GameManager.Instance.Player.RefreshContacts();
        
        foreach (Transform child in conocidosContainer.transform)
        {
            Destroy(child.gameObject);
        }
        
        foreach (Contact contact in GameManager.Instance.Player.Contacts)
        {
            GameObject contactObject = Instantiate(contactPrefab, conocidosContainer.transform);
            ContactDisplay contactObjectScript = contactObject.GetComponent<ContactDisplay>();
            contactObjectScript.Configure(contact);
        }
    }
    
    public SerializableItemData[] GetItems()
    {
        SerializableItemData[] itemsData = new SerializableItemData[items.Count];
        
        for (int i = 0; i < items.Count; i++)
        {
            itemsData[i] = items[i].ToItemData();
        }
        
        return itemsData;
    }
    
    public void AddItem(Items itemToAdd)
    {
        //If theres already an item with the same name, add it to the stack
        foreach (Item item in items)
        {
            if (item.ItemData.name == itemToAdd)
            {
                item.Add(1);
                return;
            }
        }
        
        Item itemToAddToInventory = Instantiate(ItemManager.Instance.GetItem(itemToAdd).prefab, inventoryContainer.transform).GetComponent<Item>();
        items.Add(itemToAddToInventory);
    }
    
    public void AddItem(SerializableItemData serializableItemData)
    {
        //If theres already an item with the same name, add it to the stack
        foreach (Item item in items)
        {
            if (item.ItemData.name == serializableItemData.name)
            {
                item.Add(serializableItemData.quantity);
                return;
            }
        }
        
        ItemData itemData = ItemManager.Instance.GetItem(serializableItemData.name);
        Item itemToAdd = Instantiate(itemData.prefab, inventoryContainer.transform).GetComponent<Item>();
        itemToAdd.Initialize(itemData, serializableItemData.quantity);
        items.Add(itemToAdd);
    }

    public void Initialize()
    {
        //Set all images of the slots to the items image
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (i < items.Count)
            {
                inventorySlots[i].Configure(items[i]);
            }
            else
            {
                inventorySlots[i].Configure(null);
            }
        }
        
        //Un select all slots and select the first one
        foreach (ItemDisplay itemDisplay in principalInventorySlots)
        {
            itemDisplay.UnSelect();
        }

        if (items.Count > 0)
        {
            currentItem = items[0];
            principalInventorySlots[0].Select();
        }
    }
}
