using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Photon.Pun;
using TMPro;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class HungerGamesRoomConfiguration : RoomConfiguration
{
    bool startTimer = false;
    double timerIncrementValue;
    double startTime;
    [SerializeField] double timer = 20;
    ExitGames.Client.Photon.Hashtable CustomeValue;
    ExitGames.Client.Photon.Hashtable HasStarted;
    ExitGames.Client.Photon.Hashtable PlayersPlaying;
    
    [SerializeField]
    [InspectorName("Time remaining")]
    private TMP_Text[] _timeRemaining;
    
    [SerializeField]
    [InspectorName("People to play")]
    [Tooltip("The number of people to start the game")]
    private int _peopleToPlay = 2;
    
    private bool _isPlaying = false;
    
    public bool IsPlaying => _isPlaying;
    
    private bool isSpectating = false;
    
    public bool IsSpectating => isSpectating;

    public override void ConfigureRoom()
    {
        base.ConfigureRoom();
        
        CanPlayerUseItems = false;
        
        MenuManager.Instance.ClearNulls();
    }

    public override void HandlePlayer(Player player)
    {
        base.HandlePlayer(player);
        
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            CustomeValue = new ExitGames.Client.Photon.Hashtable();
            HasStarted = new ExitGames.Client.Photon.Hashtable();
            startTime = PhotonNetwork.Time;
            CustomeValue.Add("StartTime", startTime);
            HasStarted.Add("HasStarted", false);
            PhotonNetwork.CurrentRoom.SetCustomProperties(CustomeValue);
            PhotonNetwork.CurrentRoom.SetCustomProperties(HasStarted);
            
            //startTimer = true;
        }
        else
        {
            startTime = double.Parse(PhotonNetwork.CurrentRoom.CustomProperties["StartTime"].ToString());
            _isPlaying = bool.Parse(PhotonNetwork.CurrentRoom.CustomProperties["HasStarted"].ToString());
        }
        
        
        if (_isPlaying)
        {
            AdvertiseSpectator();
        }
        else
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount >= _peopleToPlay)
            {
                startTime = PhotonNetwork.Time;
                
                startTimer = true;
            }
        }
    }
    
    void Update()
    {
        if (!startTimer && !_isPlaying)
        {
            //Waiting text
            for (int i = 0; i < _timeRemaining.Length; i++)
            {
                _timeRemaining[i].text = "WAITING";
            }
        }

        if (startTimer && !_isPlaying)
        {
            timerIncrementValue = PhotonNetwork.Time - startTime;
        
            foreach (TMP_Text text in _timeRemaining)
            {
                text.text = (timer - timerIncrementValue).ToString("F0");
            }
        
            if (timerIncrementValue >= timer)
            {
                StartGame();
            }
        }

        if (_isPlaying)
        {
            //Playing text
            for (int i = 0; i < _timeRemaining.Length; i++)
            {
                _timeRemaining[i].text = "PLAYING";
            }
        }
    }
    
    private void StartGame()
    {
        startTimer = false;
        
        Player[] players = FindObjectsOfType<Player>();
        
        foreach (Player player in players)
        {
            player.Resume();
        }
        
        _isPlaying = true;

        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            //Update the room propertie HasStarted
            HasStarted = new Hashtable();
            HasStarted.Add("HasStarted", true);
            
            //We save how many players are playing in case spectators join
            PlayersPlaying = new Hashtable();
            PlayersPlaying.Add("PlayersPlaying", PhotonNetwork.PlayerList);
            
            PhotonNetwork.CurrentRoom.SetCustomProperties(HasStarted);
            PhotonNetwork.CurrentRoom.SetCustomProperties(PlayersPlaying);
        }
        
        //We change the text to play
        for (int i = 0; i < _timeRemaining.Length; i++)
        {
            _timeRemaining[i].text = "PLAY!!!";
        }
        
        MenuManager.Instance.PopUp("THE HUNGER GAMES HAS STARTED");
        
        CanPlayerUseItems = true;
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);

        if (!_isPlaying && !startTimer)
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount >= _peopleToPlay && PhotonNetwork.LocalPlayer.IsMasterClient)
            {
                startTimer = true;
                //Reset the timer
                CustomeValue = new Hashtable();
                startTime = PhotonNetwork.Time;
                CustomeValue.Add("StartTime", startTime);
                    
                PhotonNetwork.CurrentRoom.SetCustomProperties(CustomeValue);
            }
        }
    }
    
    private void AdvertiseSpectator()
    {
        //We wait 2 seconds to show the message using DOTween
        DOVirtual.DelayedCall(2f, () =>
        { 
            MenuManager.Instance.PopUp("THE HUNGER GAMES WAS ALREADY STARTED, YOU ARE SPECTATING");
        });
        
        isSpectating = true;
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        
        //We get the players that started playing
        Photon.Realtime.Player[] playersPlaying = (Photon.Realtime.Player[]) PhotonNetwork.CurrentRoom.CustomProperties["PlayersPlaying"];
        
        //We get the players playing right now
        Photon.Realtime.Player[] playersPlayingNow = PhotonNetwork.PlayerList;
        
        //We check how many original players are left
        int playersLeft = 0;
        
        foreach (Photon.Realtime.Player player in playersPlaying)
        {
            foreach (Photon.Realtime.Player playerNow in playersPlayingNow)
            {
                if (player != null && playerNow != null && player.ActorNumber == playerNow.ActorNumber)
                {
                    playersLeft++;
                }
            }
        }
        
        //If there is no players left, we end the game
        if (playersLeft <= 1)
        {
            if (!isSpectating)
            {
                //TODO mandar un rpc a todos los jugadores usando el PV del gamemanager
                EndGame();
            }
        }
    }

    public void EndGame()
    {
        //We change the text to play
        for (int i = 0; i < _timeRemaining.Length; i++)
        {
            _timeRemaining[i].text = "END";
        }

        //TODO: Particulas de victoria
        
        GameManager.Instance.Player.WinHungerGames();
    }
}
