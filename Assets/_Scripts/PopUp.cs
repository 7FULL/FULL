using TMPro;
using UnityEngine;

public class PopUp: MenuUtils
{
    [InspectorName("Animator")]
    [SerializeField]
    private Animator _animator;
    
    [InspectorName("Text to PopUp")]
    [SerializeField]
    private TMP_Text text;
    
    private void Awake()
    {
        HasAnimation = true;
    }
    
    public override void OpenAnimation()
    {
        _animator.SetBool("IsOpen", true);
    }
    
    public void Configure(string text)
    {
        this.text.text = text;
    }
    
    public override void CloseAnimation()
    {
        _animator.SetBool("IsOpen", false);
    }
}