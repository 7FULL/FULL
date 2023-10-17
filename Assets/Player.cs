using System;
using System.Collections;
using System.Collections.Generic;
using KinematicCharacterController;
using KinematicCharacterController.Examples;
using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using UnityEngine;
using Random = UnityEngine.Random;

public class Player : Entity
{
    private int _id = 2;
    
    public int ID => _id;
    
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

    private PhotonView _pv;

    public PhotonView PV => _pv;
    
    [Tooltip("The name our player will have in the inspectior to identify it")]
    [InspectorName("Player Name")]
    [SerializeField]
    private string _playerName = "Our Player";
    
    [InspectorName("Smooth Blend Transition")]
    [SerializeField]
    private float _smoothBlendTransition = 0.3f;

    [InspectorName("Call Canvas")] 
    [SerializeField]
    private GameObject _receiveCallCanvas;
    
    [InspectorName("OnGoingCall Canvas")] 
    [SerializeField]
    private GameObject _onGoingCanvas;
    
    [InspectorName("Calling Canvas")]
    [SerializeField]
    private GameObject _callingCanvas;
    
    [InspectorName("VideoCall Canvas")]
    [SerializeField]
    private GameObject _videoCallCanvas;
    
    [InspectorName("Video call camera")]
    [SerializeField]
    private Camera _videoCallCamera;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        // Tell camera to follow transform
        CharacterCamera.SetFollowTransform(Character.CameraFollowPoint);

        // Ignore the character's collider(s) for camera obstruction checks
        CharacterCamera.IgnoredColliders.Clear();
        CharacterCamera.IgnoredColliders.AddRange(Character.GetComponentsInChildren<Collider>());
        
        _pv = GetComponent<PhotonView>();
        
        _videoCallCamera.enabled = false;

        if (!_pv.IsMine){
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
            // We change all the layers to Player
            gameObject.layer = 3;

            _videoCallCamera.Reset();
            
            Character.MeshRoot.transform.GetChild(0).transform.GetChild(0).gameObject.layer = 3;

            // We register the menus
            MenuManager.Instance.RegisterMenu(_receiveCallCanvas, Menu.RECEIVE_CALL, _receiveCallCanvas.GetComponent<ReceiveCallMenu>());
            MenuManager.Instance.RegisterMenu(_onGoingCanvas, Menu.CALL, _onGoingCanvas.GetComponent<OnGoingCallMenu>());
            MenuManager.Instance.RegisterMenu(_callingCanvas, Menu.CALLING, _callingCanvas.GetComponent<OnGoingCallMenu>());
            MenuManager.Instance.RegisterMenu(_videoCallCanvas, Menu.VIDEO_CALL, _videoCallCanvas.GetComponent<OnGoingCallMenu>());
            
            // We change the name of the player
            gameObject.name = _playerName;
            
            //TODO: Retrieve the id from the database
            int x = Random.Range(1, 1000000);
            
            _pv.RPC("UpdateIDRPC", RpcTarget.AllBuffered, x);
        }
    }

    private void Update()
    {
        if (!_pv.IsMine) return;

        HandleCharacterInput();
        
        TestCall();
        
        HandleCallCanvas();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Free mouse
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    #region Controller Input Handling
    
    private void LateUpdate()
    {
        if (!_pv.IsMine) return;
        
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
        animator.SetFloat("x", characterInputs.MoveAxisForward, _smoothBlendTransition, Time.deltaTime);
        animator.SetFloat("y", characterInputs.MoveAxisRight, _smoothBlendTransition, Time.deltaTime);
    }
    
    #endregion
    
    private void TestCall()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            //We obtain the photonView of the other player
            Player other = GameObject.Find("Player(Clone)").GetComponent<Player>();

            //TODO: Send the id of the player from database
            SocialManager.Instance.Call(other, true);
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
    
    [PunRPC]
    private void CallRPC(int callerID, string channelName, bool videoCall)
    {
        SocialManager.Instance.ReceiveCall(callerID, channelName, videoCall);
    }
    
    [PunRPC]
    public void AcceptCallRPC()
    {
        SocialManager.Instance.JoinCall();
    }
    
    [PunRPC]
    public void LeaveCallRPC()
    {
        SocialManager.Instance.EndCall();
    }
    
    [PunRPC]
    private void UpdateIDRPC(int id)
    {
        _id = id;
    }
    
    public void EnableVideo()
    {
        _videoCallCamera.enabled = true;
    }
    
    public void DisableVideo()
    {
        _videoCallCamera.enabled = false;
    }
    
    public override void Die()
    {
        RestorePlayer();
        
        GameManager.Instance.JoinRoom(Rooms.LOBBY);
    }

    private void RestorePlayer()
    {
        //TODO: Clear the database of the player from objects and stuff
    }
}
