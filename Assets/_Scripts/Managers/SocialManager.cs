using System;
using System.Collections;
using System.Collections.Generic;
using Agora.Rtc;
using Photon.Pun;
using UnityEngine;
using Random = UnityEngine.Random;

public class SocialManager : MonoBehaviour
{

    private string _agoraAppID = "666729d18268402bb2f5f3ad40601c49";
    
    private string _photonAppID = "e11858fa-121d-49c0-906f-665c53c4e2f6";

    // Singleton
    private static SocialManager _instance;
    
    public static SocialManager Instance => _instance;
    
    private IRtcEngine _rtcEngine;
    
    private string _channelName = null;
    
    private Contact _contact;
    
    public Contact Contact => _contact;
    
    public bool IsOnCall => _channelName != null;
    public bool IsBeingCalled => _contact != null;
    public bool IsAvailable => _channelName == null && _contact == null;
    
    public string ChannelName => _channelName;
    
    // In seconds
    private int _callReponseTimeout = 10;

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
        _rtcEngine = Agora.Rtc.RtcEngine.CreateAgoraRtcEngineEx();
        UserEventHandler handler = new UserEventHandler(this);
            
        RtcEngineContext context = new RtcEngineContext(_agoraAppID, 0,
            CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING,
            AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_GAME_STREAMING);
        _rtcEngine.Initialize(context);
        _rtcEngine.InitEventHandler(handler);
        
        // Set basic configuration
        _rtcEngine.EnableAudio();
        _rtcEngine.SetChannelProfile(CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_COMMUNICATION);
        _rtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
        
    }

    //TODO: Menu to wait for response from the other player
    
    public void Call(Player playerToCall)
    {
        PhotonView caller = null;

        int id = playerToCall.ID;
        
        foreach (Player player in GameObject.FindObjectsOfType<Player>())
        {
            if (player.ID == id)
            {
                caller = player.PV;
            }
        }
        
        if (caller == null)
        {
            Debug.Log("Player not found");
            return;
        }

        _contact = new Contact(caller, "Pepito");
        
        string channelName = GameManager.Instance.Player.PV.Controller.ActorNumber + "-" + PhotonNetwork.LocalPlayer.ActorNumber + Random.Range(1, 1000);

        StartCoroutine(CallTimeOut());
        
        //TODO: Open menu of calling someone

        GameManager.Instance.Player.PV.RPC("CallRPC", caller.Controller, GameManager.Instance.Player.ID, channelName);
    }

    IEnumerator CallTimeOut()
    {
        yield return new WaitForSecondsRealtime(_callReponseTimeout);
        
        RestartCallInfo();
        
        MenuManager.Instance.CloseMenu();
    }

    private void RestartCallInfo()
    {
        _contact = null;
        _channelName = null;
    }

    public void ReceiveCall(int callerID, string channelName)
    {
        PhotonView caller = null;
        
        foreach (Player player in GameObject.FindObjectsOfType<Player>())
        {
            if (player.ID == callerID)
            {
                caller = player.PV;
            }
        }
        
        Debug.Log(callerID);

        //TODO: Get the name of the caller from the contacts list
        
         _contact = new Contact(caller, "Pepito");
         
         Debug.Log(_contact);
         
         _channelName = channelName;
         
         MenuStruct menuStruct = MenuManager.Instance.GetMenu(Menu.RECEIVE_CALL);

         if (menuStruct.MenuType == Menu.NONE)
         {
             Debug.Log("Menu Receive Call not found");
             return;
         }
        
        menuStruct.Menu.GetComponent<ReceiveCallMenu>().Configure();
        
        MenuManager.Instance.OpenMenu(Menu.RECEIVE_CALL);
    }
    
    public void AcceptCall()
    {
        JoinCall();
        
        GameManager.Instance.Player.PV
            .RPC("AcceptCallRPC", _contact.PV.Controller);
    }

    public void JoinCall()
    {
        MenuStruct menuStruct = MenuManager.Instance.GetMenu(Menu.CALL);
        
        if (menuStruct.MenuType == Menu.NONE)
        {
            Debug.Log("Menu Receive Call not found");
            return;
        }
        
        StopCoroutine(CallTimeOut());
        
        menuStruct.Menu.GetComponent<OnGoingCallMenu>().Configure();
        
        MenuManager.Instance.OpenMenu(Menu.CALL);
            
        _rtcEngine.JoinChannel("", _channelName);
    }

    public void EndCall()
    {
        GameManager.Instance.Player.PV.RPC("EndCallRPC", RpcTarget.Others);
        
        MenuManager.Instance.CloseMenu();
        
        _rtcEngine.LeaveChannel();
    }
    
    [PunRPC]
    private void EndCallRPC()
    {
        _rtcEngine.LeaveChannel();
    }
    
    private void OnDestroy()
    {
        if (_rtcEngine == null) return;
        _rtcEngine.InitEventHandler(null);
        _rtcEngine.LeaveChannel();
        _rtcEngine.Dispose();
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

