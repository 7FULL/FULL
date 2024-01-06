using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Recoil : MonoBehaviour
{
    private Vector3 currentRotation;
    private Vector3 tarjetRotation;
    
    private RecoilData recoilData;

    private float normalrecoilx;
    
    private float normalrecoily;

    private void Start()
    {
        normalrecoilx = -recoilData.recoilX;
        normalrecoily = recoilData.recoilY;
    }

    private void Update()
    {
        tarjetRotation = Vector3.Lerp(tarjetRotation, Vector3.zero, recoilData.returnSpeed * Time.deltaTime);
        currentRotation = Vector3.Slerp(currentRotation, tarjetRotation, recoilData.snapiness * Time.fixedDeltaTime);
        transform.localRotation=Quaternion.Euler(currentRotation);
    }

    public void Configure(RecoilData recoilData)
    {
        this.recoilData = recoilData;
    }

    public void RecoilFire()
    {
        tarjetRotation += new Vector3(-recoilData.recoilX, Random.Range(-recoilData.recoilY, recoilData.recoilY), 0);
    }
}