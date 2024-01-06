using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Gun", menuName = "Items/Guns/Gun", order = 1)]
public abstract class GunData: ItemData
{
    public int damage;
    public int magazineSize;
    public int maxMagazines;
    public float reloadTime;
    public float fireRate;
    public RecoilData recoilData;
    public GameObject bulletHole;
    public Sprite crosshair;
    public int reloadFlips;
    public AudioClip shootSound;
    public AudioClip reloadSound;
    
    //To String
    public override string ToString()
    {
        return "Damage: " + damage + "\nMagazine Size: " + magazineSize + "\nMax Magazines: " + maxMagazines + "\nReload Time: " + reloadTime + "\nFire Rate: " + fireRate;
    }
}

[Serializable]
public struct RecoilData
{
    public float recoilX;
    public float recoilY;
    
    public float snapiness;
    public float returnSpeed;
}