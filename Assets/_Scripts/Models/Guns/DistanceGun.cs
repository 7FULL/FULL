using UnityEngine;

[CreateAssetMenu(fileName = "DistanceGun", menuName = "Items/Guns/DistanceGun", order = 1)]
public class DistanceGun: Gun
{
    private int _range;
    
    public int Range => _range;
    
    public DistanceGun(DistanceGunScriptableObject distanceGunScriptableObject): base(distanceGunScriptableObject)
    {
        _range = distanceGunScriptableObject.range;
    }
}