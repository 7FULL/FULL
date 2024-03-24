using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ChatMenu : MenuUtils
{
    [SerializeField] 
    [InspectorName("User name")]
    private TMP_Text username;
    
    [SerializeField]
    [InspectorName("Message input")]
    private TMP_InputField messageInput;
    
    [SerializeField]
    [InspectorName("Receive message")]
    private GameObject receiveMessage;
    
    [SerializeField]
    [InspectorName("Send message")]
    private GameObject sendMessage;
    
    [SerializeField]
    [InspectorName("Chat")]
    private GameObject chat;
    
    private Contact _contact;
    
    public void Configure(Contact user, DMS[] msg)
    {
        _contact = user;
        
        username.text = user.Name;
        
        //Clear chat
        foreach (Transform child in chat.transform)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < msg.Length; i++)
        {
            if (msg[i].Sent)
            {
                ChatItem chatItem = Instantiate(sendMessage, chat.transform).GetComponent<ChatItem>();
            
                chatItem.Configure(msg[i].Msg, true);
            }
            else
            {
                ChatItem chatItem = Instantiate(receiveMessage, chat.transform).GetComponent<ChatItem>();
            
                chatItem.Configure(msg[i].Msg, false);
            }
        }
    }

    public void CheckEnter()
    {
        //If last character is enter
        if (messageInput.text.Length > 0 && messageInput.text[messageInput.text.Length - 1] == '\n')
        {
            //Remove last character
            messageInput.text = messageInput.text.Remove(messageInput.text.Length - 1);
            SendDM();
        }
    }
    
    public void SendDM()
    {
        if (messageInput.text != "")
        {
            SocialManager.Instance.SendDm(_contact.ID, messageInput.text);
            
            ChatItem chatItem = Instantiate(sendMessage, chat.transform).GetComponent<ChatItem>();
            
            chatItem.Configure(messageInput.text, true);
            
            messageInput.text = "";
        }
    }
    
    public void AddMessage(string message)
    {
        ChatItem chatItem = Instantiate(receiveMessage, chat.transform).GetComponent<ChatItem>();
        
        chatItem.Configure(message, false);
    }
}
