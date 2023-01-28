using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class POIVariable : ScriptableObject, Unloadable
{
    public GameObject gameObject;
    public bool isCollectable;
    private int tier;

    public int Tier { get { return tier; }  set { tier = value; } }

    [SerializeField]
    public GameEvent onUnloadAction;

    public abstract float GetValue();

    public void OnUnload()
    {
        onUnloadAction.Raise();
    }
}
