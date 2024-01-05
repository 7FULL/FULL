using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestBot : Entity
{
    // Start is called before the first frame update
    void Start()
    {
        InitializeRPC(100,100);
    }
    
    public override void Die(bool restore = true)
    {
        StartCoroutine(Respawn());
        //Restore health and armor and respawn in 2 seconds
        gameObject.SetActive(false);
    }
    
    IEnumerator Respawn()
    {
        yield return new WaitForSeconds(2);
        
        gameObject.SetActive(true);
        
        RestoreAll();
    }
}
