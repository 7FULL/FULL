﻿using System;
using TMPro;
using UnityEngine;

public class ReceiveCallMenu: MenuUtils
{
    [InspectorName("Caller Name")]
    [SerializeField]
    private TMP_Text _callerName;

    [InspectorName("Animator")]
    [SerializeField]
    private Animator _animator;

    private void Awake()
    {
        HasAnimation = true;
    }

    public void Configure()
    {
        _callerName.text = SocialManager.Instance.Contact.Name;
    }
    
    public override void OpenAnimation()
    {
        _animator.SetBool("IsOpen", true);
    }
    
    public override void CloseAnimation()
    {
        _animator.SetBool("IsOpen", false);
    }
}