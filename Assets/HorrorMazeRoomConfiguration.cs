using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HorrorMazeRoomConfiguration : RoomConfiguration
{
    [SerializeField]
    [InspectorName("Monster")]
    private MonsterHorrorMaze monster;
    
    public override void ConfigureRoom()
    {
        base.ConfigureRoom();
        
        Invoke("ShowMsg", 3);
    }

    public override void HandlePlayer(Player player)
    {
        base.HandlePlayer(player);
        
        player.ActivateScareMode();
        
        monster.AddPlayer(player);
    }

    private void ShowMsg()
    {
        MenuManager.Instance.ClearNulls();
        
        MenuManager.Instance.PopUp("Find the code and exit the maze");
        
        Player[] players = FindObjectsOfType<Player>();
        
        foreach (Player player in players)
        {
            player.Resume();
            player.IncreaseSprintSpeed();
        }
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        
        Invoke("UpdateMonsterPlayers", 3);
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        
        Invoke("UpdateMonsterPlayers", 3);
    }

    private void UpdateMonsterPlayers()
    {
        monster.UpdatePlayers();
    }
}
