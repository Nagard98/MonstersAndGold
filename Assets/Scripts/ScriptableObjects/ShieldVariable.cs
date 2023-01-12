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

    private void OnEnable()
    {
        isCollectable = true;
    }

    public void OnPickUp()
    {
        _inventory.Add(this);
        collected.Invoke();
    }
}
