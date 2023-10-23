using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddContactMenu : MenuUtils
{
    private Animator animator;
    
    private void Awake()
    {
        animator = GetComponent<Animator>();
        HasAnimation = true;
    }
    
    public override void OpenAnimation()
    {
        animator.SetBool("IsOpen", true);
    }
    
    public override void CloseAnimation()
    {
        animator.SetBool("IsOpen", false);
    }
}
