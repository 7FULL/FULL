using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemInventoryDisplay : MonoBehaviour
{
    private Item item;
    private Inventory inventory;
    
    [SerializeField]
    [InspectorName("Item Image")]
    private Image itemImage;
    
    [SerializeField]
    [InspectorName("Position dropdown")]
    private TMP_Dropdown positionDropdown;
    
    private bool emitEvent = true;

    private void Awake()
    {
        positionDropdown.onValueChanged.AddListener(delegate { OnChanged(); });
    }

    public void Configure(Item item, Inventory inventory, int position)
    {
        if (item == null || inventory == null)
        {
            return;
        }
        
        this.item = item;
        this.inventory = inventory;
        
        itemImage.sprite = item.ItemData.sprite;
        
        emitEvent = false;
        
        //If position is greater than 4, it means that item is not in principal inventory so we select the last option
        if (position > 4)
        {
            positionDropdown.value = 4;
        }
        else
        {
            positionDropdown.value = position;
        }
        
        emitEvent = true;
    }
    
    public void OnChanged()
    {
        if (emitEvent)
        {
            inventory.ChangePosition(item, positionDropdown.value);
        }
        else
        {
            Debug.Log("No emitir evento");
        }
    }
}
