using System.Collections.Generic;
using UnityEngine;

public class ItemManager: MonoBehaviour
{
    //Singleton
    public static ItemManager Instance { get; private set; }
    
    //List of all items
    public List<ItemScriptableObject> Items { get; private set; }
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        
        LoadItems();
    }
    
    //This function will load all items ( ScriptableObjects ) from the Resources folder
    public void LoadItems()
    {
        Items = new List<ItemScriptableObject>();
        var items = Resources.LoadAll("Items", typeof(ItemScriptableObject));
        
        foreach (var item in items)
        {
            Items.Add((ItemScriptableObject) item);
        }
    }

    public void DropItem(Item item, Vector3 position)
    {
        throw new System.NotImplementedException();
    }

    public void DestroyItem(Item item)
    {
        throw new System.NotImplementedException();
    }
    
    public Item GetItem(Items item)
    {
        for (int i = 0; i < Items.Count; i++)
        {
            if (Items[i].name == item)
            {
                return Items[i].ToModel();
            }
        }
        
        return null;
    }
}