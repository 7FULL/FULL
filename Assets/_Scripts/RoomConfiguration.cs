using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
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
    
    private PhotonView pv;
    
    public PhotonView PV => pv;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        
        _availableSpawnPoints = new bool[_spawnPoints.Length];
        
        pv = GetComponent<PhotonView>();
        
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
