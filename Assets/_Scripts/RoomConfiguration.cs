using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class RoomConfiguration : MonoBehaviourPunCallbacks
{
    //Singleton
    public static RoomConfiguration Instance;
    
    [SerializeField]
    [InspectorName("Spawn points")]
    private Transform[] _spawnPoints;
    
    private bool[] _availableSpawnPoints;
    
    [SerializeField]
    [InspectorName("Can player use items")]
    public bool CanPlayerUseItems = true;
    
    [SerializeField]
    [InspectorName("Stop player on enter")]
    public bool StopPlayerOnEnter = true;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        
        _availableSpawnPoints = new bool[_spawnPoints.Length];
        
        ConfigureRoom();
    }
    
    public virtual void HandlePlayer(Player player)
    {
        if (StopPlayerOnEnter)
        {
            player.Stop();
        }
    }

    public virtual void ConfigureRoom()
    {
        
    }

    public Vector3 GetSpawnPoint()
    {
        int spawnPointIndex = UnityEngine.Random.Range(0, _spawnPoints.Length);
        
        while (_availableSpawnPoints[spawnPointIndex])
        {
            spawnPointIndex = UnityEngine.Random.Range(0, _spawnPoints.Length);
        }
        
        _availableSpawnPoints[spawnPointIndex] = true;
        
        return _spawnPoints[spawnPointIndex].position;
    }
}
