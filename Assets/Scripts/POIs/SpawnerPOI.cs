using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

//When the event is called, spawns the specified POI
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
            print("There is not a chunk instantiated at that distance");
        }
        else
        {
            //finds the height of the mesh at a certain point
            spawnPosition.y += 10;
            Vector3 groundedPos = CharacterRailMovement.GetGroundPosition(spawnPosition);
            groundedPos.y += spawnSettings.groundOffset;

            spawnedPOI = Instantiate(poi.gameObject, groundedPos, Quaternion.identity);
            spawnedPOI.transform.parent = transform;
            if (poi.isCollectable)
            {
                SetupPOICollect(poi);
            }
            else
            {
                SetupPOIEnemy(poi, orthoVector);
            }

            SetupPOIDespawn(poi, spawnSettings.TTL);
        }

    }
    private void SetupPOIEnemy(POIVariable poi, Vector3 orthoVector)
    {
        spawnedPOI.transform.localPosition += orthoVector * 3f;
        Vector3 ortho2 = Vector3.Cross(orthoVector, Vector3.up).normalized;
        EnemyAI enemyAi = spawnedPOI.GetComponent<EnemyAI>();

        //Creates small navigation path on the side of the road
        Vector3[] navPoints = new Vector3[2];
        navPoints[0] = spawnedPOI.transform.position + (ortho2 * 5);
        navPoints[1] = spawnedPOI.transform.position - (ortho2 * 5);

        enemyAi.NavPoints = navPoints;
        EndlessPath.localNavMeshBuilder.m_Tracked = spawnedPOI.transform;
    }

    private void SetupPOICollect(POIVariable poi)
    {
        CollectPOI collectPOI = spawnedPOI.AddComponent<CollectPOI>();
        collectPOI.CollectableItem = (Collectable)poi;
        collectableParticleEffect.transform.position = spawnedPOI.transform.position;
        collectableParticleEffect.transform.parent = spawnedPOI.transform;
        collectPOI.collectableParticleEffect = collectableParticleEffect;
        collectableParticleEffect.SetActive(true);
    }


    private void SetupPOIDespawn(POIVariable poi, float ttl) {
        DespawnPOI despawnPOI = spawnedPOI.AddComponent<DespawnPOI>();
        despawnPOI.DespawnableObject = poi;
        despawnParticleEffect.transform.position = spawnedPOI.transform.position;
        despawnParticleEffect.transform.parent = spawnedPOI.transform;
        despawnPOI.despawnParticleEffect = despawnParticleEffect;
        despawnParticleEffect.SetActive(false);

        despawnPOI.SetDespawnTimer(ttl);
    }
}
