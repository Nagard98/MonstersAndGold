using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Vector3Variable : ScriptableObject
{
    public Vector3 Value;

    public void Destroy()
    {
        Value = Vector3.zero;
    }
}
