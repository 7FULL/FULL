using UnityEngine;

public abstract class MenuUtils: MonoBehaviour
{
    private bool _hasAnimation = false;

    public bool HasAnimation
    {
        get => _hasAnimation;
        set => _hasAnimation = value;
    }
    
    public virtual void OpenAnimation(){}

    public virtual void CloseAnimation(){}
}