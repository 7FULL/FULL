using UnityEngine;

public abstract class MenuUtils: MonoBehaviour
{
    private bool _hasAnimation = false;
    
    public bool HasAnimation => _hasAnimation;
    
    public virtual void OpenAnimation(){}

    public virtual void CloseAnimation(){}
}