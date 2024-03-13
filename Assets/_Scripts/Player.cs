using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using KinematicCharacterController;
using KinematicCharacterController.Examples;
using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Player : Entity
{
    private string id = "";
    
    public string ID => id;
    
    public ExampleCharacterController Character;
    
    public ExampleCharacterCamera CharacterCamera;

    private const string MouseXInput = "Mouse X";
    private const string MouseYInput = "Mouse Y";
    private const string MouseScrollInput = "Mouse ScrollWheel";
    private const string HorizontalInput = "Horizontal";
    private const string VerticalInput = "Vertical";
    
    [SerializeField]
    private Animator animator;
    
    [Tooltip("The name our player will have in the inspectior to identify it")]
    [InspectorName("Player Name")]
    [SerializeField]
    private string playerName = "Our Player";
    
    [InspectorName("Max Reach Distance")]
    [SerializeField]
    private int maxReachDistance = 5;
    
    [InspectorName("Smooth Blend Transition")]
    [SerializeField]
    private float smoothBlendTransition = 0.3f;

    [InspectorName("Call Canvas")] 
    [SerializeField]
    private GameObject receiveCallCanvas;
    
    [InspectorName("Chat menu")]
    [SerializeField]
    private ChatMenu chatMenu;
    
    [InspectorName("OnGoingCall Canvas")] 
    [SerializeField]
    private GameObject onGoingCanvas;
    
    [InspectorName("Calling Canvas")]
    [SerializeField]
    private GameObject callingCanvas;
    
    [InspectorName("VideoCall Canvas")]
    [SerializeField]
    private GameObject videoCallCanvas;

    [InspectorName("Video call camera")]
    [SerializeField]
    private Camera videoCallCamera;
    
    [InspectorName("Inventory")]
    [SerializeField]
    private Inventory inventory;
    
    [InspectorName("Max Void Hieght")]
    [SerializeField]
    private int maxVoidHeight = -50;
    
    [Tooltip("The time the player has to press the button to send a contact request (in seconds)")]
    [InspectorName("Request Time")]
    [SerializeField]
    private int requestTime = 5;
    
    [InspectorName("Over Canvas")]
    [SerializeField]
    private OverCanvas overCanvas;
    
    [SerializeField]
    [InspectorName("Minimap camera")]
    private Camera minimapCamera;
    
    [SerializeField]
    [InspectorName("Main canvas")]
    private GameObject mainCanvas;
    
    [SerializeField]
    [InspectorName("Coins text")]
    private TMP_Text coinsText;
    
    [SerializeField]
    [InspectorName("Hit crosshair")]
    private Image hitCrosshair;
    
    [SerializeField]
    [InspectorName("PostProcessVolume")]
    private PostProcessVolume postProcessVolume;
    
    private Vignette vignette;
    
    private float vignetIntensity = 0;

    private float originalMovementSpeed = 0;
    
    private bool canMove = false;

    private ApiClient api;

    private UserData userData;
    
    public string StreamKey => userData.StreamKey;
    
    public TopContactResponse[] TopContacts => userData.TopContacts;
    
    public Contact[] Contacts => userData.Contacts;
    
    private bool isRequestingContact = false;
    
    private int startedRequestTime = 0;
    
    private GameObject contactRequest;
    
    private bool isVignetteChanging = false;
    
    [SerializeField]
    [InspectorName("Contact Time Out")]
    [Tooltip("The time for the player to wait for a contact response (in seconds) Once the time is over the request will be canceled")]
    private int contactTimeOut = 20;
    
    [SerializeField]
    [InspectorName("Recoil script")]
    private Recoil recoilScript;
    
    [SerializeField]
    [InspectorName("Ragdoll")]
    private Ragdoll ragdoll;

    [SerializeField]
    [InspectorName("Sprint FOV")]
    private float sprintFOV = 60;
    
    private float originalFOV;
    
    [SerializeField]
    [InspectorName("Footstep Sound")]
    private AudioClip footStepSound;
    
    [SerializeField]
    [InspectorName("Footstep Delay")]
    [Tooltip("The time between each footstep sound")]
    private float footStepDelay;
 
    private float nextFootstep = 0;
    
    private bool left = true;
    
    public Recoil RecoilScript => recoilScript;

    private void OnEnable()
    {
        originalMovementSpeed = Character.MaxStableMoveSpeed;
        
        originalFOV = CharacterCamera.Camera.GetComponent<Camera>().fieldOfView;
    }

    private void Start()
    {
        // The request time is updated because the fixed update executes 50 times per second
        requestTime *= 50;
        
        startedRequestTime = requestTime;
        
        Cursor.lockState = CursorLockMode.Locked;

        // Tell camera to follow transform
        CharacterCamera.SetFollowTransform(Character.CameraFollowPoint);

        // Ignore the character's collider(s) for camera obstruction checks
        CharacterCamera.IgnoredColliders.Clear();
        CharacterCamera.IgnoredColliders.AddRange(Character.GetComponentsInChildren<Collider>());
        
        videoCallCamera.enabled = false;

        if (!PV.IsMine){
            CharacterCamera.Camera.GetComponent<Camera>().enabled = false;
            
            minimapCamera.enabled = false;

            // Listener
            CharacterCamera.Camera.gameObject.GetComponent<AudioListener>().enabled = false;
            
            // Movement and pyhsics
            Character.enabled = false;
            CharacterCamera.enabled = false;
            
            postProcessVolume.enabled = false;
            
            inventory.Crosshair.gameObject.SetActive(false);
            
            // Whe desactivate the sound source of the player child 
            //GetComponentInChildren<AudioSource>().enabled = false;
        }
        else
        {
            api = GameManager.Instance.ApiClient;

            GetUserInfo();

            // We change all the layers to Player
            gameObject.layer = 3;

            videoCallCamera.Reset();
            
            Character.MeshRoot.transform.GetChild(0).transform.GetChild(0).gameObject.layer = 3;

            // We register the menus
            MenuManager.Instance.SetCrossHair(inventory.Crosshair.gameObject);
            
            MenuManager.Instance.RegisterMenu(receiveCallCanvas, Menu.RECEIVE_CALL, receiveCallCanvas.GetComponent<ReceiveCallMenu>());
            MenuManager.Instance.RegisterMenu(onGoingCanvas, Menu.CALL, onGoingCanvas.GetComponent<OnGoingCallMenu>());
            MenuManager.Instance.RegisterMenu(callingCanvas, Menu.CALLING, callingCanvas.GetComponent<OnGoingCallMenu>());
            MenuManager.Instance.RegisterMenu(videoCallCanvas, Menu.VIDEO_CALL, videoCallCanvas.GetComponent<OnGoingCallMenu>());
            MenuManager.Instance.RegisterMenu(inventory.gameObject, Menu.INVENTORY, inventory);
            MenuManager.Instance.RegisterMenu(chatMenu.gameObject, Menu.CHAT_MENU, chatMenu);
            
            // We change the name of the player
            gameObject.name = playerName;
            
            postProcessVolume.profile.TryGetSettings<Vignette>(out vignette);
            
            vignette.enabled.Override(false);
        }
    }
    
    [PunRPC]
    public void EnableRagdoll()
    {
        ragdoll.EnableRagdoll();
        
        inventory.StartEditing();
    }
    
    public void EnableRagdollRPC()
    {
        PV.RPC("EnableRagdoll", RpcTarget.All);
    }
    
    IEnumerator TakeDamageEffect()
    {
        isVignetteChanging = true;
        vignette.enabled.Override(true);

        float startTime = Time.time;
        float duration = 1f;

        while (Time.time - startTime < duration)
        {
            float t = (Time.time - startTime) / duration;
            vignette.intensity.Override(Mathf.Lerp(0.4f, 0f, t));
            yield return null;
        }

        vignette.intensity.Override(0f);
        vignette.enabled.Override(false);
        isVignetteChanging = false;
    }


    public void RefreshContacts()
    {
        if (userData == null || userData.Contacts == null) return;
        
        Contact[] contactsAux = userData.Contacts;
        
        //Clean all the PVs
        foreach (Contact contact in contactsAux)
        {
            contact.SetPV(null);
        }
        
        Player[] players = GameObject.FindObjectsOfType<Player>();

        for (int i = 0; i < contactsAux.Length; i++)
        {
            foreach (Player player in players)
            {
                if (player.ID == contactsAux[i].ID)
                {
                    contactsAux[i].SetPV(player.PV);
                }
            }
        }
    }
    
    [Button]
    private async void GetUserInfo()
    {
        string uniqueIdentifier = SystemInfo.deviceUniqueIdentifier;
        
        if (Application.isEditor)
        {
            uniqueIdentifier = "1";
        }

        if (GameManager.Instance != null && !GameManager.Instance.ProdBuild)
        {
            if (Debug.isDebugBuild)
            {
                uniqueIdentifier = "2";
            }
        }
        
        /*
        Contact contact = new Contact("Rodrigo", "2");

        string json = "{";
        json +=  "\"contact\":" + contact.ToJson() + ",";
        json +=  "\"user\": \"" + uniqueIdentifier + "\"";
        json += "}";

        api.Post("contact", json);
        */

        if (PV != null)
        {
            PV.RPC("UpdateIDRPC", RpcTarget.AllBuffered, uniqueIdentifier);
        }
        
        SocialManager.Instance.StartDMSuscription(uniqueIdentifier);

        string response = "";

        //This is only when calling this function from the editor
        if (api == null)
        {
            api = new ApiClient("http://localhost:3000/api/");
        }
        
        response = await api.Post("user", uniqueIdentifier);

        userData = JsonUtility.FromJson<UserDataResponse>(response).ToUserData(api,id);
           
        //In case the user has no items we add the item Basic Sword
        if (userData.Items == null || userData.Items.Length == 0)
        {
            //Debug.Log("No items" + response);
            inventory.AddItem(Items.GLOCK);
        }
        else
        {
            //We add the items to the inventory
            foreach (SerializableItemData item in userData.Items)
            {
                inventory.AddItem(item);
            }
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ConfigureTopCoins();
        }

        if (Application.isPlaying)
        {
            inventory.Initialize();
            
            ShowFormattedCoins();
        
        
            if (userData == null)
            {
                Debug.LogError("User data is null");
            }
            /*#region Just for development
            uniqueIdentifier = Random.Range(0, 1000000).ToString();
            // We find the other player
            Player other = GameObject.Find("Player(Clone)").GetComponent<Player>();
            
            if (other != null)
            {
                // We add to contacts the other player
                userData.AddToContact(new Contact(other.PV, "Frederico", other.ID));
            }
        #endregion*/

            RefreshContacts();
        
            // We save the data every 5 min
            InvokeRepeating("SaveObjectsData", 1, 300);
        
            // We save the coins every 5 min
            InvokeRepeating("SaveCoins", 300, 300);
        }
    }

    private void Update()
    {
        if (!PV.IsMine) return;

        //DevAddObject();

        if (canMove)
        {
            Rooms name = GameManager.Instance.CurrentRoom;
            switch (name)
            {
                case Rooms.LOBBY:
                    HandleCharacterInput();
                    break;
                case Rooms.HUNGER_GAMES:
                    HungerGamesRoomConfiguration hungerGamesRoomConfiguration = (HungerGamesRoomConfiguration) RoomConfiguration.Instance;
                    if (!hungerGamesRoomConfiguration.IsSpectating && hungerGamesRoomConfiguration.IsPlaying)
                    {
                        HandleCharacterInput();
                    }
                    break;
                default:
                    HandleCharacterInput();
                    break;
            }
        }
        
        HandleCallCanvas();
        
        HandleVoidTP();

        HandleMouseFocus();
        
        HandleRaycast();
        
        HandleInventory();
        
        Chat();
        
        DevPopUp();

        if (RoomConfiguration.Instance.CanPlayerUseItems)
        {
            HandleItemUse();
        }

        //DevCall();

        //DevRefreshContacts();
    }

    private void DevPopUp()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            MenuManager.Instance.PopUp("This is a test");
        }
    }

    public void ActivateScareMode()
    {
        //We activate Chromatic Aberration, Grain, Ambient Occlusion and Motion Blur
        postProcessVolume.profile.TryGetSettings<ChromaticAberration>(out ChromaticAberration chromaticAberration);
        postProcessVolume.profile.TryGetSettings<Grain>(out Grain grain);
        postProcessVolume.profile.TryGetSettings<AmbientOcclusion>(out AmbientOcclusion ambientOcclusion);
        postProcessVolume.profile.TryGetSettings<MotionBlur>(out MotionBlur motionBlur);
        
        chromaticAberration.enabled.Override(true);
        grain.enabled.Override(true);
        ambientOcclusion.enabled.Override(true);
        motionBlur.enabled.Override(true);
    }

    private void FixedUpdate()
    {
        if (!PV.IsMine || !canMove) return;
        
        SprintCameraMovement(Character.IsSprinting);
        
        //Check for life
        if (Health <= 0)
        {
            Die();
        }

        if (isRequestingContact)
        {
            if (requestTime >= 0)
            {
                requestTime--;
            }

            if (requestTime == 0)
            {
                if (userData == null)
                {
                    Debug.LogError("User data is null");
                }
                
                if (!SocialManager.Instance.IsReceivingContactRequest)
                {
                    SocialManager.Instance.ContactRequest(contactRequest.GetComponentInParent<Player>());
                    
                    StartCoroutine(WaitForContactResponse());
                }
                else
                {
                    //If we are receiving a contact request we check if the request is from the same player
                    if (contactRequest.GetComponentInParent<Player>().ID == SocialManager.Instance.ContactToAdd.ID)
                    {
                        SocialManager.Instance.AcceptContactRequest();
                    }
                    else
                    {
                        MenuManager.Instance.PopUp("You are already receiving a contact request from a different player");
                    }
                }
            }
        }
    }

    private void HandleItemUse()
    {
        if (Input.GetMouseButton(0) && MenuManager.Instance.CanShoot())
        {
            if (inventory.CurrentItem != null)
            {
                if (BuildingSystem.Instance != null && BuildingSystem.Instance.CanBuild)
                {
                    return;
                }
                inventory.CurrentItem.Use();
                inventory.UpdateItemText();
            }
        }
    }
    
    public void StartEditing()
    {
        inventory.StartEditing();
    }
    
    public void RemoveItem(Item item)
    {
        inventory.RemoveItem(item);
    }

    public override bool TakeDamage(int damage, PhotonView playerToKill =  null)
    {
        bool x = base.TakeDamage(damage, playerToKill);

        if (playerToKill != null)
        {
            PV.RPC("AnimateHit", playerToKill.Controller);
        }
        
        return x;
    }

    public void Stop() 
    {
        canMove = false;
        
        Character.MaxStableMoveSpeed = 0;
    }
    
    public void Resume()
    {
        canMove = true;
        
        Character.MaxStableMoveSpeed = originalMovementSpeed;
    }

    private void Over(string letra, string mensaje)
    {
        overCanvas.gameObject.SetActive(true);
        overCanvas.Configure(letra, mensaje);
    }

    #region Controller Input Handling
    
    private void LateUpdate()
    {
        if (!PV.IsMine) return;
        
        // Handle rotating the camera along with physics movers
        if (CharacterCamera.RotateWithPhysicsMover && Character.Motor.AttachedRigidbody != null)
        {
            CharacterCamera.PlanarDirection = Character.Motor.AttachedRigidbody.GetComponent<PhysicsMover>().RotationDeltaFromInterpolation * CharacterCamera.PlanarDirection;
            CharacterCamera.PlanarDirection = Vector3.ProjectOnPlane(CharacterCamera.PlanarDirection, Character.Motor.CharacterUp).normalized;
        }

        HandleCameraInput();
    }

    private void HandleCameraInput()
    { 
        // Create the look input vector for the camera
        float mouseLookAxisUp = Input.GetAxisRaw(MouseYInput);
        float mouseLookAxisRight = Input.GetAxisRaw(MouseXInput);
        Vector3 lookInputVector = new Vector3(mouseLookAxisRight, mouseLookAxisUp, 0f);

        // Prevent moving the camera while the cursor isn't locked
        if (Cursor.lockState != CursorLockMode.Locked)
        {
            lookInputVector = Vector3.zero;
        }
        
        // We make the player object rotate to camera forward
        Character.MeshRoot.transform.rotation = Quaternion.Euler(0, CharacterCamera.Transform.rotation.eulerAngles.y, 0);

        // Input for zooming the camera (disabled in WebGL because it can cause problems)
        float scrollInput = -Input.GetAxis(MouseScrollInput);
        #if UNITY_WEBGL
                scrollInput = 0f;
        #endif

        // Apply inputs to the camera
        CharacterCamera.UpdateWithInput(Time.deltaTime, scrollInput, lookInputVector, Character.IsCrouching);

        // Handle toggling zoom level
        if (Input.GetMouseButtonDown(1))
        {
            CharacterCamera.TargetDistance = (CharacterCamera.TargetDistance == 0f) ? CharacterCamera.DefaultDistance : 0f;
        }
    }

    private void HandleCharacterInput()
    {
        PlayerCharacterInputs characterInputs = new PlayerCharacterInputs();

        // Build the CharacterInputs struct
        characterInputs.MoveAxisForward = Input.GetAxisRaw(VerticalInput);
        characterInputs.MoveAxisRight = Input.GetAxisRaw(HorizontalInput);
        characterInputs.CameraRotation = CharacterCamera.Transform.rotation;
        characterInputs.JumpDown = Input.GetKeyDown(KeyCode.Space);
        characterInputs.CrouchDown = Input.GetKeyDown(KeyCode.C);
        characterInputs.CrouchUp = Input.GetKeyUp(KeyCode.C);
        characterInputs.Sprint = Input.GetKey(KeyCode.LeftShift);

        // Apply inputs to character
        Character.SetInputs(ref characterInputs);
        
        // Animation
        animator.SetFloat("x", characterInputs.MoveAxisForward, smoothBlendTransition, Time.deltaTime);
        animator.SetFloat("y", characterInputs.MoveAxisRight, smoothBlendTransition, Time.deltaTime);
        
        // Footsteps normal and sprint
        // We check for movement to play the footstep sound
        if (characterInputs.MoveAxisForward != 0 || characterInputs.MoveAxisRight != 0)
        {
            if (characterInputs.Sprint)
            {
                if (Time.time > (nextFootstep - 0.15f))
                {
                    nextFootstep = Time.time + footStepDelay;
                
                    AudioManager.Instance.PlaySound(footStepSound, CharacterCamera.Camera.transform.position);
                }
            }
            else
            {
                if (Time.time > nextFootstep)
                {
                    nextFootstep = Time.time + footStepDelay;
                
                    AudioManager.Instance.PlaySound(footStepSound, CharacterCamera.Camera.transform.position);
                }
            }
        }
    }
    
    #endregion

    IEnumerator WaitForContactResponse()
    {
        yield return new WaitForSecondsRealtime(contactTimeOut);
        
        if (isRequestingContact && SocialManager.Instance.IsReceivingContactRequest && SocialManager.Instance.ContactToAdd == null)
        {
            isRequestingContact = false;
            requestTime = startedRequestTime;
            
            contactRequest = null;

            SocialManager.Instance.ClearContactRequest();
            
            MenuManager.Instance.PopUp("Contact request time out");
        }
    }

    private void HandleRaycast()
    {
        RaycastHit hit;

        if (Physics.Raycast(CharacterCamera.Transform.position, CharacterCamera.Transform.forward, out hit, maxReachDistance))
        {
            if (hit.collider.gameObject.CompareTag("Player") && hit.collider.gameObject != this.gameObject && hit.collider.gameObject != Character.gameObject)
            {
                //We check if the player is already in our contacts
                bool isInContacts = false;

                if (userData.Contacts != null)
                {
                    foreach (Contact contact in userData.Contacts)
                    {
                        if (contact.ID == hit.collider.gameObject.GetComponentInParent<Player>().ID)
                        {
                            isInContacts = true;
                        }
                    }
                }

                if (!isInContacts)
                {
                    Over("F", "Hold to add person as a contact");
                }
                
                if (Input.GetKey(KeyCode.F) && !isInContacts)
                {
                    isRequestingContact = true;
                    
                    contactRequest = hit.collider.gameObject;
                }
                else
                {
                    if (requestTime > 0)
                    {
                        isRequestingContact = false;
                        requestTime = startedRequestTime;
                    
                        contactRequest = null;
                    }
                }
            }
            else if (hit.collider.gameObject.CompareTag("Chess"))
            {
                Over("Q", "Press to open chess");
                
                if (Input.GetKeyDown(KeyCode.Q))
                {
                    hit.collider.gameObject.GetComponent<Chess>().Open();
                }
            }
            else
            {
                overCanvas.gameObject.SetActive(false);
                
                if (requestTime > 0)
                {
                    isRequestingContact = false;
                    requestTime = startedRequestTime;
                    
                    contactRequest = null;
                }
            }
        }
        else
        {
            overCanvas.gameObject.SetActive(false);
        }
    }
    
    public void EnableMainCanvas()
    {
        mainCanvas.SetActive(true);
        inventory.UpdateItemText();
    }

    private void HandleInventory()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (!inventory.IsOpen)
            {
                MenuManager.Instance.OpenMenu(Menu.INVENTORY);
            }
            else
            {
                MenuManager.Instance.CloseMenu();
            }
        }
    }

    private void HandleMouseFocus()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        
        if (Input.GetMouseButtonDown(0) && Cursor.lockState == CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void HandleVoidTP()
    {
        if (Character.transform.position.y < maxVoidHeight)
        {
            GameManager.Instance.TPToSpawn(this);
        }
    }
    
    private void DevCall()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            SocialManager.Instance.Call(userData.Contacts[0], true);
        }
    }

    private void DevRefreshContacts()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            RefreshContacts();
        }
    }
    
    private void DevAddObject()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            inventory.AddItem(Items.GLOCK);
            
            SaveObjectsData();
        }
    }

    private void HandleCallCanvas()
    {
        if (SocialManager.Instance.IsBeingCalled)
        {
            if (Input.GetKeyDown(KeyCode.Y))
            {
                SocialManager.Instance.AcceptCall();
            }
            else if (Input.GetKeyDown(KeyCode.N))
            {
                MenuManager.Instance.CloseMenu();
                SocialManager.Instance.Clean();
            }
        }

        if (SocialManager.Instance.IsOnCall)
        {
            if (Input.GetKeyDown(KeyCode.L))
            {
                SocialManager.Instance.LeaveCall();
            }
        }
    }
    
    private void SprintCameraMovement(bool sprint)
    {
        //If sprinting we move the camera simulating the effect of the player running. We make it smooth by using DOTween and we move it side to side
        if (sprint)
        {
            //We check the side and we move the camera to the opposite side
            if (left)
            {
                CharacterCamera.Camera.transform.DOLocalMoveX(.05f, .35f);
            }
            else
            {
                CharacterCamera.Camera.transform.DOLocalMoveX(-.05f, .35f);
            }
            
            if (CharacterCamera.Camera.transform.localPosition.x >= 0.025f)
            {
                left = false;
            }
            else if (CharacterCamera.Camera.transform.localPosition.x <= -0.025f)
            {
                left = true;
            }
        }
        else
        {
            CharacterCamera.Camera.transform.DOLocalMoveX(0, .15f);
        }
    }
    
    public void SprintFOV(bool sprint)
    {
        //We make it smooth by using DOTween
        if (sprint)
        {
            CharacterCamera.Camera.DOFieldOfView(sprintFOV, 0.5f);
        }
        else
        {
            CharacterCamera.Camera.DOFieldOfView(originalFOV, 0.5f);
        }
    }

    private void Chat()
    {
        if (Input.GetKeyDown(KeyCode.T) && SocialManager.Instance.ContactToAdd == null && !SocialManager.Instance.IsReceivingContactRequest && !MenuManager.Instance.IsOpen(Menu.CHAT_MENU))
        {
            ChatManager.Instance.FocusChat();
        }
    }
    
    #region RPCs
    [PunRPC]
    void CallRPC(string callerID, string channelName, bool videoCall)
    {
        SocialManager.Instance.ReceiveCall(callerID, channelName, videoCall);
    }
    
    [PunRPC]
    public void AnimateHit()
    {
        if (isVignetteChanging)
        {
            StopCoroutine("TakeDamageEffect");
            vignette.intensity.value = 0.4f;
            isVignetteChanging = false;
        }

        StartCoroutine("TakeDamageEffect");
    }
    
    [PunRPC]
    void AcceptCallRPC()
    {
        SocialManager.Instance.JoinCall();
    }
    
    [PunRPC]
    void LeaveCallRPC()
    {
        SocialManager.Instance.EndCall();
    }
    
    [PunRPC]
    void UpdateIDRPC(string id)
    {
        this.id = id;
        
        // We change the name of the player
        this.gameObject.name =  id;
    }
    
    [PunRPC]
    void ReciveContactRequestRPC(string contactID)
    {
        SocialManager.Instance.ReceiveContactRequest(contactID);
    }
    
    [PunRPC]
    void AcceptContactRequestRPC()
    {
        MenuManager.Instance.OpenMenu(Menu.CONTACT_REQUEST);
        
        isRequestingContact = false;
        
        SocialManager.Instance.IsReceivingContactRequest = false;
    }
    
    [PunRPC]
    void GoToLobbyRPC()
    {
        Lobby();
    }
    #endregion

    public void AddContact(Contact contact)
    {
        if (userData == null)
        {
            Debug.LogError("User data is null");
        }

        userData.AddToContact(contact);

        MenuManager.Instance.PopUp("Contact added succesfully");
        
        requestTime = startedRequestTime;
    }
    
    public void EnableVideo()
    {
        videoCallCamera.enabled = true;
    }
    
    public void DisableVideo()
    {
        videoCallCamera.enabled = false;
    }
    
    public override void Die(bool restore = true)
    {
        if (!PV.IsMine) return;
        
        //TODO
        if (restore)
        {
            //RestorePlayer();
        }
        
        SaveObjectsData();
        
        SaveCoins();
        
        EnableRagdollRPC();
        
        Invoke(nameof(Lobby), 5.0f);
    }

    private void Lobby()
    {
        SceneManager.LoadScene(Rooms.LOBBY.ToString());
    }

    public void WinHungerGames()
    {
        //We add 1000 coins then we show a popup satisfying the user and then we wait 5 seconds to go to the lobby
        AddCoins(10000);
        
        MenuManager.Instance.PopUp("Congratulations you won the Hunger Games!!!");
        
        PV.RPC("GoToLobbyRPC", RpcTarget.Others);
        
        SaveCoins();
        SaveObjectsData();
        
        Invoke(nameof(GoToLobby), 10.0f);
    }
    
    public void GoToLobby()
    {
        Die(false);
    }

    private void RestorePlayer()
    {
        api.Post("user/restore", id);
        
        userData.Reset();
    }
    
    //Every 5 min we save the objects of the player
    [Button]
    private void SaveObjectsData()
    {
        SerializableItemData[] itemsAux = inventory.GetItems();
        
        if (itemsAux != null)
        {
            string itemsJson = "[";
        
            if (itemsAux.Length > 0)
            {
                foreach (SerializableItemData item in itemsAux)
                {
                    itemsJson += JsonUtility.ToJson(item) + ",";
                }
            
                itemsJson = itemsJson.Remove(itemsJson.Length - 1);
            }
        
            itemsJson += "]";
        
            string json = "{";
            json +=  "\"data\":" + itemsJson + ",";
            json +=  "\"user\": \"" + id + "\"";
            json += "}";
        
            api.Post("objects/update", json);
        }
    }
    
    //Every 5 min we save the coins of the player
    [Button]
    private void SaveCoins()
    {
        string json = "{";
        json +=  "\"data\":" + userData.Coins + ",";
        json +=  "\"user\": \"" + id + "\"";
        json += "}";
        
        api.Post("coins/update", json);
    }
    
    private void OnApplicationQuit()
    {
        SaveObjectsData();
        SaveCoins();
    }
    
    public void AddItem(Item item)
    {
        inventory.AddItem(item);
    }
    
    public void AddCoins(int coins)
    {
        userData.AddCoins(coins);
        
        ShowFormattedCoins();
    }
    
    private void ShowFormattedCoins()
    {
        int coins = userData.Coins;

        string aux = "";
        
        if (coins < 1000)
        {
            aux = coins.ToString();
        }
        else if (coins < 1000000)
        {
            float kValue = coins / 1000f;
            aux = kValue.ToString("F2") + " K";
        }
        else if (coins < 1000000000)
        {
            float mValue = coins / 1000000f;
            aux = mValue.ToString("F2") + " M";
        }
        else
        {
            float bValue = coins / 1000000000f;
            aux = bValue.ToString("F2") + " B";
        }
        
        //We substitute the , for .
        aux = aux.Replace(',', '.');
        
        coinsText.text = aux;
    }
    
    public void RemoveCoins(int coins)
    {
        userData.RemoveCoins(coins);
        
        ShowFormattedCoins();
    }

    public void HitCrosshair()
    {
        //Animation using DOTween
        hitCrosshair.DOFade(1, 0.1f).OnComplete(() => hitCrosshair.DOFade(0, 0.1f));
        
        //TODO: Sound of hit player
    }

    public void UpdateAmmoUI()
    {
        inventory.UpdateItemText();
    }
    
    public void IncreaseSprintSpeed()
    {
        Character.SprintSpeed = Character.SprintSpeed * 2;
    }
}
