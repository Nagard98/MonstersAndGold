using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu]
public class GoldVariable : POIVariable, Collectable
{
    [SerializeField]
    private FloatVariable _playerGP;
    public int valueGP;
    public UnityEvent collected;

    private void OnEnable()
    {
        isCollectable = true;
    }

    public void OnPickUp()
    {
        _playerGP.Value += valueGP;
        collected.Invoke();
    }

    public override float GetValue()
    {
        return valueGP;
    }

    public override Color GetSpriteColor()
    {
        return Color.yellow;
    }
}
