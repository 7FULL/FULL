using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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
    
    [SerializeField]
    [InspectorName("Ammo Text")]
    private TMP_Text ammoText;
    
    private Item currentItem;
    
    public Item CurrentItem => currentItem;
    
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
        
        ConfigureItems();

        if (items.Count > 0)
        {
            currentItem = items[0];
            
            principalInventorySlots[0].Select();
        }
    }

    private void Update()
    {
        //If  mouse scroll up, select the next item and if mouse scroll down, select the previous item from the principal inventory
        if (Input.GetAxis("Mouse ScrollWheel") < 0f)
        {
            if (currentSlot < principalInventorySlots.Length - 1)
            {
                currentSlot++;
            }
            else
            {
                currentSlot = 0;
            }
            
            ConfigureItems();
        }
        else if (Input.GetAxis("Mouse ScrollWheel") > 0f)
        {
            if (currentSlot > 0)
            {
                currentSlot--;
            }
            else
            {
                currentSlot = principalInventorySlots.Length - 1;
            }
            
            ConfigureItems();
        }
    }

    private void ConfigureItems()
    {
        if (currentSlot < items.Count && items[currentSlot] != null)
        {
            currentItem = items[currentSlot];
        }
        else
        {
            currentItem = null;
        }
            
        foreach (ItemDisplay itemDisplay in principalInventorySlots)
        {
            itemDisplay.UnSelect();
        }
            
        principalInventorySlots[currentSlot].Select();
        
        //We disable every item in the inventory
        for (int i = 0; i < inventoryContainer.transform.childCount; i++)
        {
            inventoryContainer.transform.GetChild(i).gameObject.SetActive(false);
        }
        
        //We enable the item we want to display
        if (inventoryContainer.transform.childCount > currentSlot)
        {
            inventoryContainer.transform.GetChild(currentSlot).gameObject.SetActive(true);
        }
        
        UpdateItemText();
    }
    
    public void UpdateItemText()
    {
        //We update the ammo text
        if (currentItem != null)
        {
            if (currentItem is Gun gun)
            {
                string left = "";
                
                if (gun.LeftAmmo > 99)
                {
                    left = "99+";
                }
                else
                {
                    left = gun.LeftAmmo.ToString();
                }
                
                ammoText.text = gun.CurrentAmmo + " / " + left;
            }
            else
            {
                string left = "";
                
                if (currentItem.Quantity > 99)
                {
                    left = "99+";
                }
                else
                {
                    left = currentItem.Quantity.ToString();
                }
                
                ammoText.text = "1 / " + left;
            }
        }
        else
        {
            ammoText.text = "1 / 1";
        }
    }
}
