using UnityEngine;

[CreateAssetMenu(fileName = "Gun", menuName = "Items/Guns/Gun", order = 1)]
public abstract class GunScriptableObject: ItemScriptableObject
{
    public int damage;
    public int magazineSize;
    public int maxAmmo;
    public int maxMagazines;
    public float reloadTime;
    public float fireRate;
}