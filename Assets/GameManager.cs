﻿using System;
using System.Collections;
using System.Threading.Tasks;
using NSubstitute.Extensions;
using Photon.Pun;
using Photon.Realtime;
using SunTemple;
using TMPro;
using Unity.Services.RemoteConfig;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.Video;
using Unity.RemoteConfig;
using Unity.Services.Authentication;
using Unity.Services.Core;

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
        
        [SerializeField]
        [InspectorName("Top coin tesxts")]
        private TMP_Text[] topCoinTexts;
        
        public bool ProdBuild => prodBuild;
        
        // Remote config variables
        private bool isInMaintenance = false;
        private int minimumPeopleToPlayHG = 2;
        private int startTimerHG = 20;
        private string zoneSettingsHG = "";
        
        private bool isReconnecting = false;
        
        public struct userAttributes {}
        public struct appAttributes {}

        async Task InitializeRemoteConfigAsync()
        {
	        // initialize handlers for unity game services
	        await UnityServices.InitializeAsync();

	        // remote config requires authentication for managing environment information
	        if (!AuthenticationService.Instance.IsSignedIn)
	        {
		        await AuthenticationService.Instance.SignInAnonymouslyAsync();
	        }
        }

        async void Start()
        {
	        // initialize Unity's authentication and core services, however check for internet connection
	        // in order to fail gracefully without throwing exception if connection does not exist
	        if (Utilities.CheckForInternetConnection())
	        {
		        await InitializeRemoteConfigAsync();
	        }

	        RemoteConfigService.Instance.FetchCompleted += ApplyRemoteSettings;
	        RemoteConfigService.Instance.FetchConfigs(new userAttributes(), new appAttributes());
        }

        void ApplyRemoteSettings(ConfigResponse configResponse)
        {
	        // check if remote config has been fetched successfully
	        if (configResponse.status == ConfigRequestStatus.Success)
	        {
		        // get the value of the key "isInMaintenance" as a bool
		        isInMaintenance = !RemoteConfigService.Instance.appConfig.GetBool("online");
		        minimumPeopleToPlayHG = RemoteConfigService.Instance.appConfig.GetInt("minimumPeopleToPlayHG");
		        startTimerHG = RemoteConfigService.Instance.appConfig.GetInt("startTimerHG");
		        zoneSettingsHG = RemoteConfigService.Instance.appConfig.GetJson("zoneSettingsHG");
	        }
	        else
	        {
		        // if remote config fetch failed, use default value
		        isInMaintenance = true;

		        if (player != null)
		        {
			        Destroy(player);
		        }
	        }
	        
	        Debug.Log("En mantenimiento: " + isInMaintenance);
	        Debug.Log("Minimum people to play HG: " + minimumPeopleToPlayHG);
	        Debug.Log("Start timer HG: " + startTimerHG);
	        Debug.Log("Zone settings HG: " + zoneSettingsHG);

	        if (isInMaintenance)
	        {
		        MenuManager.Instance.OpenMenu(Menu.MANTENANCE);
		        
		        videoPlayer.gameObject.transform.parent.gameObject.SetActive(false);
	        }
	        
	        //If we are in the hunger games room we configure it
	        if (SceneManager.GetActiveScene().name == Rooms.HUNGER_GAMES.ToString())
	        {
		        ZoneSettingsRC settings = JsonUtility.FromJson<ZoneSettingsRC>(zoneSettingsHG);
		        
		        GameObject.FindObjectOfType<HungerGamesZone>().ConfigureZone(settings);
		        
		        FindObjectOfType<HungerGamesRoomConfiguration>().Configure(startTimerHG, minimumPeopleToPlayHG);
	        }
        }

        public void ConfigureTopCoins()
        {
	        if (topCoinTexts.Length > 0)
	        {
		        for (int i = 0; i < player.TopContacts.Length; i++)
		        {
			        for (int j = 0; j < topCoinTexts.Length; j++)
			        {
				        topCoinTexts[j].text += player.TopContacts[i].name + ": " + player.TopContacts[i].coins + "\n";
			        }
		        }
	        }
	        else
	        {
		        for (int j = 0; j < topCoinTexts.Length; j++)
		        {
			        topCoinTexts[j].text = "There are no contacts";
			    }
	        }
        }

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

	        if (SceneManager.GetActiveScene() != SceneManager.GetSceneByName(mainRoom.ToString()))
	        {
		        videoPlayer.transform.parent.gameObject.SetActive(false);
	        }
	        
	        //PhotonNetwork.AutomaticallySyncScene = true;
	        
	        //DontDestroyOnLoad(this.gameObject);

	        if (prodBuild)
	        {
		        //TODO quitan comentario
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
	        currentRoom = (Rooms) Enum.Parse(typeof(Rooms), room);
	        
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
	        if (player == null && !isInMaintenance)
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
			        if (this.player == null)
			        {
				        this.player = player.GetComponentInChildren<Player>();
			        }
			        
			        EnableChat();
			        this.player.EnableMainCanvas();
			        //this.player.Resume();
			        
			        videoPlayer.transform.parent.gameObject.SetActive(false);
			        loadingCanvas.SetActive(false);
			        opacarImagen.SetActive(false);
		        }
		        else
		        {
			        videoPlayer.transform.parent.gameObject.SetActive(true);
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
		        if (isInMaintenance)
		        {
			        videoPlayer.gameObject.transform.parent.gameObject.SetActive(false);
		        }
		        else
		        {
			        Debug.Log("Connected to room but player already exists");
		        }
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
	        
	        if (isReconnecting)
	        {
		        isReconnecting = false;
		        
		        SceneManager.LoadScene(mainRoom.ToString());
	        }
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
			isReconnecting = true;
			
	        // We try to connect and wait 5 seconds to check if we are connected
	        PhotonNetwork.ConnectUsingSettings();     
	        
	        MenuManager.Instance.CloseMenu();
	        
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

		public void LoadOfflineMinigame()
		{
			SceneManager.LoadScene("Minecraft");
		}
    }