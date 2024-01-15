using System;
using UnityEngine;

public class Util: Item
{
    private UtilData utilData;
    
    public UtilData UtilData => utilData;
    
    private SpriteRenderer spriteRenderer;
    
    private Transform playerCamera;
    
    private void Awake()
    {
        utilData = (UtilData) ItemData;

        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        
        playerCamera = GameManager.Instance.Player.CharacterCamera.Camera.transform;
        
        spriteRenderer.sprite = utilData.sprite;
    }
    
    public override void Use()
    {
        int healthToRestore = utilData.restoreHealth ? utilData.healthToRestore : 0;
        int shieldToRestore = utilData.restoreShield ? utilData.shieldToRestore : 0;

        if (healthToRestore > 0 || shieldToRestore > 0)
        {
            GameManager.Instance.Player.RestoreHealthAndArmor(healthToRestore, shieldToRestore);
        }
        
        Remove(1);
    }

    private void Update()
    {
        //Sprite renderer look at player camera
        spriteRenderer.transform.LookAt(playerCamera);
    }
}