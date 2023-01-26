using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class BezierCurveVariable : ScriptableObject
{

    public BezierSpline Value;

    public void Destroy()
    {
        Value = null;
    }

}
