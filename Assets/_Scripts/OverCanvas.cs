using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OverCanvas: MonoBehaviour
{
    [SerializeField] 
    private TMP_Text tecla;
    
    [SerializeField]
    private TMP_Text texto;
    
    HorizontalLayoutGroup layout;
    
    private void Awake()
    {
        layout = GetComponent<HorizontalLayoutGroup>();
        
        this.gameObject.SetActive(false);
    }
    
    public void Configure(string tecla, string texto)
    {
        this.tecla.text = tecla;
        this.texto.text = texto;
        
        //Actualizamos el tamaño del layout
        layout.CalculateLayoutInputHorizontal();
    }
}