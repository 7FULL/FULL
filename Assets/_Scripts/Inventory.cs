using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MenuUtils
{
    [InspectorName("Animator")]
    [SerializeField]
    private Animator animator;
    
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
    [InspectorName("Principal inventory slots")]
    private Image[] principalInventorySlots;
    
    private Item currentItem;
        
    private void Awake()
    {
        HasAnimation = true;
    }
    
    //Close the inventory
    public void Close()
    {
        if (isOpen)
        {
            CloseAnimation();
        }
    }
    
    public void CloseOnCLick()
    {
        MenuManager.Instance.CloseMenu();
    }
    
    public override void OpenAnimation()
    {
        animator.SetBool("IsOpen", true);
        loadContacts();
        isOpen = true;
    }
    
    public override void CloseAnimation()
    {
        animator.SetBool("IsOpen", false);
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
}
