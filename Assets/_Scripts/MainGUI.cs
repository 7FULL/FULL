using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainGUI : MonoBehaviour
{
    [SerializeField]
    [InspectorName("Life Text")]
    private TMP_Text lifeText;
    
    [SerializeField]
    [InspectorName("Life Bar")]
    private Image lifeBar;
    
    [SerializeField]
    [InspectorName("Armor Text")]
    private TMP_Text armorText;
    
    [SerializeField]
    [InspectorName("Armor Bar")]
    private Image armorBar;

    private void Awake()
    {
        this.gameObject.SetActive(false);
    }

    private void FixedUpdate()
    {
        if (GameManager.Instance.Player != null)
        {
            lifeText.text = GameManager.Instance.Player.Health.ToString() + " / " + GameManager.Instance.Player.MaxHealth.ToString();
            lifeBar.fillAmount = (float) GameManager.Instance.Player.Health / GameManager.Instance.Player.MaxHealth;
            armorText.text = GameManager.Instance.Player.Armor.ToString() + " / " + GameManager.Instance.Player.MaxArmor.ToString();
            armorBar.fillAmount = (float) GameManager.Instance.Player.Armor / GameManager.Instance.Player.MaxArmor;
        }
    }
}
