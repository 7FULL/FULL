using UnityEngine;

[CreateAssetMenu(fileName = "DistanceGun", menuName = "Items/Guns/DistanceGun", order = 1)]
public class DistanceGunScriptableObject: GunScriptableObject
{
    public int range;
    public override Item ToModel()
    {
        DistanceGun distanceGun = new DistanceGun(this);
        return distanceGun;
    }
}