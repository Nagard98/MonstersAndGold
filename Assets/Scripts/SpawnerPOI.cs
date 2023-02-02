using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SpawnerPOI : MonoBehaviour
{

    public PathChunksSet pathChunks;
    public BezierCurveVariable path;
    private GameObject spawnedPOI;
    public GameObject collectableParticleEffect;
    public GameObject despawnParticleEffect;

    private void Awake()
    {
        collectableParticleEffect = Instantiate(collectableParticleEffect, transform);
        collectableParticleEffect.SetActive(false);
        despawnParticleEffect = Instantiate(despawnParticleEffect, transform);
        despawnParticleEffect.SetActive(false);
    }

    public void CleanUp()
    {
        collectableParticleEffect.transform.parent = transform;
        collectableParticleEffect.SetActive(false);
        despawnParticleEffect.transform.parent = transform;
        despawnParticleEffect.SetActive(false);
        Destroy(spawnedPOI);
    }

    public void SpawnPOI(POIVariable poi, SpawnSettings spawnSettings)
    {
        BezierSpline tmp = (BezierSpline)path.Value.Clone();
        Vector3 orthoVector;
        Vector3 spawnPosition = tmp.MoveLongDistance(spawnSettings.distance, out orthoVector);

        if (spawnPosition == null)
        {
            //TO-DO: Aggiungi richiesta spawn uno volta che chunk verr� istanziato
            print("There is not a chunk instantiated at that distance");
        }
        else
        {
            spawnPosition.y += 10;
            Vector3 groundedPos = CharacterRailMovement.GetGroundPosition(spawnPosition);
            groundedPos.y += spawnSettings.groundOffset;

            spawnedPOI = Instantiate(poi.gameObject, groundedPos, Quaternion.identity);
            spawnedPOI.transform.parent = transform;
            if (poi.isCollectable)
            {
                CollectPOI collectPOI = spawnedPOI.AddComponent<CollectPOI>();
                collectPOI.CollectableItem = (Collectable)poi;
                collectableParticleEffect.transform.position = spawnedPOI.transform.position;
                collectableParticleEffect.transform.parent = spawnedPOI.transform;
                collectPOI.collectableParticleEffect = collectableParticleEffect;
                collectableParticleEffect.SetActive(true);
            }
            else
            {
                spawnedPOI.transform.localPosition += orthoVector * 3f;
                EnemyAI enemyAi = spawnedPOI.GetComponent<EnemyAI>();
                //TO-DO: forse non � necessario fare cos�
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
            despawnParticleEffect.transform.position = spawnedPOI.transform.position;
            despawnParticleEffect.transform.parent = spawnedPOI.transform;
            despawnPOI.despawnParticleEffect = despawnParticleEffect;
            despawnParticleEffect.SetActive(false);
            despawnPOI.SetDespawnTimer(spawnSettings.TTL);

            //Destroy(spawnedPOI, spawnSettings.TTL);
        }

    }
}
