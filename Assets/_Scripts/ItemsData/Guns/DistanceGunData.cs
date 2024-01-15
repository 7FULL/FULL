using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "DistanceGun", menuName = "Items/Guns/DistanceGun", order = 1)]
public class DistanceGunData: GunData
{
    [Title("Distance Gun data")]
    public int range; 
}