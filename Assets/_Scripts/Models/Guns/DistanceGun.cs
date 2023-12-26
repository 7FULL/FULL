using System;
using UnityEngine;

[CreateAssetMenu(fileName = "DistanceGun", menuName = "Items/Guns/DistanceGun", order = 1)]
public abstract class DistanceGun: Gun
{
    private DistanceGunData gunInfo;
    
    public DistanceGunData GunInfo => gunInfo;

    private void Awake()
    {
        gunInfo = (DistanceGunData) ItemData;
    }

    [SerializeField]
    [InspectorName("Range")]
    private int _range;
    
    public int Range => _range;
}