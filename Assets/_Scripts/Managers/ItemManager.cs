using System.Collections.Generic;
using UnityEngine;

public class ItemManager: MonoBehaviour
{
    //Singleton
    public static ItemManager Instance { get; private set; }
    
    public EditorDictionary<Items,Item> items = new EditorDictionary<Items,Item>();
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    
    public Item GetItem(Items item)
    {
        return items[item];
    }

    public Item GetRandomItem()
    {
        Items randomItem = (Items) Random.Range(0, items.Count);
        
        return items[randomItem];
    }

    public Item GetRandomItem(ItemRarity rarity)
    {
        List<Items> itemsByRarity = new List<Items>();
        
        foreach (KeyValuePair<Items, Item> item in items)
        {
            if (item.Value.Rarity == rarity)
            {
                itemsByRarity.Add(item.Key);
            }
        }
        
        Items randomItem = itemsByRarity[Random.Range(0, itemsByRarity.Count)];
        
        return items[randomItem];
    }

    public Item GetRandomItem(ItemRarity[] rarities)
    {
        List<Items> itemsByRarity = new List<Items>();
        
        foreach (KeyValuePair<Items, Item> item in items)
        {
            foreach (ItemRarity rarity in rarities)
            {
                if (item.Value.Rarity == rarity)
                {
                    itemsByRarity.Add(item.Key);
                }
            }
        }
        
        Items randomItem = itemsByRarity[Random.Range(0, itemsByRarity.Count)];
        
        return items[randomItem];
    }
    
    public void DropItem(Item item, Vector3 position)
    {
        Instantiate(item.Prefab, position, Quaternion.identity);
    }
    
    public void DestroyItem(Item item)
    {
        // TODO: Clean the item in the inventory
    }
}