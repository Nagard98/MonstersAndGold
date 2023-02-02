using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class FloatReference
{
    public bool useConstant = true;
    public FloatVariable variable;
    public float constant;

    public float Value
    {
        get { return useConstant ? constant : variable.Value; }
    }
}
