using System;
using UnityEngine;
using UnityEngine.UI;

public class EditBuildingCells: MonoBehaviour
{
    private bool isEditing = false;
    
    public bool IsEditing => isEditing;

    private Material _image;
    
    private void Awake()
    {
        _image = GetComponent<MeshRenderer>().material;
    }
    
    public void Edit()
    {
        isEditing = !isEditing;
        
        if (isEditing)
        {
            _image.color = Color.white;
        }
        else
        {
            _image.color = new Color(0.04517614f,0.5865791f,0.8867924f, 1);
        }
    }

    public void Reset()
    {
        isEditing = false;
        
        _image.color = new Color(0.04517614f,0.5865791f,0.8867924f, 1);
    }
    
    public void SetEditing(bool value)
    {
        isEditing = value;

        if (isEditing)
        {
            _image.color = Color.white;
        }
        else
        {
            _image.color = new Color(0.04517614f,0.5865791f,0.8867924f, 1);
        }
    }
}