using System;
using System.Collections;
using System.Collections.Generic;
using Agora.Rtc;
using Photon.Pun;
using UnityEngine;

public class SocialManager : MonoBehaviour
{

    private string _appID = "666729d18268402bb2f5f3ad40601c49";
    
    private string _photonAppID = "e11858fa-121d-49c0-906f-665c53c4e2f6";
    
    // Singleton
    private static SocialManager _instance;
    
    private IRtcEngine _rtcEngine;

    public IRtcEngine RtcEngine
    {
        get => _rtcEngine;
    }

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
            
        RtcEngineContext context = new RtcEngineContext(_appID, 0,
            CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING,
            AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_GAME_STREAMING);
        _rtcEngine.Initialize(context);
        _rtcEngine.InitEventHandler(handler);
        
        // Set basic configuration
        _rtcEngine.EnableAudio();
        _rtcEngine.SetChannelProfile(CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_COMMUNICATION);
        _rtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
        
    }

    public void VoiceCall(PhotonView other)
    {
        GameManager.Instance.Player.PV.RPC("ReceiveCall", other.Controller);
    }

    [PunRPC]
    private void ReceiveCall()
    {
        MenuManager.Instance.OpenMenu(Menu.RECEIVE_CALL);
    }
    
    private void OnDestroy()
    {
        if (RtcEngine == null) return;
        RtcEngine.InitEventHandler(null);
        RtcEngine.LeaveChannel();
        RtcEngine.Dispose();
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

