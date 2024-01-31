using System;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class PopUp: MenuUtils
{
    [InspectorName("Text to PopUp")]
    [SerializeField]
    private TMP_Text text;
    
    private PhotonView pv;
    
    private void Awake()
    {
        HasAnimation = true;
        
        pv = GetComponentInParent<PhotonView>();
    }

    private void Start()
    {
         if (!pv.IsMine) return;
        
        MenuManager.Instance.SetPopUp(this);
    }
    
    public void Configure(string text)
    {
        this.text.text = text;
        
        base.OpenAnimation();
    }
}