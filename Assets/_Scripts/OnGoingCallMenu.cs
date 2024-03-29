using System;
using TMPro;
using UnityEngine;

public class OnGoingCallMenu: MenuUtils
{
    [InspectorName("Contact Name")]
    [SerializeField]
    private TMP_Text _contactName;
    private void Awake()
    {
        HasAnimation = true;
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

        if (contact != null)
        {
            _contactName.text = contact.Name;
        }
    }
}