using System;

[Serializable]
public class UserData
{
    public Contact[] contacts;
    
    public int coins;

    public void Check()
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
}