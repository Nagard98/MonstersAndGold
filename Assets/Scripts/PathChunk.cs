using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Terrain))]
[RequireComponent(typeof(TerrainCollider))]

public class PathChunk : MonoBehaviour
{
    private int _chunkIndex;
    private int _chunkXSize;
    private int _chunkZSize;
    private GameObject spawnCollider;

    private Material _material;
    private Terrain _terrain;
    private TerrainData terrainData;
    private TerrainLayer[] _terrainLayers;
    private ChunkTrees _chunkTrees;

    private float[,] _heightmap;
    private float[,,] _splatmaps;

    void Start()
    {
        terrainData = new TerrainData();

        terrainData.heightmapResolution = (_chunkXSize*4) + 1;
        terrainData.SetHeights(0, 0, _heightmap);

        terrainData.size = new Vector3(x: _chunkXSize, z: _chunkZSize, y: 20);

        terrainData.terrainLayers = _terrainLayers;

        terrainData.treePrototypes = _chunkTrees.TreePrototypes;
        TreeInstance[] treeInstances = BuildTreeInstances(_chunkTrees, _heightmap, _chunkXSize*4);
        terrainData.SetTreeInstances(treeInstances, true);
        
        terrainData.alphamapResolution = (_chunkXSize*4) + 1;
        terrainData.SetAlphamaps(0, 0, _splatmaps);

        _terrain.terrainData = terrainData;
        _terrain.materialTemplate = _material;
        _terrain.allowAutoConnect = true;

        GetComponent<TerrainCollider>().terrainData = terrainData;

        spawnCollider = BuildSpawnCollider(this.transform, _chunkXSize, _chunkIndex);

        transform.position = new Vector3(x: 0f, y: 0f, z: _chunkIndex * (_chunkZSize));
    }

    private static GameObject BuildSpawnCollider(Transform parent, int colliderWidth, int colliderIndex)
    {
        GameObject tmpSpawnCollider = new GameObject();
        tmpSpawnCollider.transform.parent = parent;
        tmpSpawnCollider.AddComponent<TriggerSpawn>();
        BoxCollider box = tmpSpawnCollider.AddComponent<BoxCollider>();
        box.transform.position = new Vector3(x: colliderWidth / 2, z: colliderWidth, y: 1f);
        box.isTrigger = true;
        box.size = new Vector3(x: colliderWidth, z: 5f, y: 10f);
        box.name = "collider" + colliderIndex;

        return tmpSpawnCollider;
    }

    private static TreeInstance[] BuildTreeInstances(ChunkTrees chunkTrees, float[,] heightmap, int heightmapRes)
    {
        List<TreeInstance> tmpTreeInstances = new List<TreeInstance>();

        for (int i = 0; i < chunkTrees.Count; i++)
        {
            float x = MathUtils.Remap(chunkTrees.TreesPos[i].x, 0, heightmapRes, 0f, 1f);
            float z = MathUtils.Remap(chunkTrees.TreesPos[i].y, 0, heightmapRes, 0f, 1f);
            if (heightmap[(int)chunkTrees.TreesPos[i].x, (int)chunkTrees.TreesPos[i].y] > 0.005)
            {
                TreeInstance tmpTreeInstance = new TreeInstance();
                tmpTreeInstance.prototypeIndex = chunkTrees.TreesIndex[i];
                tmpTreeInstance.position = new Vector3(x: z, z: x, y: 0f);
                tmpTreeInstance.widthScale = 1;
                tmpTreeInstance.heightScale = 1;
                tmpTreeInstances.Add(tmpTreeInstance);
            }
        }

        return tmpTreeInstances.ToArray();
    }

    public Terrain InitChunk(int chunkIndex, int chunkXSize, int chunkZSize, float[,] heightmap, float[,,] splatmaps, TerrainLayer[] terrainLayers, ChunkTrees chunkTrees, Material terrainMaterial)
    {
        _chunkIndex = chunkIndex;
        _chunkXSize = chunkXSize;
        _chunkZSize = chunkZSize;
        _heightmap = heightmap;
        _splatmaps = splatmaps;
        _material = terrainMaterial;
        _chunkTrees = chunkTrees;
        _terrainLayers = terrainLayers;
        _terrain = GetComponent<Terrain>();

        return _terrain;
    }

    void TriggerChunkBuild()
    {
        SendMessageUpwards("BuildChunk", PathGenerator.ChunkBuildSettings);
    }
}

public struct ChunkSettings
{
    public ChunkSettings(int numCurves, float beelineLength)
    {
        NumCurves = numCurves;
        BeelineLength = beelineLength;
    }

    public int NumCurves { get; }
    public float BeelineLength { get; }

    public override string ToString() => $"({NumCurves}, {BeelineLength})";
}