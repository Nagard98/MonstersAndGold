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
        int chunkIndex;
        Vector3 spawnPosition = tmp.MoveLongDistance(spawnSettings.distance, out chunkIndex);
        PathChunk pc = pathChunks.Get(chunkIndex);
        if (pc == null)
        {
            print("Variable: " + poi + "   Its null at index: " + chunkIndex);
        }
        else
        {
            float height = pc.GetTerrainHeightAt(spawnPosition) * EndlessPath.pathGenerator.meshHeightMultiplier;
            spawnPosition.y = height + spawnSettings.groundOffset;

            GameObject spawnedPOI = Instantiate(poi.gameObject, spawnPosition, Quaternion.identity);
            if (poi.isCollectable)
            {
                CollectPOI collectPOI = spawnedPOI.AddComponent<CollectPOI>();
                collectPOI.CollectableItem = (Collectable)poi;
            }
            UnloadPOI unloadPOI = spawnedPOI.AddComponent<UnloadPOI>();
            unloadPOI.UnloadableObject = poi;

            Destroy(spawnedPOI, spawnSettings.TTL);
        }

    }
}
