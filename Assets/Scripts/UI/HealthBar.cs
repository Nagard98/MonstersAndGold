using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class HealthBar : MonoBehaviour
{
    public FloatVariable maxHealth;
    public FloatVariable currentHealth;
    public float healtAnimDuration;
    private Image bar;
    private Rect rct;

    void Start()
    {
        bar = GetComponentInChildren<Image>();
        rct = bar.rectTransform.rect;
        rct.width = currentHealth.Value;
    }

    public void UpdateHealthBar()
    {
        bar.DOFillAmount(currentHealth.Value/ maxHealth.Value, healtAnimDuration);
    }
}
