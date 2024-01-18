using System;
using UnityEngine;
using UnityEngine.UI;

public class EditBuildingCells: MonoBehaviour
{
    private bool isEditing = false;
    
    public bool IsEditing => isEditing;

    private Image _image;
    
    private void Awake()
    {
        _image = GetComponent<Image>();
    }
    
    public void Edit()
    {
        isEditing = !isEditing;
        
        _image.color = isEditing ? new Color(12,150,226) : Color.white;
    }

    public void Reset()
    {
        isEditing = false;
        
        _image.color = new Color(12,150,226);
    }
    
    public void SetEditing(bool value)
    {
        isEditing = value;
        
        _image.color = !isEditing ? new Color(12,150,226) : Color.white;
    }
}