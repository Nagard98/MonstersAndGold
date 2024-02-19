using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Manages the behaviour of POIs that can despawn(after an amount of time)
public class DespawnPOI : MonoBehaviour
{
    private Despawnable _despawnableObject;
    public GameObject despawnParticleEffect;

    public Despawnable DespawnableObject { set { _despawnableObject = value; } }
    
    public void EarlyDestroy()
    {
        StopCoroutine("DespawnTimerCoroutine");
        StartCoroutine(DespawnTimerCoroutine(2.0f));
    }

    public void SetDespawnTimer(float seconds)
    {
        StartCoroutine(DespawnTimerCoroutine(seconds));
    }

    private IEnumerator DespawnTimerCoroutine(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        despawnParticleEffect.SetActive(true);
        Destroy(transform.gameObject);
        if (_despawnableObject != null) _despawnableObject.OnDespawn();
        else Debug.LogError("ERROR: A Despawnable was not associated with this object: " + this.name);
    }

    private void OnDestroy()
    {
        despawnParticleEffect.transform.parent = transform.parent;
        StopAllCoroutines();
    }
}
