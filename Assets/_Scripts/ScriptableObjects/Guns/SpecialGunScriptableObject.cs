using UnityEngine;

[CreateAssetMenu(fileName = "SpecialGun", menuName = "Items/Guns/SpecialGun", order = 1)]
public class SpecialGunScriptableObject: GunScriptableObject
{
    public override Item ToModel()
    {
        return new SpecialGun(this);
    }
}