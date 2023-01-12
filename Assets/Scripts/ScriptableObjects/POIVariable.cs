using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class POIVariable : ScriptableObject, Unloadable
{
    public GameObject gameObject;
    public bool isCollectable;
    public int tier;

    [SerializeField]
    public GameEvent onUnloadAction;

    public void OnUnload()
    {
        onUnloadAction.Raise();
    }
}
