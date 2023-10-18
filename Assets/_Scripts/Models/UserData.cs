using System;

[Serializable]
public class UserData
{
    public Contact[] contacts;
    
    public int coins;
    
    public UserData(Contact[] contacts, int coins)
    {
        this.contacts = contacts;
        this.coins = coins;
        
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
    
    //TODO: Add it to the database
    public void AddContact(Contact contact)
    {
        Contact[] newContacts = new Contact[contacts.Length + 1];

        for (int i = 0; i < contacts.Length; i++)
        {
            newContacts[i] = contacts[i];
        }

        newContacts[newContacts.Length - 1] = contact;

        contacts = newContacts;
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
    
    public UserData ToUserData()
    {
        Contact[] newContacts = new Contact[contacts.Length];

        for (int i = 0; i < contacts.Length; i++)
        {
            newContacts[i] = contacts[i].ToContact();
        }
        
        return new UserData(newContacts, coins);
    }
}