using System;
using TMPro;
using UnityEngine;

public class PopUp: MenuUtils
{
    [InspectorName("Text to PopUp")]
    [SerializeField]
    private TMP_Text text;
    
    private void Awake()
    {
        HasAnimation = true;
    }

    private void Start()
    {
        MenuManager.Instance.SetPopUp(this);
    }
    
    public void Configure(string text)
    {
        this.text.text = text;
    }
}