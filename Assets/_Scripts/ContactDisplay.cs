using TMPro;
using UnityEngine;

public class ContactDisplay: MonoBehaviour
{
    private Contact contact;
    
    public Contact Contact => contact;
    
    [SerializeField]
    private TMP_Text name;
    
    public void Configure(Contact contact)
    {
        this.contact = contact;
        name.text = contact.Name;
    }
    
    public void OnClickCall()
    {
        SocialManager.Instance.Call(contact);
    }
    
    public void OnClickVideoCall()
    {
        SocialManager.Instance.Call(contact, true);
    }
}