using DG.Tweening;
using UnityEngine;

public class MenuUtils: MonoBehaviour
{
    [SerializeField]
    [InspectorName("Has Animation")]
    private bool _hasAnimation = false;
    
    [SerializeField]
    [InspectorName("Disable on close")]
    private bool _disableOnClose = false;
    
    [SerializeField]
    [InspectorName("Horizontal animation")]
    private bool _horizontalAnimation = false;
    
    [SerializeField]
    [InspectorName("Do translation")]
    private bool _doTranslation = true;
    
    public bool HasAnimation
    {
        get => _hasAnimation;
        set => _hasAnimation = value;
    }

    public virtual void OpenAnimation()
    {
        //Little pop up animation using DOTween
        transform.DOScale(1, 0.5f).SetEase(Ease.OutBack);

        if (_doTranslation)
        {
            if (_horizontalAnimation)
            {
                transform.DOLocalMoveX(0, 0.5f).SetEase(Ease.OutBack);
            }
            else
            {
                transform.DOLocalMoveY(0, 0.5f).SetEase(Ease.OutBack);
            }
        }
    }

    public virtual void CloseAnimation()
    {
        transform.DOScale(0, 0.5f).SetEase(Ease.InBack).onComplete += () =>
        {
            if (_disableOnClose)
            {
                gameObject.SetActive(false);
            }
        };

        if (_doTranslation)
        {
            if (_horizontalAnimation)
            {
                transform.DOLocalMoveX(-1000, 0.5f).SetEase(Ease.InBack).onComplete += () =>
                {
                    if (_disableOnClose)
                    {
                        gameObject.SetActive(false);
                    }
                };
            }
            else
            {
                transform.DOLocalMoveY(-1000, 0.5f).SetEase(Ease.InBack).onComplete += () =>
                {
                    if (_disableOnClose)
                    {
                        gameObject.SetActive(false);
                    }
                };
            }
        }
    }

    public void OnClickCloseAnimation()
    {
        MenuManager.Instance.CloseMenu();
    }
}