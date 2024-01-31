using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(PhotonView))]
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
    
    [SerializeField]
    [InspectorName("Empty item prefab")]
    private GameObject emptyItemPrefab;
    
    [SerializeField]
    [InspectorName("Crosshair")]
    private Image crosshair;
    
    public Image Crosshair => crosshair;
    
    [SerializeField]
    [InspectorName("Crosshair default")]
    private Sprite crosshairDefault;

    [SerializeField] 
    [InspectorName("Streaming")]
    private HISPlayerSample streaming;
    
    [SerializeField]
    [InspectorName("Streaming prefab")]
    private GameObject streamingPrefab;
    
    [SerializeField]
    [InspectorName("Streaming container")]
    private GameObject streamingContainer;
    
    [SerializeField]
    [InspectorName("DMS Conainer")]
    private GameObject dmsContainer;
    
    [SerializeField]
    [InspectorName("DMS Prefab")]
    private GameObject dmPrefab;
    
    [SerializeField]
    [InspectorName("Streaming info")]
    private TMP_Text streamingInfo;
    
    [SerializeField]
    [InspectorName("Dropdown quality filter")]
    private TMP_Dropdown qualityFilter;
    
    [SerializeField]
    [InspectorName("Dropdown type filter")]
    private TMP_Dropdown typeFilter;
    
    [SerializeField]
    [InspectorName("Lobby button")]
    private GameObject lobbyButton;
    
    private PhotonView pv;
    
    private Item currentItem;

    private ItemInventoryDisplay[] inventoryAux;
    
    public Item CurrentItem => currentItem;
    
    private int currentSlot = 0;
        
    private void Awake()
    {
        HasAnimation = true;
        
        pv = GetComponent<PhotonView>();

        if (pv.IsMine)
        {
            //We load the dropdowns with the Enum values
            qualityFilter.ClearOptions();
            qualityFilter.AddOptions(new List<string>(Enum.GetNames(typeof(ItemQuality))));
            
            typeFilter.ClearOptions();
            typeFilter.AddOptions(new List<string>(Enum.GetNames(typeof(ItemCategory))));
        }

        if (!GameManager.Instance.IsInMainRoom)
        {
            lobbyButton.SetActive(true);
        }
        else
        {
            lobbyButton.SetActive(false);
        }
    }

    public void CloseOnCLick()
    {
        MenuManager.Instance.CloseMenu();
    }
    
    public override void OpenAnimation()
    {
        if (!pv.IsMine) return;
        
        base.OpenAnimation();
        
        LoadContacts();
        LoadItems();
        LoadStreams();
        LoadStreamInfo();
        
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
        
        foreach (Transform dm in dmsContainer.transform)
        {
            Destroy(dm.gameObject);
        }

        try
        {
            foreach (Contact contact in GameManager.Instance.Player.Contacts)
            {
                GameObject contactObject = Instantiate(contactPrefab, conocidosContainer.transform);
                ContactDisplay contactObjectScript = contactObject.GetComponent<ContactDisplay>();
                contactObjectScript.Configure(contact);
                
                GameObject dmObject = Instantiate(dmPrefab, dmsContainer.transform);
                DMItem dmObjectScript = dmObject.GetComponent<DMItem>();
                
                bool connected = contact.PV != null;
                
                dmObjectScript.Configure(contact.Name, connected, contact.ID);
            }
        }
        catch (Exception e)
        {
            //Console.WriteLine(e);
        }
    }

    private void LoadStreams()
    {
        // We acces the callbakcs of the function
        StartCoroutine(SocialManager.Instance.GetStreams((streams) =>
        {
            foreach (Transform child in streamingContainer.transform)
            {
                Destroy(child.gameObject);
            }
            
            foreach (string stream in streams)
            {
                GameObject streamingObject = Instantiate(streamingPrefab, streamingContainer.transform);
                StreamingDisplay streamingObjectScript = streamingObject.GetComponent<StreamingDisplay>();
                streamingObjectScript.Configure(stream, this);
            }
        }));
    }

    private void LoadStreamInfo()
    {
        streamingInfo.text = $"nameOfYourStream?key={GameManager.Instance.Player.StreamKey}&username={GameManager.Instance.Player.ID}";
    }
    
    public void RemoveItem(Item item)
    {
        if (!pv.IsMine) return;
        
        int id = item.GetComponent<PhotonView>().ViewID;
        
        pv.RPC("RemoveItem", RpcTarget.AllBuffered, id);
        
        Initialize(false);
        LoadItems();
    }
    
    public void StartSpectatingStreaming(string streamName)
    {
        string baseUrl = "http://localhost:8080/hls/";
        streaming.multiStreamProperties[0].url[0] = baseUrl + streamName + ".m3u8";
            
        streaming.gameObject.SetActive(true);
        streaming.OnRestart();
        
        GameManager.Instance.Player.Stop();
        
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }
    
    public void StopSpectatingStreaming()
    {
        MenuManager.Instance.CloseMenu();
        
        streaming.gameObject.SetActive(false);
        
        GameManager.Instance.Player.Resume();
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void LoadItems()
    {
        List<ItemInventoryDisplay> aux = new List<ItemInventoryDisplay>();
        
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
                
                aux.Add(itemObjectScript);
            }
        }
        
        inventoryAux = aux.ToArray();
    }

    public void Filter()
    {
        //We get the category enum from the dropdown
        ItemCategory category = (ItemCategory) typeFilter.value;
        
        //We get the quality enum from the dropdown
        ItemQuality quality = (ItemQuality) qualityFilter.value;
        
        foreach (ItemInventoryDisplay itemDisplay in inventoryAux)
        {
             //If the item is not null and the category and quality are not the same
             if (itemDisplay.Item != null)
             {
                 if (itemDisplay.Item.ItemData.category != category && category != ItemCategory.ITEM)
                 {
                     itemDisplay.gameObject.SetActive(false);
                 }
                 else if (itemDisplay.Item.ItemData.quality != quality && quality != ItemQuality.Any)
                 {
                     itemDisplay.gameObject.SetActive(false);
                 }
                 else
                 {
                     itemDisplay.gameObject.SetActive(true);
                 }
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
        if (!pv.IsMine) return;
        
        //If theres already an item with the same name, add it to the stack
        foreach (Item item in items)
        {
            if (item.ItemData.name == itemToAdd)
            {
                item.Add(1);
                return;
            }
        }
        
        Item itemToAddToInventory = PhotonNetwork.Instantiate(Path.Combine("ItemsPrefabs", ItemManager.Instance.GetItem(itemToAdd).prefab.name), Vector3.zero, Quaternion.identity).GetComponent<Item>();
        GameObject o;
        o = itemToAddToInventory.gameObject;

        int id = o.GetComponent<PhotonView>().ViewID;
        
        pv.RPC("SetParent", RpcTarget.AllBuffered, id);
        
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
            pv.RPC("AddItem", RpcTarget.AllBuffered, id);
        }
        
        Initialize(false);
    }

    public void  AddItem(Item item)
    {
        if (!pv.IsMine) return;
        
        //If theres already an item with the same name, add it to the stack
        foreach (Item item2 in items)
        {
            //If it is a gun, we reload it
            if (item2 is Gun gun)
            {
                if (gun.ItemData.name == item.ItemData.name)
                {
                    gun.RestartAmmo();
                    return;
                }
            }
            
            if (item2.ItemData.name == item.ItemData.name)
            {
                item2.Add(1);
                return;
            }
        }
        
        Item itemObject = PhotonNetwork.Instantiate(Path.Combine("ItemsPrefabs", item.ItemData.prefab.name), Vector3.zero, Quaternion.identity).GetComponent<Item>();

        GameObject o;
        o = itemObject.gameObject;
        int id = o.GetComponent<PhotonView>().ViewID;
        
        pv.RPC("SetParent", RpcTarget.AllBuffered, id);
        
        //Reset the position and rotation of the item
        o.transform.localPosition = Vector3.zero;
        o.transform.localRotation = Quaternion.identity;
        
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
            pv.RPC("AddItem", RpcTarget.AllBuffered, id);
        }
        
        Initialize(false);
    }
    
    public void AddItem(SerializableItemData serializableItemData)
    {
        if (pv == null || !pv.IsMine) return;
        
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
        Item itemToAdd = PhotonNetwork.Instantiate(Path.Combine("ItemsPrefabs", itemData.prefab.name), Vector3.zero, Quaternion.identity).GetComponent<Item>();

        GameObject o;
        o = itemToAdd.gameObject;
        
        int id = o.GetComponent<PhotonView>().ViewID;
        
        pv.RPC("SetParent", RpcTarget.AllBuffered, id);
        
        //Reset the position and rotation of the item
        o.transform.localPosition = Vector3.zero;
        o.transform.localRotation = Quaternion.identity;
        
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
            pv.RPC("AddItem", RpcTarget.AllBuffered, id);
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
        if (!pv.IsMine) return;

        if (!isOpen)
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
    }

    private void ConfigureItems()
    {
        if (BuildingSystem.Instance != null)
        {
            BuildingSystem.Instance.CantBuild();
        }
        
        if (currentSlot < items.Count && currentSlot >= 0)
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
            
        if (currentSlot < principalInventorySlots.Length && currentSlot >= 0)
        {
            principalInventorySlots[currentSlot].Select();
        }
        
        //We enable the item we want to display
        pv.RPC("SetGameObjects", RpcTarget.AllBuffered, currentSlot);
        
        UpdateItemText();
        
        UpdateCrosshair();
    }
    
    public void StartEditing()
    {
        currentSlot = -1;
        
        ConfigureItems();
    }
    
    private void UpdateCrosshair()
    {
        crosshair.sprite = crosshairDefault;
        
        if (currentItem != null)
        {
            if (currentItem is Gun gun)
            {
                crosshair.sprite = gun.GetCrosshair();
            }
        }
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
        
        LoadItems();
        Initialize(false);
        
        UpdateItemsGameobject();
    }
    
    private void UpdateItemsGameobject()
    {
        //We update the position of the items in the inventory
        pv.RPC("SetItems", RpcTarget.AllBuffered);
        
        ConfigureItems();
    }

    [PunRPC]
    public void SetGameObjects(int index)
    {
        //We disable every item in the inventory
        for (int i = 0; i < inventoryContainer.transform.childCount; i++)
        {
            inventoryContainer.transform.GetChild(i).gameObject.SetActive(false);
        }

        if (inventoryContainer.transform.childCount > index && index >= 0)
        {
            inventoryContainer.transform.GetChild(index).gameObject.SetActive(true);
        }
    }

    [PunRPC]
    public void SetItems()
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] != null)
            {
                items[i].transform.SetParent(inventoryContainer.transform);
                items[i].transform.SetSiblingIndex(i);
            }
            else
            {
                //If there is a null item, we replace it with an empty item
                GameObject emptyItem = Instantiate(emptyItemPrefab, inventoryContainer.transform);
                emptyItem.transform.SetSiblingIndex(i);
            }
        }
    }

    [PunRPC]
    public void AddItem(int id)
    {
        GameObject gameObject = PhotonView.Find(id).gameObject;
        
        items.Add(gameObject.GetComponent<Item>());
    }
    
    [PunRPC]
    public void SetParent(int o)
    {
        //We found the gameobject with the photonview id
        GameObject gameObject = PhotonView.Find(o).gameObject;
        
        gameObject.GetComponent<BoxCollider>().enabled = false;
        
        gameObject.transform.SetParent(inventoryContainer.transform);
        
        //Reset the position and rotation of the item
        gameObject.transform.localPosition = Vector3.zero;
        gameObject.transform.localRotation = Quaternion.identity;
    }
    
    [PunRPC]
    public void RemoveItem(int id)
    {
        GameObject gameObject = PhotonView.Find(id).gameObject;
        
        items.Remove(gameObject.GetComponent<Item>());
        
        Destroy(gameObject);
    }
}
