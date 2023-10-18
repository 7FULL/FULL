using System;
using Photon.Pun;

[Serializable]
public class Contact
{
    PhotonView _pv;
    string _name;
    
    public Contact(PhotonView pv, string name)
    {
        _pv = pv;
        _name = name;
    }
    
    public PhotonView PV => _pv;
    public string Name => _name;
    
    public override string ToString()
    {
        return $"Contact: {_name} - {_pv.ViewID}";
    }
}