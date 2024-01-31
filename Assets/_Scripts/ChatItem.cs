using System;
using TMPro;
using UnityEngine;

public class ChatItem: MonoBehaviour
{
    [SerializeField]
    [InspectorName("Message")]
    private TMP_Text _message;
    
    RectTransform rectTransform;
    
    public void Configure(string message, bool sent)
    {
        rectTransform = GetComponentInChildren<RectTransform>();
        
        //We insert enters to the message every 25 characters
        string newMessage = "";
        
        for (int i = 0; i < message.Length; i++)
        {
            if (i % 25 == 0 && i != 0)
            {
                newMessage += "\n";
            }
            
            newMessage += message[i];
        }

        _message.text = newMessage;
    }
}