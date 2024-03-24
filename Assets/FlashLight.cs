using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Sirenix.OdinInspector;
using UnityEngine;

public class FlashLight : Item
{
    [ReadOnly]
    [SerializeField]
    private bool isOn;
    
    [SerializeField]
    [InspectorName("Light")]
    private Light lightSource;
    
    [SerializeField]
    [InspectorName("Cooldown")]
    private float cooldown;
    
    private float lastUse;
    
    private PhotonView pv;
    
    private void Awake()
    {
        pv = GetComponent<PhotonView>();
    }
    
    public override void Use()
    {
        if (Time.time - lastUse > cooldown)
        {
            isOn = !isOn;
            
            pv.RPC("StartFlashLight", RpcTarget.All, isOn);
            
            lastUse = Time.time;
        }
    }
    
    [PunRPC]
    public void StartFlashLight(bool isOn)
    {
        lightSource.enabled = isOn;
    }
}
