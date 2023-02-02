using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Events;

public class HealthBar : MonoBehaviour
{
    public FloatVariable maxHealth;
    public FloatVariable currentHealth;
    public float healtAnimDuration;
    private Image bar;
    private Rect rct;
    public UnityEvent Death;

    void Start()
    {
        bar = GetComponentInChildren<Image>();
        rct = bar.rectTransform.rect;
        rct.width = currentHealth.Value;
    }

    public void UpdateHealthBar()
    {
        bar.DOFillAmount(currentHealth.Value/ maxHealth.Value, healtAnimDuration);
        if (currentHealth.Value <= 0f) Death.Invoke();
    }
}
