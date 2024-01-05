using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chess : MonoBehaviour
{
    private Animation _animator;
    
    private bool _isOpen = false;
    
    private void Awake()
    {
        _animator = GetComponent<Animation>();
    }
    
    public void Open()
    {
        if (_isOpen) return;
        
        _animator.Play();
        
        Instantiate(ItemManager.Instance.GetRandomItem().prefab, transform.position + new Vector3(), Quaternion.identity).GetComponent<Item>().Drop();
        
        _isOpen = true;
    }
}
