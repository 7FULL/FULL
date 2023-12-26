using UnityEngine;

[CreateAssetMenu(fileName = "Gun", menuName = "Items/Guns/Gun", order = 1)]
public class Gun: Item
{
    private int _damage;
    private int _magazineSize;
    private int _currentAmmo;
    private int _maxAmmo;
    private float _reloadTime;
    private float _fireRate;
    private float _nextTimeToFire;
    private bool _isReloading;
    private Transform _firePoint;
    private int _maxMagazines;

    public int Damage => _damage;
    public int MagazineSize => _magazineSize;
    public int CurrentAmmo => _currentAmmo;
    public int MaxAmmo => _maxAmmo;
    public float ReloadTime => _reloadTime;
    public float FireRate => _fireRate;
    public float NextTimeToFire => _nextTimeToFire;
    public bool IsReloading => _isReloading;
    public Transform FirePoint => _firePoint;
     
    public int MaxMagazines => _maxMagazines;

    // We need this function because depends is VR or not the fire point is different (it changes between the camera and the gun)
    public void InitializeFirePoint(Transform firePoint)
    {
        _firePoint = firePoint;
    }
    
    public override string ToString()
    {
        string itemString = base.ToString();
        itemString += "Damage: " + _damage + "\n";
        itemString += "Magazine Size: " + _magazineSize + "\n";
        itemString += "Current Ammo: " + _currentAmmo + "\n";
        itemString += "Max Ammo: " + _maxAmmo + "\n";
        itemString += "Reload Time: " + _reloadTime + "\n";
        itemString += "Fire Rate: " + _fireRate + "\n";
        itemString += "Next Time To Fire: " + _nextTimeToFire + "\n";
        itemString += "Is Reloading: " + _isReloading + "\n";
        itemString += "Max Magazines: " + _maxMagazines + "\n";
        return itemString;
    }

    public virtual void Use(){}

    public virtual void Reload() {}
    
    //Constructor based on a ScriptableObject
    public Gun(GunScriptableObject gunScriptableObject): base(gunScriptableObject)
    {
        _damage = gunScriptableObject.damage;
        _magazineSize = gunScriptableObject.magazineSize;
        _maxAmmo = gunScriptableObject.maxAmmo;
        _reloadTime = gunScriptableObject.reloadTime;
        _fireRate = gunScriptableObject.fireRate;
        _maxMagazines = gunScriptableObject.maxMagazines;
    }
    
    public Gun(){}
}