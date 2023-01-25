using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class TriggerSpawn : MonoBehaviour
{
    private void Start()
    {
        BoxCollider collider = GetComponent<BoxCollider>();
        collider.isTrigger = true;
        collider.center = new Vector3(x: PathGenerator.pathChunkSize / 2, z: PathGenerator.pathChunkSize, y: 10f);
        collider.size = new Vector3(x: PathGenerator.pathChunkSize, z: 5f, y: 40f);

    }

    private void OnTriggerEnter(Collider other)
    {

        SendMessageUpwards("InitNewChunk", true);
    }

}