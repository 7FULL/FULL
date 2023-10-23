using System;
using System.Collections;
using System.Collections.Generic;
using Agora.Rtc;
using Photon.Pun;
using UnityEngine;
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
    
    public Contact Contact => contact;
    public Player RequestContact => requestContact;
    
    private bool isOnCall = false;
    
    private bool isVideoCall = false;
    
    private bool isCalling = false;
    
    private bool isReceivingContactRequest = false;
    
    public bool IsReceivingContactRequest => isReceivingContactRequest;
    public bool IsCalling => isCalling;
    public bool IsOnCall => isOnCall;
    public bool IsBeingCalled => contact != null;
    public bool IsAvailable => channelName == null && contact == null;

    public string ChannelName => channelName;
    
    // In seconds
    private int callReponseTimeout = 15;

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

    private void Start()
    {
        // Init RTC Engine
        rtcEngine = Agora.Rtc.RtcEngine.CreateAgoraRtcEngineEx();
        UserEventHandler handler = new UserEventHandler(this);
            
        RtcEngineContext context = new RtcEngineContext(AGORA_APP_ID, 0,
            CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING,
            AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_GAME_STREAMING);
        rtcEngine.Initialize(context);
        rtcEngine.InitEventHandler(handler);
        
        // Set basic configuration
        rtcEngine.EnableAudio();
        rtcEngine.SetChannelProfile(CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_COMMUNICATION);
        rtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
        
    }

    //TODO: Check if player is available before calling
    public void Call(Contact playerToCall, bool videoCall = false)
    {
        contact = playerToCall;

        string channelName = PhotonNetwork.LocalPlayer.ActorNumber + "-" + Random.Range(1, 1000);

        StartCoroutine(CallTimeOut());
        OnGoingCallMenu x = (OnGoingCallMenu)MenuManager.Instance.GetMenu(Menu.CALLING).Menu;
        
        x.Configure();

        MenuManager.Instance.OpenMenu(Menu.CALLING, true);

        GameManager.Instance.Player.PV.RPC("CallRPC", contact.PV.Controller, GameManager.Instance.Player.ID, channelName, videoCall);
        
        isVideoCall = videoCall;
        
        isCalling = true;
    }

    IEnumerator CallTimeOut(int seconds = 0)
    {
        if (seconds != 0)
        {
            yield return new WaitForSecondsRealtime(seconds);
        }
        else
        {
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

    public void ReceiveCall(string callerID, string channelName, bool videoCall)
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
            
            MenuManager.Instance.PopUp("A player that has you as a contact and you dont is calling you, add it first");
            
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

        this.channelName = channelName;
        
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
        
        MenuManager.Instance.CloseMenu();

        rtcEngine.LeaveChannel();

        Clean();
        
        MenuManager.Instance.Focus();
    }
    
    public void ClearContactRequest()
    {
        isReceivingContactRequest = false;
        
        requestContact = null;
    }
    
    public void ContactRequest(Player player)
    {
        isReceivingContactRequest = true;
        
        requestContact = player;
        
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
        
        requestContact = player;

        MenuManager.Instance.PopUp("New contact request");
    }
    
    public void AcceptContactRequest()
    {
        if (requestContact != null)
        {
            GameManager.Instance.Player.PV.RPC("AcceptContactRequestRPC", requestContact.PV.Controller);

            MenuManager.Instance.OpenMenu(Menu.CONTACT_REQUEST);
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
}

internal class UserEventHandler : IRtcEngineEventHandler
    {
        private readonly SocialManager _audioSample;

        internal UserEventHandler(SocialManager audioSample)
        {
            _audioSample = audioSample;
        }

        public override void OnError(int err, string msg)
        {

        }

        public override void OnJoinChannelSuccess(RtcConnection connection, int elapsed)
        {

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

        }

        public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
        {

        }
    }

