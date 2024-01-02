using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemDisplay : MonoBehaviour
{
    [SerializeField]
    [InspectorName("Select frame")]
    private GameObject selectFrame;
    
    [SerializeField]
    [InspectorName("Item image")]
    private Image itemImage;
    
    [SerializeField]
    [InspectorName("Default sprite")]
    private Sprite defaultSprite;
    
    private Item item;

    private void Awake()
    {
        if (selectFrame != null)
        {
            selectFrame.SetActive(false);
        }
    }

    public void Select()
    {
        selectFrame.SetActive(true);
    }
    
    public void UnSelect()
    {
        selectFrame.SetActive(false);
    }
    
    public void Configure(Item item)
    {
        this.item = item;

        if (item != null)
        {
            itemImage.sprite = item.ItemData.sprite;
        }
        else
        {
            itemImage.sprite = defaultSprite;
        }
    }
}
