using System;
using TMPro;
using UnityEngine;

public class OnGoingCallMenu: MenuUtils
{
    [InspectorName("Animator")]
    [SerializeField]
    private Animator _animator;
    
    [InspectorName("Contact Name")]
    [SerializeField]
    private TMP_Text _contactName;
    private void Awake()
    {
        HasAnimation = true;
    }

    public override void OpenAnimation()
    {
        _animator.SetBool("IsOpen", true);
    }
    
    public void Configure()
    {
        //TODO: Add the contact name. Text doesnt update
        _contactName.text = SocialManager.Instance.Contact.Name;
        Debug.Log(SocialManager.Instance.Contact);
    }
    
    public override void CloseAnimation()
    {
        _animator.SetBool("IsOpen", false);
    }
}