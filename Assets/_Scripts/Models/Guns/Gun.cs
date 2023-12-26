﻿using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Gun", menuName = "Items/Guns/Gun", order = 1)]
public class Gun: Item
{
    private GunData GunData;
    
    private int leftAmmo = -1;
    private int currentAmmo = -1;
    
    public int LeftAmmo => leftAmmo;
    public int CurrentAmmo => currentAmmo;
    
    [SerializeField]
    [InspectorName("Muzzle Flash")]
    private Transform _muzzleFlash;
    
    [SerializeField]
    [InspectorName("Use Muzzle Flash")]
    private bool useMuzzleFlash;
    
    private float shootTimer;

    private void Initialize()
    {
        GunData = (GunData) ItemData;
        
        leftAmmo = GunData.magazineSize * GunData.maxMagazines;
        currentAmmo = GunData.magazineSize;
        
        shootTimer = 0;
    }

    public virtual void Reload()
    {
        //TODO: Do it using reload time
        if (leftAmmo > 0)
        {
            leftAmmo -= GunData.magazineSize;
            currentAmmo = GunData.magazineSize;
        }
        else
        {
            //TODO: Play empty sound
        }
    }
    public virtual void Aim(){}
    public virtual void UnAim(){}
    
    public override void Use()
    {
        Shoot();
    }

    public virtual void Shoot()
    {
        if (currentAmmo == -1 || leftAmmo == -1)
        {
            Initialize();
        }
        
        if (currentAmmo > 0)
        {
            Debug.Log("Shoot timer: " + shootTimer);
            if (GunData.fireRate <= shootTimer)
            {
                Debug.Log("Shoot");
                    
                shootTimer = 0;
                
                currentAmmo--;

                if (Physics.Raycast(GameManager.Instance.Player.CharacterCamera.transform.position, GameManager.Instance.Player.CharacterCamera.transform.forward, out RaycastHit hit))
                {
                    Debug.Log(hit.collider.gameObject.name);
                    
                    if (hit.collider.gameObject.CompareTag("Player"))
                    {
                        Debug.Log("Hit player");
                        
                        hit.collider.gameObject.GetComponent<Entity>().TakeDamage(GunData.damage);
                    }
                }
                
                //TODO: Activate muzzle flash particles
            }
        }
        else
        {
            Reload();
        }
    }

    private void FixedUpdate()
    {
        shootTimer += Time.deltaTime;
    }
}