using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(PhotonView))]
public class Interruptor : MonoBehaviour
{
    [SerializeField]
    [InspectorName("Image")]
    private Image image;

    private PhotonView pv;
    
    private bool isOn = false;
    
    public bool IsOn => isOn;
    
    private void Start()
    {
        pv = GetComponent<PhotonView>();
    }
    
    [PunRPC]
    public void ChangeState(bool state)
    {
        isOn = state;
        
        image.color = state ? Color.green : Color.red;
        
        ExitDoorHorrorMaze.Instance.CheckInterruptors();
    }
    
    public void OnClick()
    {
        pv.RPC("ChangeState", RpcTarget.AllBuffered, !isOn);
    }
}
