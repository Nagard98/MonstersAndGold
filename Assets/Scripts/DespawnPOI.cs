using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DespawnPOI : MonoBehaviour
{
    private Despawnable despawnableObject;

    public Despawnable DespawnableObject { set { despawnableObject = value; } }
    
    public void SetDespawnTimer(float seconds)
    {
        StartCoroutine(DespawnTimerCoroutine(seconds));
    }

    private IEnumerator DespawnTimerCoroutine(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        Destroy(transform.gameObject);
        if (despawnableObject != null) despawnableObject.OnDespawn();
        else Debug.LogError("ERROR: A Despawnable was not associated with this object: " + this.name);
    }

    /*private void OnDestroy()
    {
        if (despawnableObject != null) despawnableObject.OnDespawn();
        else Debug.LogError("ERROR: A Despawnable was not associated with this object: " + this.name);
    }*/
}
