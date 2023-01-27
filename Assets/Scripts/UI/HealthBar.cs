using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public FloatVariable maxHealth;
    public FloatVariable currentHealth;
    private Image bar;
    private Rect rct;

    void Start()
    {
        bar = GetComponentInChildren<Image>();
        rct = bar.rectTransform.rect;
        rct.width = currentHealth.Value;
    }

    // Update is called once per frame
    void Update()
    {
        rct.width = currentHealth.Value;
        bar.fillAmount = (currentHealth.Value / maxHealth.Value);
    }
}
