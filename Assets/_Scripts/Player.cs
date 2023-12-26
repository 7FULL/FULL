using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using KinematicCharacterController;
using KinematicCharacterController.Examples;
using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using UnityEngine;
using Random = UnityEngine.Random;

public class Player : Entity
{
    private string id = "";
    
    public string ID => id;
    
    [SerializeField]
    private ExampleCharacterController Character;
    
    [SerializeField]
    private ExampleCharacterCamera CharacterCamera;

    private const string MouseXInput = "Mouse X";
    private const string MouseYInput = "Mouse Y";
    private const string MouseScrollInput = "Mouse ScrollWheel";
    private const string HorizontalInput = "Horizontal";
    private const string VerticalInput = "Vertical";
    
    [SerializeField]
    private Animator animator;

    private PhotonView pv;

    public PhotonView PV => pv;
    
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
    
    private bool canMove = true;

    private ApiClient api;

    private UserData userData;
    
    public Contact[] Contacts => userData.Contacts;
    
    private bool isRequestingContact = false;
    
    private int startedRequestTime = 0;
    
    private GameObject contactRequest;
    
    private int contactTimeOut = 20;

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
        
        pv = GetComponent<PhotonView>();
        
        videoCallCamera.enabled = false;

        if (!pv.IsMine){
            CharacterCamera.gameObject.GetComponent<Camera>().enabled = false;

            // Listener
            CharacterCamera.gameObject.GetComponent<AudioListener>().enabled = false;
            
            // Movement and pyhsics
            Character.enabled = false;
            CharacterCamera.enabled = false;
            
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
            MenuManager.Instance.RegisterMenu(receiveCallCanvas, Menu.RECEIVE_CALL, receiveCallCanvas.GetComponent<ReceiveCallMenu>());
            MenuManager.Instance.RegisterMenu(onGoingCanvas, Menu.CALL, onGoingCanvas.GetComponent<OnGoingCallMenu>());
            MenuManager.Instance.RegisterMenu(callingCanvas, Menu.CALLING, callingCanvas.GetComponent<OnGoingCallMenu>());
            MenuManager.Instance.RegisterMenu(videoCallCanvas, Menu.VIDEO_CALL, videoCallCanvas.GetComponent<OnGoingCallMenu>());
            MenuManager.Instance.RegisterMenu(inventory.gameObject, Menu.INVENTORY, inventory);

            // We change the name of the player
            gameObject.name = playerName;
        }
    }
    
    public void RefreshContacts()
    {
        if (userData.Contacts == null) return;
        
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
    

    private async void GetUserInfo()
    {
        string uniqueIdentifier = SystemInfo.deviceUniqueIdentifier;
        
        // If a development build
        if (Debug.isDebugBuild)
        {
            uniqueIdentifier = "2";
        }
        if (Application.isEditor)
        {
            uniqueIdentifier = "1";
        }
        
        /*
        Contact contact = new Contact("Rodrigo", "2");

        string json = "{";
        json +=  "\"contact\":" + contact.ToJson() + ",";
        json +=  "\"user\": \"" + uniqueIdentifier + "\"";
        json += "}";

        api.Post("contact", json);
        */
        
        pv.RPC("UpdateIDRPC", RpcTarget.AllBuffered, uniqueIdentifier);

        string response = "";
        
        try
        {
            response = await api.Post("user", uniqueIdentifier);

            userData = JsonUtility.FromJson<UserDataResponse>(response).ToUserData(api,id);
            
            //In case the user has no items we add the item Basic Sword
            if (userData.Items == null || userData.Items.Length == 0)
            {
                Debug.Log("No items" + response);
                //inventory.AddItem(Items.BASIC_SWORD);
            }
            else
            {
                //We add the items to the inventory
                foreach (ItemData item in userData.Items)
                {
                    Debug.Log(item);
                    inventory.AddItem(item);
                }
            }
        }
        catch (Exception e)
        {
            //TODO: Servers down menu
            Debug.LogError(e);
            return;
        }
        
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
        InvokeRepeating("SaveObjectsData", 300, 300);
    }

    private void Update()
    {
        if (!pv.IsMine || !canMove) return;

        DevAddObject();
        
        HandleCharacterInput();
        
        HandleCallCanvas();
        
        HandleVoidTP();

        HandleMouseFocus();
        
        HandleRaycast();
        
        HandleInventory();
        
        //DevCall();
        
        //DevRefreshContacts();
        
        //DevAddObject();
    }

    private void FixedUpdate()
    {
        if (!pv.IsMine || !canMove) return;

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
                    Debug.Log("Already receiving a contact request");
                    SocialManager.Instance.AcceptContactRequest();
                }
            }
        }
    }

    public void Stop()
    {
        canMove = false;
    }
    
    public void Resume()
    {
        canMove = true;
    }

    private void Over(string letra, string mensaje)
    {
        overCanvas.Configure(letra, mensaje);
        overCanvas.gameObject.SetActive(true);
    }

    #region Controller Input Handling
    
    private void LateUpdate()
    {
        if (!pv.IsMine) return;
        
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

        // Apply inputs to character
        Character.SetInputs(ref characterInputs);
        
        // Animation
        animator.SetFloat("x", characterInputs.MoveAxisForward, smoothBlendTransition, Time.deltaTime);
        animator.SetFloat("y", characterInputs.MoveAxisRight, smoothBlendTransition, Time.deltaTime);
    }
    
    #endregion

    IEnumerator WaitForContactResponse()
    {
        yield return new WaitForSecondsRealtime(contactTimeOut);
        
        if (isRequestingContact)
        {
            isRequestingContact = false;
            requestTime = startedRequestTime;
            
            contactRequest = null;

            SocialManager.Instance.ClearContactRequest();
        }
    }

    private void HandleRaycast()
    {
        RaycastHit hit;

        if (Physics.Raycast(CharacterCamera.Transform.position, CharacterCamera.Transform.forward, out hit, maxReachDistance))
        {
            if (hit.collider.gameObject.CompareTag("Player") && hit.collider.gameObject != this.gameObject)
            {
                Over("F", "Hold to add person as a contact");
                
                if (Input.GetKey(KeyCode.F))
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
            // Switch between cursor modes
            if (Cursor.lockState == CursorLockMode.None)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
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
    
    #region RPCs
    [PunRPC]
    void CallRPC(string callerID, string channelName, bool videoCall)
    {
        SocialManager.Instance.ReceiveCall(callerID, channelName, videoCall);
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
    
    public override void Die()
    {
        RestorePlayer();
        
        GameManager.Instance.JoinRoom(Rooms.LOBBY);
    }

    private void RestorePlayer()
    {
        api.Post("user/restore", id);
        
        userData.Reset();
    }
    
    //Every 5 min we save the objects of the player
    private void SaveObjectsData()
    {
        ItemData[] itemsAux = inventory.GetItems();
        
        if (itemsAux != null)
        {
            string itemsJson = "[";
        
            if (itemsAux.Length > 0)
            {
                foreach (ItemData item in itemsAux)
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
    
    private void OnApplicationQuit()
    {
        SaveObjectsData();
    }
    
    public void AddItem(Items item)
    {
        inventory.AddItem(item);
    }
}