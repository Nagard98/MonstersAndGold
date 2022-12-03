using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerSpawn : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        SendMessageUpwards("TriggerChunkBuild");
    }

}
