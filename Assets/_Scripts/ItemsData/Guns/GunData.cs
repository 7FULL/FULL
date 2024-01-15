using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "Gun", menuName = "Items/Guns/Gun", order = 1)]
public abstract class GunData: ItemData
{
    [Title("Gun data")]
    public int damage;
    
    [InlineButton("_5", "   5   ")]
    [InlineButton("_12", "   12   ")]
    [InlineButton("_25", "   25   ")]
    [InlineButton("_33", "   33   ")]
    public int magazineSize;
    public int maxMagazines;
    public float reloadTime;
    public float fireRate;
    [BoxGroup("Recoil"), HideLabel]
    public RecoilData recoilData;
    
    [Space(10)]
    public GameObject bulletHole;
    public Sprite crosshair;
    public int reloadFlips;
    
    [Title("Sounds")]
    [InlineButton("PlayShootSound", "   Play   ")]
    public AudioClip shootSound;

    [InlineButton("PlayReloadSound", "   Play   ")]
    public AudioClip reloadSound;
    
    //To String
    public override string ToString()
    {
        return "Damage: " + damage + "\nMagazine Size: " + magazineSize + "\nMax Magazines: " + maxMagazines + "\nReload Time: " + reloadTime + "\nFire Rate: " + fireRate;
    }
    
    //Inline Buttons
    private void _5()
    {
        magazineSize = 5;
    }
    
    private void _12()
    {
        magazineSize = 12;
    }
    
    private void _25()
    {
        magazineSize = 25;
    }
    
    private void _33()
    {
        magazineSize = 33;
    }
    
    private void PlaySound(AudioClip clip)
    {
        AudioSource.PlayClipAtPoint(clip, Vector3.zero);
    }

    private void PlayShootSound()
    {
        PlaySound(shootSound);
    }

    private void PlayReloadSound()
    {
        PlaySound(reloadSound);
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