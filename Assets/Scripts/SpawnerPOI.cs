using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SpawnerPOI : MonoBehaviour
{

    public PathChunksSet pathChunks;
    public BezierCurveVariable path;
    
    public void SpawnPOI(POIVariable poi, SpawnSettings spawnSettings)
    {
        BezierSpline tmp = (BezierSpline)path.Value.Clone();
        Vector3 orthoVector;
        Vector3 spawnPosition = tmp.MoveLongDistance(spawnSettings.distance, out orthoVector);
        //PathChunk pc = pathChunks.Get(chunkIndex);
        if (spawnPosition == null)
        {
            //TO-DO: Aggiungi richiesta spawn uno volta che chunk verrà istanziato
            print("There is not a chunk instantiated at that distance");
        }
        else
        {
            spawnPosition.y += 10;
            Vector3 groundedPos = CharacterRailMovement.GetGroundPosition(spawnPosition);
            groundedPos.y += spawnSettings.groundOffset;

            GameObject spawnedPOI = Instantiate(poi.gameObject, groundedPos, Quaternion.identity);
            spawnedPOI.transform.parent = transform;
            if (poi.isCollectable)
            {
                CollectPOI collectPOI = spawnedPOI.AddComponent<CollectPOI>();
                collectPOI.CollectableItem = (Collectable)poi;
            }
            else
            {
                spawnedPOI.transform.localPosition += orthoVector * 3f;
                EnemyAI enemyAi = spawnedPOI.GetComponent<EnemyAI>();
                //TO-DO: forse non è necessario fare così
                enemyAi.enemyInfo = (EnemyVariable)poi;
                Vector3 ortho2 = Vector3.Cross(orthoVector, Vector3.up).normalized;
                Vector3[] navPoints = new Vector3[2];
                navPoints[0] = spawnedPOI.transform.position + (ortho2 * 5);
                navPoints[1] = spawnedPOI.transform.position - (ortho2 * 5);
               
                enemyAi.navPoints = navPoints;
                EndlessPath.localNavMeshBuilder.m_Tracked = spawnedPOI.transform;
            }
            DespawnPOI despawnPOI = spawnedPOI.AddComponent<DespawnPOI>();
            despawnPOI.DespawnableObject = poi;
            despawnPOI.SetDespawnTimer(spawnSettings.TTL);

            //Destroy(spawnedPOI, spawnSettings.TTL);
        }

    }
}
