using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ShieldInventory : MonoBehaviour
{
    public Inventory inventory;
    private RectTransform[] _shieldIcons;
    private Image[] _shieldSprites;
    private int _shieldsInUI;
    public float shieldAnimDuration;

    public void UpdateInventory()
    {
        if (inventory.Items.Count > _shieldsInUI)
        {
            _shieldIcons[inventory.LastTierTouched].DOScale(1, shieldAnimDuration);
            _shieldsInUI += 1;
        }
        else if(inventory.Items.Count < _shieldsInUI)
        {
            _shieldIcons[inventory.LastTierTouched].DOScale(0, shieldAnimDuration);
            _shieldsInUI -= 1;
        }
        
    }

    private void OnDisable()
    {
        for(int i = 1; i < _shieldIcons.Length; i++)
        {
            _shieldIcons[i].DOKill();
            _shieldIcons[i].localScale = Vector3.zero;
        }
        _shieldsInUI = 0;
    }

    void Start()
    {
        _shieldSprites = GetComponentsInChildren<Image>();
        _shieldIcons = GetComponentsInChildren<RectTransform>();
        _shieldsInUI = 0;
    }

}
