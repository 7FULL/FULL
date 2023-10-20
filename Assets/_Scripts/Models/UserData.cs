using System;
using UnityEngine;

public class UserData
{
    private Contact[] contacts;
    
    public Contact[] Contacts => contacts;

    private int coins;

    private string id;
    
    public int Coins => coins;
    
    private ApiClient api;
    
    public UserData(Contact[] contacts, int coins, string id, ApiClient api)
    {
        this.contacts = contacts;
        this.coins = coins;
        this.id = id;
        this.api = api;
        
        Check();
    }

    private void Check()
    {
        if (coins < 0)
        {
            coins = 0;
        }
    }
    
    public void AddCoins(int amount)
    {
        coins += amount;
        Check();
    }
    
    public void RemoveCoins(int amount)
    {
        coins -= amount;
        Check();
    }

    public void Reset()
    {
        coins = 0;
    }

    public override string ToString()
    {
        string result = "";

        if (contacts.Length > 0)
        {
            foreach (Contact contact in contacts)
            {
                result += contact.ToString() + "\n";
            }
        }
        
        result += "Coins: " + coins;
        
        return result;
    }
    
    public void AddToContact(Contact contact)
    {
        if (contact == null)
        {
            Debug.LogError("Contact is null");
        }
        
        if (contacts.Length > 0)
        {
            Contact[] newContacts = new Contact[contacts.Length + 1];

            for (int i = 0; i < contacts.Length; i++)
            {
                newContacts[i] = contacts[i];
            }

            newContacts[newContacts.Length - 1] = contact;

            contacts = newContacts;
        }
        else
        {
            contacts = new Contact[] { contact };
        }
        
        string json = "{";
        json +=  "\"contact\":" + contact.ToJson() + ",";
        json +=  "\"user\": \"" + id + "\"";
        json += "}";

        api.Post("contact/add", json);
        
        //Close the menu
        MenuManager.Instance.CloseMenu();
    }

    public void RemoveContact(Contact contact)
    {
        //TODO: Remove it from the database
    }
    
    
    //TODO: Everytime we do something with the coins we should do it in the back
}

[Serializable]
public class UserDataResponse
{
    public ContactResponse[] contacts;
    
    public int coins;

    public UserDataResponse(ContactResponse[] contacts, int coins)
    {
        this.contacts = contacts;
        this.coins = coins;
    }
    
    public UserData ToUserData(ApiClient api, string id)
    {
        Contact[] newContacts = new Contact[contacts.Length];

        for (int i = 0; i < contacts.Length; i++)
        {
            newContacts[i] = contacts[i].ToContact();
        }
        
        UserData userData = new UserData(newContacts, coins, id, api);
        
        return userData;
    }
}