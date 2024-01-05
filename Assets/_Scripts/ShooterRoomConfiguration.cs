using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class ShooterRoomConfiguration : RoomConfiguration
{
    bool startTimer = false;
    double timerIncrementValue;
    double startTime;
    [SerializeField] double timer = 20;
    ExitGames.Client.Photon.Hashtable CustomeValue;
    
    [SerializeField]
    [InspectorName("Time remaining")]
    private TMP_Text[] _timeRemaining;
    
    [SerializeField]
    [InspectorName("Time to play")]
    private int _timeToPlay = 60;
    
    private bool _isPlaying = false;
    
    public bool IsPlaying => _isPlaying;

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
            startTime = PhotonNetwork.Time;
            startTimer = true;
            CustomeValue.Add("StartTime", startTime);
            PhotonNetwork.CurrentRoom.SetCustomProperties(CustomeValue);
        }
        else
        {
            startTime = double.Parse(PhotonNetwork.CurrentRoom.CustomProperties["StartTime"].ToString());
            startTimer = true;
        }
    }
    
    void Update()
    {
        if (!startTimer) return;
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
    
    private void StartGame()
    {
        startTimer = false;
        
        Player[] players = FindObjectsOfType<Player>();
        
        foreach (Player player in players)
        {
            player.Resume();
        }
        
        _isPlaying = true;
        
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

        if (_isPlaying)
        {
            //TODO: Spectator
        }
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);

        //We count the players in the room
        Player[] players = FindObjectsOfType<Player>();
        
        //If there is only 2 players left we search for the winner
        if (players.Length == 2)
        {
            Player winner = null;
            
            foreach (Player player in players)
            {
                if (player.PV.Controller.ActorNumber != otherPlayer.ActorNumber)
                {
                    winner = player;
                }
            }
            
            EndGame(winner);
        }
    }

    public void EndGame(Player winner)
    {
        //We change the text to play
        for (int i = 0; i < _timeRemaining.Length; i++)
        {
            _timeRemaining[i].text = "END";
        }

        //TODO: Particulas de victoria
        
        winner.WinHungerGames();
    }
}
