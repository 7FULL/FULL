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
    [InspectorName("Principal inventory slots")]
    private ItemDisplay[] principalInventorySlots;
    
    [SerializeField]
    [InspectorName("Ammo Text")]
    private TMP_Text ammoText;
    
    [SerializeField]
    [InspectorName("Items container")]
    private GameObject itemsContainer;
    
    [SerializeField]
    [InspectorName("Item prefab")]
    private GameObject itemPrefab;
    
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
        
        LoadContacts();
        LoadItem();
        
        isOpen = true;
    }
    
    public override void CloseAnimation()
    {
        base.CloseAnimation();
        
        isOpen = false;
    }

    private void LoadContacts()
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

    private void LoadItem()
    {
        foreach (Transform child in itemsContainer.transform)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] != null)
            {
                GameObject itemObject = Instantiate(itemPrefab, itemsContainer.transform);
                ItemInventoryDisplay itemObjectScript = itemObject.GetComponent<ItemInventoryDisplay>();
                itemObjectScript.Configure(items[i], this, i); 
            }
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
        
        bool aux = false;
        
        //If there is a null item in the inventory, we replace it with the new item
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] == null && !aux)
            {
                items[i] = itemToAddToInventory;
                aux = true;
            }
        }
        
        if (!aux)
        {
            items.Add(itemToAddToInventory);
        }
        
        Initialize(false);
    }

    public void AddItem(Item item)
    {
        Item itemObject = Instantiate(item.ItemData.prefab, inventoryContainer.transform).GetComponent<Item>();
        itemObject.Initialize(item.ItemData, item.Quantity);
        
        bool aux = false;
        
        //If there is a null item in the inventory, we replace it with the new item
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] == null && !aux)
            {
                items[i] = itemObject;
                aux = true;
            }
        }
        
        if (!aux)
        {
            items.Add(itemObject);
        }
        
        Initialize(false);
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
        
        bool aux = false;
        
        //If there is a null item in the inventory, we replace it with the new item
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] == null && !aux)
            {
                items[i] = itemToAdd;
                aux = true;
            }
        }
        
        if (!aux)
        {
            items.Add(itemToAdd);
        }
        
        Initialize(false);
    }

    public void Initialize(bool deselect = true)
    {
        //Set all images of the slots to the items image
        for (int i = 0; i < principalInventorySlots.Length; i++)
        {
            if (i < items.Count)
            {
                principalInventorySlots[i].Configure(items[i]);
            }
            else
            {
                principalInventorySlots[i].Configure(null);
            }
        }
        
        ConfigureItems();

        if (deselect)
        {
            if (items.Count > 0)
            {
                currentItem = items[0];
            
                principalInventorySlots[0].Select();
            }
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
        
        //If we press a number, select the item in that slot
        switch (Input.inputString)
        {
            case "1":
                currentSlot = 0;
                ConfigureItems();
                break;
            case "2":
                currentSlot = 1;
                ConfigureItems();
                break;
            case "3":
                currentSlot = 2;
                ConfigureItems();
                break;
            case "4":
                currentSlot = 3;
                ConfigureItems();
                break;
            case "5":
                currentSlot = 4;
                ConfigureItems();
                break;
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
    
    //This function is called when the player changes the position of an object in the inventory to a position in the principal inventory
    public void ChangePosition(Item item, int to)
    {
        int from = items.IndexOf(item);
        if (to < 5)
        {
            Item itemToChange = null;
        
            if (to < items.Count)
            {
                itemToChange = items[to];
            
                items.RemoveAt(from);
                items.Insert(from, itemToChange);
                items.RemoveAt(to);
                items.Insert(to, item);
            }
            else
            {
                while (items.Count <= to)
                {
                    items.Add(null);
                }
            
                itemToChange = items[to];
            
                items.RemoveAt(from);
                items.Insert(from, itemToChange);
                items.RemoveAt(to);
                items.Insert(to, item);
            }
        }
        else
        {
            while (items.Count < 5)
            {
                items.Add(null);
            }
            
            items.RemoveAt(from);
            items.Insert(from, null);
            items.Add(item);
        }
        
        //Set all images of the slots to the items image
        for (int i = 0; i < principalInventorySlots.Length; i++)
        {
            if (i < items.Count)
            {
                principalInventorySlots[i].Configure(items[i]);
            }
        }
        
        Initialize(false);
    }
}
