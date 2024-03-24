using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class DMItem : MonoBehaviour
{
    [SerializeField]
    private TMP_Text userName;

    [SerializeField]
    private GameObject connected;
    
    [SerializeField]
    private GameObject disConnected;
    
    private string _userID;

    public void Configure(string name, bool connected, string userID)
    {
        userName.text = name;
        this.connected.SetActive(connected);
        disConnected.SetActive(!connected);
        _userID = userID;
    }

    public void EnterDM()
    {
        if (connected.activeSelf)
        {
            SocialManager.Instance.EnterDM(_userID);
        }
    }
}
