using System.Collections;
using UnityEngine;

[CreateAssetMenu]
public class Inventory : RuntimeSet<ShieldVariable>
{
    public int maxShields;
    private int _lastTierTouched;

    public int LastTierTouched { get { return _lastTierTouched; } set { _lastTierTouched = value; } }

    public bool IsFull { get { return maxShields == this.Items.Count; } }

    public void Clear()
    {
        Items.Clear();
    }
}