using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu]
public class ShieldVariable : POIVariable, Collectable
{
    [SerializeField]
    private Inventory _inventory;
    public UnityEvent collected;
    public float damageThreshold;


    public override Color GetSpriteColor()
    {
        return Color.green;
    }

    private void OnEnable()
    {
        isCollectable = true;
    }

    public void OnPickUp()
    {
        _inventory.Add(this);
        _inventory.LastTierTouched = this.Tier;
        collected.Invoke();
    }

    public override float GetValue()
    {
        return damageThreshold;
    }
}
