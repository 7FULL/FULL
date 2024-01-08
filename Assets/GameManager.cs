using System;
using System.Collections;
using Photon.Pun;
using Photon.Realtime;
using SunTemple;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.Video;

public class GameManager : MonoBehaviourPunCallbacks
    {
	    public static GameManager Instance;

	    [Tooltip("The prefab to spawn when the player joins the room")]
        [SerializeField]
        private GameObject playerPrefab;
        
        [Tooltip("Spawn point")]
        [SerializeField]
        private Transform spawnPoint;
        
        // The name of the room where the player will be connected
        private Rooms mainRoom = Rooms.LOBBY;
        
        //Current room
        private Rooms currentRoom;
        
        public Rooms CurrentRoom => currentRoom;
        
        public bool IsInMainRoom => currentRoom == mainRoom;
        
        private Player player;

        [SerializeField] private GameObject chat;

        private ApiClient apiClient = new ApiClient("http://localhost:3000/api/");
        //private ApiClient apiClient = new ApiClient("https://full-apirest.onrender.com/api/");
        
        public ApiClient ApiClient => apiClient;

        public Player Player => player;
        
        [SerializeField] private VideoPlayer videoPlayer;
        
        [SerializeField] private GameObject loadingCanvas;

        [SerializeField] private TMP_InputField contactInput;
        
        [SerializeField]
        [InspectorName("Opacar Imagen")]
        private GameObject opacarImagen;
        
        [FormerlySerializedAs("prodMode")]
        [SerializeField]
        [InspectorName("Prod Mode")]
        [Tooltip("Is this a production build?")]
        private bool prodBuild = false;
        
        public bool ProdBuild => prodBuild;

        public override void OnEnable()
        {
	        base.OnEnable();
	        
	        JoinRoom(SceneManager.GetActiveScene().name);
        }

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
	        
	        //PhotonNetwork.AutomaticallySyncScene = true;
	        
	        //DontDestroyOnLoad(this.gameObject);

	        if (prodBuild)
	        {
		        apiClient = new ApiClient("https://full-apirest.onrender.com/api/");
	        }
        }
        
        public void EnableChat()
		{
	        chat.SetActive(true);
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
	        
	        currentRoom = (Rooms) Enum.Parse(typeof(Rooms), room);
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

        public void AddContact()
        {
	        Player player = SocialManager.Instance.ContactToAdd;
	        
	        Debug.Log("Add contact" + player.ID + " " + contactInput.text);

	        Contact contact = new Contact(contactInput.text, player.ID);
	        
	        contact.SetPV(player.PV);

	        this.player.AddContact(contact);
	        
	        SocialManager.Instance.ClearContactRequest();
	        
	        contactInput.text = "";
        }

        public override void OnJoinedRoom()
        {
	        if (player == null)
	        {
		        if (SceneManager.GetActiveScene().name != mainRoom.ToString())
		        {
			        spawnPoint.position = RoomConfiguration.Instance.GetSpawnPoint();
		        }
			        
		        // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
		        GameObject player = PhotonNetwork.Instantiate(this.playerPrefab.name, spawnPoint.position, spawnPoint.rotation, 0);
		        
		        this.player = player.GetComponent<Player>();

		        if (SceneManager.GetActiveScene().name != mainRoom.ToString())
		        {
			        EnableChat();
			        this.player.EnableMainCanvas();
			        //this.player.Resume();
			        
			        videoPlayer.transform.parent.gameObject.SetActive(false);
			        loadingCanvas.SetActive(false);
			        opacarImagen.SetActive(false);
		        }
		        else
		        {
			        StartCoroutine(WaitForVideo());
		        }
		        
		        //If the scene is not the main room, we let the room handle the player
		        if (SceneManager.GetActiveScene().name != mainRoom.ToString())
		        {
			        RoomConfiguration.Instance.HandlePlayer(this.player);
		        }
	        }
	        else
	        {
		        Debug.Log("Connected to room but player already instantiated");
	        }
        }

        public void TPToSpawn(Player player)
        {
	        Destroy(player.gameObject);

	        // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
	        GameObject player2 = PhotonNetwork.Instantiate(this.playerPrefab.name, spawnPoint.position, spawnPoint.rotation, 0);
		        
	        this.player = player2.GetComponent<Player>();
        }

        IEnumerator WaitForVideo()
        {
	        yield return new WaitUntil(() => !videoPlayer.isPlaying);
	        
	        videoPlayer.gameObject.SetActive(false);
	        
	        loadingCanvas.SetActive(false);
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
	        
	        player = null;
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

		public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
		{
			//Find the player and if it is contact in the call leaveroom
			if (SocialManager.Instance.Contact != null && SocialManager.Instance.Contact.PV.Controller.ActorNumber == otherPlayer.ActorNumber)
			{
				SocialManager.Instance.EndCall();
			}
		}

		public override void OnDisconnected(DisconnectCause cause)
		{
			player = null;
			MenuManager.Instance.OpenMenu(Menu.RECONNECT);
		}

		public override void OnLeftRoom()
		{
			player = null;
		}
    }