using UnityEngine;

[CreateAssetMenu(fileName = "Gun", menuName = "Items/Guns/Gun", order = 1)]
public abstract class GunData: ItemData
{
    public int damage;
    public int magazineSize;
    public int maxMagazines;
    public float reloadTime;
    public float fireRate;
    
    //To String
    public override string ToString()
    {
        return "Damage: " + damage + "\nMagazine Size: " + magazineSize + "\nMax Magazines: " + maxMagazines + "\nReload Time: " + reloadTime + "\nFire Rate: " + fireRate;
    }
}