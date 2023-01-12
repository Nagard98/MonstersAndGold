using System.Collections;
using UnityEngine;

[CreateAssetMenu]
public class Inventory : RuntimeSet<ShieldVariable>
{
    public int maxShields;

    public bool IsFull { get { return maxShields == this.Items.Count; } }
}