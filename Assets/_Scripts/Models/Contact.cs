using System;
using Photon.Pun;

public class Contact
{
    private PhotonView pv;
    public string Name;
    public string ID;
    
    public PhotonView PV
    {
        get => pv;
    }
    
    public Contact(string name, string id)
    {
        Name = name;
        ID = id;
    }
    
    public void SetPV(PhotonView pv)
    {
        this.pv = pv;
    }
    
    public string ToJson()
    {
        string json = "{";
        json +=  "\"name\": \"" + Name + "\",";
        json +=  "\"id\": \"" + ID + "\"";
        json += "}";
        
        return json;
    }

    public override string ToString()
    {
        return "Name: " + Name + ", ID: " + ID;
    }
}

[Serializable]
public class ContactResponse
{
    public string name;
    public string id;
    
    public ContactResponse(string name, string id)
    {
        this.name = name;
        this.id = id;
    }
    
    public Contact ToContact()
    {
        return new Contact(name, id);
    }
}

[Serializable]
public class TopContactResponse
{
    public string name;
    public int coins;
    
    public TopContactResponse(string name, int coins)
    {
        this.name = name;
        this.coins = coins;
    }
}