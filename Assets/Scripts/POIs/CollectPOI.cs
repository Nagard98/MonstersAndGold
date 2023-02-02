using UnityEngine;

//Manages the behaviour of POIs that can be collected
[RequireComponent(typeof(SphereCollider))]
public class CollectPOI : MonoBehaviour
{
    private Collectable _collectableItem;
    public GameObject collectableParticleEffect;

    public Collectable CollectableItem { set { _collectableItem = value; } }

    private void OnTriggerEnter(Collider other)
    {
        collectableParticleEffect.transform.parent = gameObject.transform.parent;
        collectableParticleEffect.SetActive(false);
        if (_collectableItem != null) _collectableItem.OnPickUp();
        else Debug.LogError("ERROR: A Collectable was not associated with this object: "+this.name);

        Destroy(this.gameObject);
    }

    private void OnDestroy()
    {
        collectableParticleEffect.transform.parent = transform.parent;
        collectableParticleEffect.SetActive(false);
    }

}
