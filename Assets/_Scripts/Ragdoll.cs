using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Ragdoll : MonoBehaviour
{
    // Start is called before the first frame update

    private Animator animator;
    private Rigidbody[] rigidbodies;
    
    private Collider[] colliders;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rigidbodies = GetComponentsInChildren<Rigidbody>();
        colliders = GetComponentsInChildren<Collider>();
        
        SetEnabledRagdoll(false);
    }

    private void SetEnabledRagdoll(bool enabled)
    {
        animator.enabled = !enabled;
        
        foreach (Rigidbody rigidbody in rigidbodies)
        {
            rigidbody.isKinematic = !enabled;
        }
        
        foreach (Collider collider in colliders)
        {
            collider.enabled = enabled;
        }
        
        //If we are enabling the ragdoll, we will add a force in a random rigidbody in a random direction
        if (enabled)
        {
            Rigidbody randomRigidbody = rigidbodies[Random.Range(0, rigidbodies.Length)];
            randomRigidbody.AddForce(Random.insideUnitSphere * 250);
        }
    }
    
    public void EnableRagdoll()
    {
        SetEnabledRagdoll(true);
    }
}
