using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class ChatManager : MonoBehaviour
{
    [SerializeField]
    [InspectorName("Chat")]
    private TMP_Text chat;
    
    [SerializeField]
    [InspectorName("Input")]
    private TMP_InputField input;
    
    private PhotonView pv;
    
    //Singleton
    public static ChatManager Instance;
    
    private void Awake()
    {
        pv = GetComponent<PhotonView>();
        
        if (Instance == null)
        {
            Instance = this;
        }
    }
    
    [PunRPC]
    public void AddMessage(string message)
    {
        chat.text += message + "\n";
        
        //If chat has more than 10 lines, remove first line
        if (chat.text.Split('\n').Length > 10)
        {
            chat.text = chat.text.Remove(0, chat.text.IndexOf('\n') + 1);
        }
    }
    
    public void AddMessageRPC(string message)
    {
        pv.RPC("AddMessage", RpcTarget.Others, message);
        input.text = "";
        
        chat.text += "<color=#72DE42>" + message + "</color>\n";
        
        //If chat has more than 10 lines, remove first line
        if (chat.text.Split('\n').Length > 10)
        {
            chat.text = chat.text.Remove(0, chat.text.IndexOf('\n') + 1);
        }
        
        //Desfocus input
        input.DeactivateInputField(true);
    }
    
    public void CheckEnter()
    {
        //If last character is enter
        if (input.text.Length > 0 && input.text[input.text.Length - 1] == '\n')
        {
            //Remove last character
            input.text = input.text.Remove(input.text.Length - 1);
            SendMessage();
        }
    }
    
    public void SendMessage()
    {
        if (input.text != "")
        {
            AddMessageRPC(input.text);
            input.text = "";
            
            //Desfocus input
            input.DeactivateInputField();
        }
    }
    
    public void FocusChat()
    {
        input.ActivateInputField();
    }

    public void OnFocus()
    {
        GameManager.Instance.Player.Stop();
    }
    
    public void OnUnfocus()
    {
        GameManager.Instance.Player.Resume();
        input.DeactivateInputField();
    }
}