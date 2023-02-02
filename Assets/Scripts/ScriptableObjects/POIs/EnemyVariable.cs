using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class EnemyVariable : POIVariable
{
    public int atkDamage;

    public override Color GetSpriteColor()
    {
        return Color.blue;
    }

    public override float GetValue()
    {
        return atkDamage;
    }
}
