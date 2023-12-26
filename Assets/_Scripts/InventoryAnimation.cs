using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryAnimation : MonoBehaviour
{
    [SerializeField]
    private Sprite[] _sprites;
    
    [SerializeField]
    private float _animationSpeed = 0.1f;
    
    private Image _image;
    
    private int _currentSprite = 0;
    
    private float _timer = 0f;
    
    private void Awake()
    {
        _image = GetComponent<Image>();
    }
    
    private void Update()
    {
        _timer += Time.deltaTime;
        
        if (_timer >= _animationSpeed)
        {
            _timer = 0f;
            
            _currentSprite++;
            
            if (_currentSprite >= _sprites.Length)
            {
                _currentSprite = 0;
            }
            
            _image.sprite = _sprites[_currentSprite];
        }
    }
}
