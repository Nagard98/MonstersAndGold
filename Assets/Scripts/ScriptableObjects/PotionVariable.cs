using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu]
public class PotionVariable : POIVariable, Collectable
{
    [SerializeField]
    private FloatVariable playerHP, playerMaxHP;

    public UnityEvent collected;
    public float hpRecovered;

    private void OnEnable()
    {
        isCollectable = true;
    }

    public void OnPickUp()
    {
        playerHP.Value = Mathf.Clamp(playerHP.Value + hpRecovered, 0, playerMaxHP.Value);
        collected.Invoke();
    }
}
