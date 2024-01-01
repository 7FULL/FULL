using System;
using TMPro;
using UnityEngine;

public class ReceiveCallMenu: MenuUtils
{
    [InspectorName("Caller Name")]
    [SerializeField]
    private TMP_Text _callerName;

    private void Awake()
    {
        HasAnimation = true;
    }

    public void Configure()
    {
        _callerName.text = SocialManager.Instance.Contact.Name;
    }
}