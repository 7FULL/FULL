using System;
using UnityEngine;

public class DistanceGun: Gun
{
    private DistanceGunData gunInfo;
    
    public DistanceGunData GunInfo => gunInfo;

    private void Awake()
    {
        gunInfo = (DistanceGunData) ItemData;
    }
}