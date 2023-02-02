using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu]
public class PotionVariable : POIVariable, Collectable
{
    [SerializeField]
    private FloatVariable _playerHP, _playerMaxHP;

    public UnityEvent collected;
    public float hpRecovered;
    

    public override Color GetSpriteColor()
    {
        return Color.red;
    }

    private void OnEnable()
    {
        isCollectable = true;
    }

    public void OnPickUp()
    {
        _playerHP.Value = Mathf.Clamp(_playerHP.Value + hpRecovered, 0, _playerMaxHP.Value);
        collected.Invoke();
    }

    public override float GetValue()
    {
        return hpRecovered;
    }
}
