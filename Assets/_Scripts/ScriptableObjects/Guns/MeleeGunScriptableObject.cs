using UnityEngine;

[CreateAssetMenu(fileName = "MeleeGun", menuName = "Items/Guns/MeleeGun", order = 1)]
public class MeleeGunScriptableObject: GunScriptableObject
{
    public override Item ToModel()
    {
        return new MeleeGun(this);
    }
}