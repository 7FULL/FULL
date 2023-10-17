using System;
using System.Collections;
using Photon.Pun;
using Photon.Realtime;
using SunTemple;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviourPunCallbacks
    {
	    public static GameManager Instance;

	    private GameObject instance;

	    [Tooltip("The prefab to spawn when the player joins the room")]
        [SerializeField]
        private GameObject playerPrefab;
        
        [Tooltip("Spawn point")]
        [SerializeField]
        private Transform _spawnPoint;
        
        // The name of the room where the player will be connected
        private Rooms _mainRoom = Rooms.LOBBY;
        
        private Player _player;

        public Player Player => _player;
        
        
        
        private void Awake()
        {
	        PhotonNetwork.ConnectUsingSettings();
	        
	        if (Instance == null)
	        {
		        Instance = this;
	        }
	        else
	        {
		        Destroy(this.gameObject);
	        }
	        
	        PhotonNetwork.AutomaticallySyncScene = true;
	        
	        SceneManager.sceneLoaded += OnSceneLoaded;
	        
	        DontDestroyOnLoad(this.gameObject);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
		{
			Debug.LogError("Joining room "+ scene.name);
			JoinRoom(scene.name);
		} 
        
        public void JoinRoom(Rooms room)
        {
	        if (PhotonNetwork.InRoom)
	        {
		        PhotonNetwork.LeaveRoom();
	        }
	        else
	        {
		        StartCoroutine(WaitForJoin(room.ToString()));
		        return;
	        }
	        
	        StartCoroutine(WaitForLoadScene(room));
        }
        
        public void JoinRoom(string room)
        {
	        if (PhotonNetwork.InRoom)
	        {
		        PhotonNetwork.LeaveRoom();
	        }
	        else
	        {
		        StartCoroutine(WaitForJoin(room));
		        return;
	        }

	        StartCoroutine(WaitForLoadScene(room));
        }

        IEnumerator WaitForJoin(string room)
        {
	        yield return new WaitUntil(() => PhotonNetwork.InLobby);
	        
	        PhotonNetwork.JoinOrCreateRoom(room, null, null);
        }

	    IEnumerator WaitForLoadScene(Rooms room)
		{
	        if (SceneManager.GetActiveScene().name == room.ToString())
	        {
		        yield return null;
	        }
	        
	        yield return new WaitUntil(() => PhotonNetwork.IsConnectedAndReady);

	        SceneManager.LoadScene(room.ToString());
		}
        
        IEnumerator WaitForLoadScene(string room)
        {
	        if (SceneManager.GetActiveScene().name == room.ToString())
	        {
		        yield return null;
	        }
	        
	        yield return new WaitUntil(() => PhotonNetwork.IsConnectedAndReady);

	        SceneManager.LoadScene(room.ToString());
        }
        
        // Open all the doors
        public void OpenAllDoors()
		{
	        Door[] doors = FindObjectsOfType<Door>();

	        foreach (Door door in doors)
	        {
		        door.EditorOpen();
	        }
		}

        public override void OnJoinedRoom()
        {
	        if (_player == null)
	        {
		        // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
		        GameObject player = PhotonNetwork.Instantiate(this.playerPrefab.name, _spawnPoint.position, Quaternion.identity, 0);
		        
		        _player = player.GetComponent<Player>();
	        }
	        else
	        {
		        Debug.Log("Connected to room but player already instantiated");
	        }
        }

        public override void OnConnectedToMaster()
        {
	        PhotonNetwork.JoinLobby();
        }

        public override void OnLeftLobby()
        {
	        if (!PhotonNetwork.IsConnected)
	        {
		        MenuManager.Instance.OpenMenu(Menu.RECONNECT);
	        }
	        
	        _player = null;
        }
        
        public void Reconnect()
		{
	        // We try to connect and wait 5 seconds to check if we are connected
	        PhotonNetwork.ConnectUsingSettings();     
	        Invoke(nameof(CheckConnection), 5f);
		}
        
        private void CheckConnection()
		{
			// If we are not connected, we show the canvas group
	        if (!PhotonNetwork.IsConnected)
	        {
		        MenuManager.Instance.OpenMenu(Menu.RECONNECT);
	        }
		}

		public void Disconnect()
		{
	        PhotonNetwork.Disconnect();
		}

		public void Quit()
		{
	        Application.Quit();
		}

		public override void OnDisconnected(DisconnectCause cause)
		{
			_player = null;
			MenuManager.Instance.OpenMenu(Menu.RECONNECT);
		}

		public override void OnLeftRoom()
		{
			_player = null;
		}
    }