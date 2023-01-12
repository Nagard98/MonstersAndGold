using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnloadPOI : MonoBehaviour
{
    private Unloadable unloadableObject;

    public Unloadable UnloadableObject { set { unloadableObject = value; } }

    private void OnDestroy()
    {
        if (unloadableObject != null) unloadableObject.OnUnload();
        else Debug.LogError("ERROR: An Unloadable was not associated with this object: " + this.name);
    }
}
