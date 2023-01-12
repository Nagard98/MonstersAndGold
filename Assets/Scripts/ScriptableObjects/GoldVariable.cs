using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu]
public class GoldVariable : POIVariable, Collectable
{
    [SerializeField]
    private FloatVariable playerGP;
    public int valueGP;
    public UnityEvent collected;

    private void OnEnable()
    {
        isCollectable = true;
    }

    public void OnPickUp()
    {
        playerGP.Value += valueGP;
        collected.Invoke();
    }
}
