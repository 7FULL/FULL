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
    
    public void Configure(Contact contact = null)
    {
        //Debug.Log(this.gameObject.name);
        //Debug.Log(SocialManager.Instance.Contact.Name);
        //Debug.Log(_contactName.text);
        
        if (contact == null)
        {
            contact = SocialManager.Instance.Contact;
        }
        
        _contactName.text = contact.Name;
    }
    
    public override void CloseAnimation()
    {
        _animator.SetBool("IsOpen", false);
    }
}