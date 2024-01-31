using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using Agora.Rtc;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Networking;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using SocketIOClient;
using SocketIOClient.Newtonsoft.Json;
using Random = UnityEngine.Random;

public class SocialManager : MonoBehaviour
{

    private const string AGORA_APP_ID = "666729d18268402bb2f5f3ad40601c49";

    // Singleton
    private static SocialManager _instance;
    
    public static SocialManager Instance => _instance;
    
    private IRtcEngine rtcEngine;
    
    private string channelName = null;
    
    private Contact contact;
    private Player requestContact;
    private Player contactToAdd;
    
    public Contact Contact => contact;
    public Player RequestContact => requestContact;
    
    public Player ContactToAdd => contactToAdd;
    
    private bool isOnCall = false;
    
    private bool isVideoCall = false;
    
    private bool isCalling = false;
    
    private bool isReceivingContactRequest = false;
    
    public bool IsReceivingContactRequest
    {
        get => isReceivingContactRequest;
        set { isReceivingContactRequest = value; }
    }

    public bool IsCalling => isCalling;
    public bool IsOnCall => isOnCall;
    public bool IsBeingCalled => contact != null;
    public bool IsAvailable => channelName == null && contact == null;

    public string ChannelName => channelName;
    
    // In seconds
    private int callReponseTimeout = 15;
    
    private Uri uri = new Uri("http://localhost:3000");

    private SocketIOUnity socket;
    
    private ChatMenu chatMenu;
    
    private Dictionary<string, List<DMS>> chatMessages = new Dictionary<string, List<DMS>>();

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public void StartDMSuscription(string id)
    {
        socket = new SocketIOUnity(uri, new SocketIOOptions
        {
            Query = new Dictionary<string, string>
            {
                { "userID", id},
                { "token", "patata" }
            },
            EIO = 4,
            Transport = SocketIOClient.Transport.TransportProtocol.WebSocket
        });
        socket.JsonSerializer = new NewtonsoftJsonSerializer();
        
        socket.OnDisconnected += (sender, e) =>
        {
            Debug.Log("disconnect: " + e);
        };
        socket.OnReconnectAttempt += (sender, e) =>
        {
            Debug.Log($"{DateTime.Now} Reconnecting: attempt = {e}");
        };
        socket.OnError += (sender, e) =>
        {
            Debug.Log("error: " + e);
        };
        
        socket.OnUnityThread("receive-dm", (response) =>
        {
            string msg = response.GetValue<string>(0);
            string author = response.GetValue<string>(1);
            
            string authorName = "";
            
            foreach (Contact authorContact1 in GameManager.Instance.Player.Contacts)
            {
                if (authorContact1.ID == author)
                {
                    authorName = authorContact1.Name;
                }
            }
            
            if(authorName != "")
            {
                MenuManager.Instance.PopUp("New message from " + authorName, false);
            }
            
            if (MenuManager.Instance.IsOpen(Menu.CHAT_MENU))
            {
                chatMenu.AddMessage(msg);
            }
            
            if (chatMessages.ContainsKey(author + "-" + GameManager.Instance.Player.ID))
            {
                chatMessages[author + "-" + GameManager.Instance.Player.ID].Add(new DMS(msg, false));
            }
            else
            {
                chatMessages.Add(author + "-" + GameManager.Instance.Player.ID, new List<DMS>());
                chatMessages[author + "-" + GameManager.Instance.Player.ID].Add(new DMS(msg, false));
            }
            
            //If the chat is longer than 100 messages, remove the first one
            if (chatMessages[author + "-" + GameManager.Instance.Player.ID].Count > 100)
            {
                chatMessages[author + "-" + GameManager.Instance.Player.ID].RemoveAt(0);
            }
        });
        ////
        
        socket.Connect();
    }
    
    public void SendDm(string id, string message)
    {
        socket.Emit("send-dm", message, GameManager.Instance.Player.ID, id);

        id = id + "-" + GameManager.Instance.Player.ID;
        
        if (chatMessages.ContainsKey(id))
        {
            chatMessages[id].Add(new DMS(message, true));
        }
        else
        {
            chatMessages.Add(id, new List<DMS>());
            chatMessages[id].Add(new DMS(message, true));
        }
        
        //If the chat is longer than 100 messages, remove the first one
        if (chatMessages[id].Count > 100)
        {
            chatMessages[id].RemoveAt(0);
        }
    }
    
    public void EnterDM(string userID)
    {
        //Search for the user in the contact list
        Contact[] contacts = GameManager.Instance.Player.Contacts;
        
        Contact contactAux = null;
        
        foreach (Contact contact in contacts)
        {
            if (contact.ID == userID)
            {
                contactAux = contact;
            }
        }
        
        if (contactAux == null)
        {
            Debug.Log("Contact not found");
            return;
        }
        
        ChatMenu x = (ChatMenu)MenuManager.Instance.GetMenu(Menu.CHAT_MENU).Menu;
        
        DMS[] dms = Array.Empty<DMS>();
        
        if (chatMessages.ContainsKey(userID + "-" + GameManager.Instance.Player.ID))
        {
            dms = chatMessages[userID + "-" + GameManager.Instance.Player.ID].ToArray();
        }
        
        x.Configure(contactAux, dms);
        
        chatMenu = x;

        MenuManager.Instance.OpenMenu(Menu.CHAT_MENU);
    }
    
    private void Start()
    {
        // Init RTC Engine
        rtcEngine = Agora.Rtc.RtcEngine.CreateAgoraRtcEngineEx();
        //UserEventHandler handler = new UserEventHandler(this);
            
        RtcEngineContext context = new RtcEngineContext(AGORA_APP_ID, 0,
            CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING,
            AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_GAME_STREAMING);
        rtcEngine.Initialize(context);
        //rtcEngine.InitEventHandler(handler);
        
        // Set basic configuration
        rtcEngine.EnableAudio();
        rtcEngine.SetChannelProfile(CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_COMMUNICATION);
        rtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
    }
    
    public void Call(Contact playerToCall, bool videoCall = false)
    {
        if (playerToCall.PV != null)
        {
            if (isCalling)
            {
                Debug.Log("Already calling");
                return;
            }
            
            contact = playerToCall;

            string channelToJoin = PhotonNetwork.LocalPlayer.ActorNumber + "-" + Random.Range(1, 1000);

            StartCoroutine(CallTimeOut());
            OnGoingCallMenu x = (OnGoingCallMenu)MenuManager.Instance.GetMenu(Menu.CALLING).Menu;
        
            x.Configure();
            
            MenuManager.Instance.ToGame();
            
            channelName = channelToJoin;

            MenuManager.Instance.OpenMenu(Menu.CALLING, true);

            GameManager.Instance.Player.PV.RPC("CallRPC", contact.PV.Controller, GameManager.Instance.Player.ID, channelToJoin, videoCall);
        
            isVideoCall = videoCall;
        
            isCalling = true;
        }
        else
        {
            Debug.Log("Player not available");
            MenuManager.Instance.PopUp("Player not available");
        }
    }

    IEnumerator CallTimeOut(int seconds = 0)
    {
        if (seconds != 0)
        {
            yield return new WaitForSecondsRealtime(seconds);
        }
        else
        {
            Debug.Log("Waiting for player to answer the call");
            yield return new WaitForSecondsRealtime(callReponseTimeout);
        }

        Clean();
        
        Debug.Log("Player didn't answer the call");
        
        MenuManager.Instance.CloseMenu();
    }

    public void Clean()
    {
        if (isVideoCall)
        {
            contact.PV.gameObject.GetComponent<Player>().DisableVideo();
        }
        
        contact = null;
        channelName = null;
        isOnCall = false;
        isVideoCall = false;
        isCalling = false;
    }

    public void ReceiveCall(string callerID, string channelToJoin, bool videoCall)
    {
        // If we already have the player as contact, we don't send the request
        Contact[] contactsAux = GameManager.Instance.Player.Contacts;

        Player player = GameObject.Find(callerID).GetComponent<Player>();

        bool aux = false;
        
        for (int i = 0; i < contactsAux.Length; i++)
        {
            if (contactsAux[i].ID == player.ID)
            {
                aux = true;
            }
        }

        if (!aux)
        {
            requestContact = player;
            
            GameManager.Instance.Player.PV.RPC("AcceptContactRequestRPC", player.PV.Controller, GameManager.Instance.Player.ID);
            
            MenuManager.Instance.PopUp("A player that has you as a contact and you dont, is calling you, add it first");
            
            return;
        }
        
        // Refresh contacts
        GameManager.Instance.Player.RefreshContacts();

        Contact[] contacts = GameManager.Instance.Player.Contacts;
        
        foreach (Contact contact in contacts)
        {
            if (contact.ID == callerID)
            {
                this.contact = contact;
            }
        }

        if (contact == null)
        {
            Debug.Log("Contact not found");
            return;
        }

        channelName = channelToJoin;
        
        isVideoCall = videoCall;
         
        MenuStruct menuStruct = MenuManager.Instance.GetMenu(Menu.RECEIVE_CALL);

        if (menuStruct.MenuType == Menu.NONE)
        {
            Debug.Log("MenuGameObject Receive Call not found");
            return;
        }

        ReceiveCallMenu menu = (ReceiveCallMenu)menuStruct.Menu;
        
        menu.Configure();
        
        MenuManager.Instance.OpenMenu(Menu.RECEIVE_CALL, true);
        
        StartCoroutine(CallTimeOut(13));
    }
    
    public void AcceptCall()
    {
        JoinCall();

        GameManager.Instance.Player.PV.RPC("AcceptCallRPC", contact.PV.Controller);
    }

    public void JoinCall()
    {
        if (contact == null)
        {
            Debug.Log("Contact not found");
            return;
        }
        
        //Si ya esta en una llamada, me salgo de la llamada
        if (isOnCall)
        {
            LeaveCall();
        }
        
        if (isVideoCall)
        {
            contact.PV.gameObject.GetComponent<Player>().EnableVideo();
        }
        
        StopAllCoroutines();

        MenuStruct menuStruct;
        
        if (isVideoCall)
        {
            menuStruct = MenuManager.Instance.GetMenu(Menu.VIDEO_CALL);
        }
        else
        {
            menuStruct = MenuManager.Instance.GetMenu(Menu.CALL);
        }
        
        if (menuStruct.MenuType == Menu.NONE)
        {
            Debug.Log("MenuGameObject Receive Call not found");
            return;
        }
        
        OnGoingCallMenu menu = (OnGoingCallMenu)menuStruct.Menu;

        menu.Configure();
        
        MenuManager.Instance.ToGame();   
        
        if(isVideoCall)
        {
            MenuManager.Instance.OpenMenu(Menu.VIDEO_CALL, true);
        }
        else
        {
            MenuManager.Instance.OpenMenu(Menu.CALL, true);
        }
        
        isOnCall = true;
            
        rtcEngine.JoinChannel("", channelName);
    }

    public void LeaveCall()
    {
        Debug.Log("Leave call");
        GameManager.Instance.Player.PV.RPC("LeaveCallRPC", contact.PV.Controller);
        EndCall();
    }

    public void EndCall()
    {
        if (contact == null)
        {
            Debug.Log("Contact not found");
            return;
        }
        
        Debug.Log("End call");
        
        MenuManager.Instance.CloseMenu();

        rtcEngine.LeaveChannel();

        Clean();
        
        MenuManager.Instance.Focus();
    }
    
    public void ClearContactRequest()
    {
        if (isReceivingContactRequest)
        {
            isReceivingContactRequest = false;
        
            contactToAdd = null;
        }
    }
    
    public void ContactRequest(Player player)
    {
        isReceivingContactRequest = true;
        
        contactToAdd = player;
        
        //Check if the player is already in the contact list
        Contact[] contacts = GameManager.Instance.Player.Contacts;
        
        foreach (Contact contact in contacts)
        {
            if (contact.ID == player.ID)
            {
                MenuManager.Instance.PopUp("Player already in contact list");
                return;
            }
        }

        GameManager.Instance.Player.PV.RPC("ReciveContactRequestRPC", player.PV.Controller, GameManager.Instance.Player.ID);
        
        MenuManager.Instance.PopUp("Contact request sent successfully");
    }
    
    public void ReceiveContactRequest(string playerID)
    {
        isReceivingContactRequest = true;
        
        Player player = GameObject.Find(playerID).GetComponent<Player>();
        
        if (player == null)
        {
            Debug.Log("Player not found");
            return;
        }
        
        contactToAdd = player;

        MenuManager.Instance.PopUp("New contact request");
    }
    
    public void AcceptContactRequest()
    {
        if (contactToAdd != null)
        {
            GameManager.Instance.Player.PV.RPC("AcceptContactRequestRPC", contactToAdd.PV.Controller);

            MenuManager.Instance.OpenMenu(Menu.CONTACT_REQUEST);
            
            isReceivingContactRequest = false;
        }else{
            Debug.Log("Player not found");
        }
    }
    
    /*public void DeclineContactRequest()
    {
        GameManager.Instance.Player.PV.RPC("DeclineContactRequestRPC", requestContact.PV.Controller);
        
        requestContact = null;
        
        MenuManager.Instance.CloseMenu();
    }*/
    
    private void OnDestroy()
    {
        if (rtcEngine == null) return;
        rtcEngine.InitEventHandler(null);
        rtcEngine.LeaveChannel();
        rtcEngine.Dispose();
    }

    public IEnumerator GetStreams(Action<List<string>> callback)
    {
        string url = "http://localhost:8080/stat";

        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error en la solicitud: " + www.error);
            }
            else
            {
                string data = www.downloadHandler.text;

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(data);

                XmlNodeList streamElements = xmlDoc.GetElementsByTagName("stream");
                List<string> aux = new List<string>();

                for (int i = 0; i < streamElements.Count; i++)
                {
                    aux.Add(streamElements[i].SelectSingleNode("name").InnerText);
                }

                callback(aux);
            }
        }
    }
}

public class DMS
{
    private string msg;
    public string Msg => msg;

    private bool sent;
    public bool Sent => sent;
    
    public DMS(string message, bool sent)
    {
        this.msg = message;
        this.sent = sent;
    }
}

/*
internal class UserEventHandler : IRtcEngineEventHandler
    {
        private readonly SocialManager _audioSample;

        internal UserEventHandler(SocialManager audioSample)
        {
            _audioSample = audioSample;
        }

        public override void OnError(int err, string msg)
        {
            Debug.Log("Error: " + err + " " + msg);
        }

        public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
        {
            Debug.Log("OnJoinChannelSuccess");
        }

        public override void OnRejoinChannelSuccess(RtcConnection connection, int elapsed)
        {

        }

        public override void OnLeaveChannel(RtcConnection connection, RtcStats stats)
        {

        }

        public override void OnClientRoleChanged(RtcConnection connection, CLIENT_ROLE_TYPE oldRole, CLIENT_ROLE_TYPE newRole, ClientRoleOptions newRoleOptions)
        {

        }

        public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
        {
            Debug.Log("OnUserJoined");
        }

        public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
        {
            Debug.Log("OnUserOffline");
        }
    }
*/
