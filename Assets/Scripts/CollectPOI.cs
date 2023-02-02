using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class CollectPOI : MonoBehaviour
{
    private Collectable collectableItem;
    public GameObject collectableParticleEffect;

    public Collectable CollectableItem { set { collectableItem = value; } }

    private void OnTriggerEnter(Collider other)
    {
        collectableParticleEffect.transform.parent = gameObject.transform.parent;
        collectableParticleEffect.SetActive(false);
        if (collectableItem != null) collectableItem.OnPickUp();
        else Debug.LogError("ERROR: A Collectable was not associated with this object: "+this.name);

        Destroy(this.gameObject);
    }

    private void OnDestroy()
    {
        collectableParticleEffect.transform.parent = gameObject.transform.parent;
        collectableParticleEffect.SetActive(false);
    }

}
