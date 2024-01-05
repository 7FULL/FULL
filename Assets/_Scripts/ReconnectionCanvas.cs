using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class ReconnectionCanvas : MenuUtils
{
    [SerializeField]
    [InspectorName("Camera to enable")]
    private Camera _camera;

    private void Awake()
    {
        _camera.enabled = false;
    }

    public override void OpenAnimation()
    {
        //Little pop up animation using DOTween
        transform.DOScale(1, 0.5f).SetEase(Ease.OutBack);
        transform.DOLocalMoveY(0, 0.5f).SetEase(Ease.OutBack);
        
        _camera.enabled = true;
    }

    public override void CloseAnimation()
    {
        transform.DOScale(0, 0.5f).SetEase(Ease.InBack);
        transform.DOLocalMoveY(-1000, 0.5f).SetEase(Ease.InBack);
        
        _camera.enabled = false;
    }
}
