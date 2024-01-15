using System;
using DG.Tweening;
using Photon.Pun;
using Sirenix.OdinInspector;
using UnityEngine;

public class Gun: Item
{
    private GunData GunData;
    
    private int leftAmmo = -1;
    private int currentAmmo = -1;
    
    public int LeftAmmo => leftAmmo;
    public int CurrentAmmo => currentAmmo;
    
    [SerializeField]
    [InspectorName("Use Muzzle Flash")]
    private bool useMuzzleFlash;
    
    [SerializeField]
    [InspectorName("Muzzle Flash")]
    [ShowIf("useMuzzleFlash")]
    private ParticleSystem _muzzleFlash;
    
    [SerializeField]
    [InspectorName("Gun Tip")]
    private Transform gunTip;
    
    private float shootTimer;
    
    private bool alreadeyInitialized = false;
    
    private Recoil _recoilsScript;
    
    private bool canShoot = true;
    
    private PhotonView pv;

    private void OnEnable()
    {
        if (!alreadeyInitialized)
        {
            Initialize();
            
            pv = GetComponent<PhotonView>();
        }
    }

    private void Initialize()
    {
        GunData = (GunData) ItemData;
        
        leftAmmo = GunData.magazineSize * GunData.maxMagazines;
        currentAmmo = GunData.magazineSize;
        
        shootTimer = 0;
        
        alreadeyInitialized = true;
        
        _recoilsScript = GameManager.Instance.Player.RecoilScript;
    }
    
    public void RestartAmmo()
    {
        leftAmmo = GunData.magazineSize * GunData.maxMagazines;
        currentAmmo = GunData.magazineSize;
    }

    public virtual void Reload()
    {
        if (!canShoot)
        {
            return;
        }

        if (currentAmmo == GunData.magazineSize)
        {
            return;
        }
        
        if (leftAmmo > 0)
        {
            canShoot = false;
            
            //We play the reload animation TODO
            //GameManager.Instance.Player.Animator.SetTrigger("Reload");
            
            //We play the reload sound
            AudioManager.Instance.PlaySound(GunData.reloadSound, transform.position);
            
            //This is a temporary animation using DOTween until we have the reload animation
            //The animation is the gun rotating 
            transform.DOLocalRotate(new Vector3(-360 * GunData.reloadFlips, 0, 0), GunData.reloadTime, RotateMode.FastBeyond360).SetEase(Ease.Linear);
            
            //We wait the reload time
            DOVirtual.DelayedCall(GunData.reloadTime, () =>
            {
                //We calculate the ammo that we need to reload
                int ammoToReload = GunData.magazineSize - currentAmmo;
                
                //We check if we have enough ammo to reload
                if (ammoToReload <= leftAmmo)
                {
                    //We reload the ammo
                    currentAmmo += ammoToReload;
                    leftAmmo -= ammoToReload;
                }
                else
                {
                    //We reload the ammo
                    currentAmmo += leftAmmo;
                    leftAmmo = 0;
                }
                
                canShoot = true;
                
                //We update the UI
                GameManager.Instance.Player.UpdateAmmoUI();
            });
        }
        else
        {
            //TODO: Play empty sound
        }
    }
    
    public override void Use()
    {
        Shoot();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Reload();
        }
    }

    public virtual void Shoot()
    {
        if (currentAmmo == -1 || leftAmmo == -1)
        {
            Initialize();
        }
        
        if (currentAmmo > 0 && canShoot)
        {
            if (GunData.fireRate <= shootTimer)
            {
                shootTimer = 0;
                
                currentAmmo--;
                
                _recoilsScript.Configure(GunData.recoilData);
                //We make recoil using gun data
                _recoilsScript.RecoilFire();

                if (Physics.Raycast(GameManager.Instance.Player.CharacterCamera.Camera.transform.position, GameManager.Instance.Player.CharacterCamera.Camera.transform.forward, out RaycastHit hit))
                {
                    if (hit.collider.gameObject.CompareTag("Player"))
                    {
                        Entity entity = hit.collider.gameObject.GetComponentInParent<Entity>();

                        if (entity.PV.IsMine)
                        {
                            return;
                        }
                        
                        bool dead = entity.TakeDamage(GunData.damage, entity.PV);
                        
                        //Crosshair hit
                        GameManager.Instance.Player.HitCrosshair();
 
                        if (dead)
                        {
                            GameManager.Instance.Player.AddCoins(1000);

                            MenuManager.Instance.PopUp("You have killed a player!!!");
                        }
                    }
                    else
                    {
                        if (GunData.bulletHole != null)
                        {
                            pv.RPC("CreateBulletHole", RpcTarget.All, hit.point, hit.normal);
                        }
                    }
                }
                
                if (useMuzzleFlash)
                {
                    pv.RPC("MuzzleFlash", RpcTarget.All, gunTip.position, gunTip.rotation);
                }
            }
        }
        else if (canShoot)
        {
            Reload();
        }
    }

    private void FixedUpdate()
    {
        shootTimer += Time.deltaTime;
    }
    
    public Sprite GetCrosshair()
    {
        return GunData.crosshair;
    }
    
    [PunRPC]
    public void CreateBulletHole(Vector3 position, Vector3 normal)
    {
        GameObject bulletImpacstoneObj= Instantiate(GunData.bulletHole, position + normal * 0.001f, Quaternion.LookRotation(normal, Vector3.up));
        Destroy(bulletImpacstoneObj, 10f);
        //TODO: Maybe we could move to a parent to do not have a lot of bullet holes in the scene
        //bulletImpacstoneObj.transform.SetParent(hit.collider.gameObject.transform);
    }
    
    [PunRPC]
    public void MuzzleFlash(Vector3 position, Quaternion rotation)
    {
        //GameObject muzzle= Instantiate(_muzzleFlash, gunTip.position, gunTip.rotation);
        //muzzle.transform.SetParent(gunTip.transform);
        //Destroy(muzzle,5);
        _muzzleFlash.Play();
                    
        //Play shoot sound
        AudioManager.Instance.PlaySound(GunData.shootSound, transform.position);
    }
}