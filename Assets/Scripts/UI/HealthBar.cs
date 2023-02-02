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
    private Image _bar;
    private Rect _rct;

    public UnityEvent Death;

    void Start()
    {
        _bar = GetComponentInChildren<Image>();
        _rct = _bar.rectTransform.rect;
        _rct.width = currentHealth.Value;
    }

    public void UpdateHealthBar()
    {
        _bar.DOFillAmount(currentHealth.Value/ maxHealth.Value, healtAnimDuration);
        if (currentHealth.Value <= 0f) Death.Invoke();
    }
}
