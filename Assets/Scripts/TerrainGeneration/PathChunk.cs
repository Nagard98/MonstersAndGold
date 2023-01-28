using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Unity.AI.Navigation;
using Unity.AI.Navigation.Samples;

public class PathChunk
{
    GameObject meshObject;
    MeshRenderer meshRenderer;
    MeshFilter meshFilter;
    MeshCollider meshCollider;

    private ComputeShader meshPointHeightFinder;
    private ComputeBuffer positionBuffer;

    private ChunkData chunkData;
    public Bounds bounds;
    private Vector3Variable playerPosition;
    private PathChunksSet pathChunks;
    private Dictionary<int, Stack<GameObject>> instantiatedGameObjects;
    private Dictionary<int, Stack<GameObject>> gameObjectsPool;

    public int Index { get { return chunkData.chunkIndex; } }

    public PathChunk(Vector2 coord, Material material, Transform parent, Vector3Variable playerPosition, PathChunksSet pathChunks, ref Dictionary<int,Stack<GameObject>> gameObjectsPool, bool async=true)
    {
        meshPointHeightFinder = Resources.Load<ComputeShader>("PositionFinderSurface");

        meshObject = new GameObject("Path Chunk");
        meshRenderer = meshObject.AddComponent<MeshRenderer>();
        meshFilter = meshObject.AddComponent<MeshFilter>();
        meshCollider = meshObject.AddComponent<MeshCollider>();
        meshObject.AddComponent<NavMeshSourceTag>();
        meshObject.AddComponent<TriggerSpawn>();

        meshRenderer.material = material;
        meshObject.transform.position = new Vector3(coord.x, 0, coord.y);
        meshObject.transform.parent = parent;

        this.playerPosition = playerPosition;
        this.pathChunks = pathChunks;
        this.instantiatedGameObjects = new Dictionary<int, Stack<GameObject>>();
        this.gameObjectsPool = gameObjectsPool;

        if (async) BuildChunkAsync();
        else BuildChunkSync();

    }

    public Vector3[] MeshVertices { get { return meshFilter.mesh.vertices; } }

    internal float GetTerrainHeightAt(Vector3 spawnPosition)
    {
        int x = (int)spawnPosition.x;
        int y = (int)spawnPosition.z;
        return chunkData.heightMap[x, y];
    }

    internal Dictionary<int, Stack<GameObject>> ReleaseGameObjects(GameObject objectPool)
    {
        /*foreach(Stack<GameObject> gList in instantiatedGameObjects.Values)
        {
            foreach(GameObject gObj in gList)
            {
                gObj.transform.parent = objectPool.transform;
            }
        }*/

        return instantiatedGameObjects;
    }

    private void BuildChunkSync()
    {
        ApplyChunkData(EndlessPath.pathGenerator.RequestChunkDataSync());
        ApplyMeshData(EndlessPath.pathGenerator.RequestMeshDataSync(chunkData));
    }

    private void BuildChunkAsync()
    {
        EndlessPath.pathGenerator.RequestChunkDataAsync(onChunkDataReceived);
    }

    private void ApplyChunkData(ChunkData chunkData)
    {
        this.chunkData = chunkData;

        Texture2D splatmap = new Texture2D(PathGenerator.pathChunkSize, PathGenerator.pathChunkSize, TextureFormat.RGBA32, false);
        splatmap.wrapMode = TextureWrapMode.Clamp;
        splatmap.filterMode = FilterMode.Bilinear;
        splatmap.SetPixels(chunkData.splatMapColors);
        splatmap.Apply();

        Texture2D heightmap = new Texture2D(PathGenerator.pathChunkSize, PathGenerator.pathChunkSize, TextureFormat.RGBA32, false);
        heightmap.wrapMode = TextureWrapMode.Clamp;
        heightmap.filterMode = FilterMode.Bilinear;
        Color[] colors = new Color[PathGenerator.pathChunkSize * PathGenerator.pathChunkSize];
        for(int i=0;i< PathGenerator.pathChunkSize; i++)
        {
            for(int j=0;j< PathGenerator.pathChunkSize; j++)
            {
                colors[i * PathGenerator.pathChunkSize + j] = new Color((chunkData.heightMap[j,i]),0,0);
            }
        }
        heightmap.SetPixels(colors);
        heightmap.Apply();

        SetupGrassInstantiator(chunkData, heightmap, splatmap, playerPosition);

        meshRenderer.material.SetTexture("_Mask", splatmap);
        meshRenderer.material.SetTexture("_HeightMap", heightmap);

        for(int i = 0; i < chunkData.chunkTrees.Length; i++)
        {
            Vector3[] pos = chunkData.chunkTrees[i].TreePos;
            positionBuffer = new ComputeBuffer(pos.Length, sizeof(float) * 3);
            positionBuffer.SetData(pos);

            meshPointHeightFinder.SetBuffer(0, "_Vertices", positionBuffer);
            meshPointHeightFinder.SetTexture(0, "_HeightMap", heightmap);
            meshPointHeightFinder.SetTexture(0, "_SplatMap", splatmap);
            meshPointHeightFinder.SetFloat("_HeightMultiplier", EndlessPath.pathGenerator.meshHeightMultiplier);
            meshPointHeightFinder.SetFloat("_Dimension", PathGenerator.pathChunkSize - 1);
            meshPointHeightFinder.Dispatch(0, Mathf.CeilToInt(pos.Length / 64f), 1, 1);

            positionBuffer.GetData(pos);
            positionBuffer.Release();

            GameObject treePrefab = chunkData.chunkTrees[i].TreePrototype.prefab;
            int dictKey = treePrefab.name.GetHashCode();

            if (gameObjectsPool.Count == 0)
            {
                Stack<GameObject> tmpStack = new Stack<GameObject>(pos.Length);
                for (int j = 0; j < pos.Length; j++)
                {
                    if (pos[j] != Vector3.zero)
                    {
                        GameObject tmp = GameObject.Instantiate(chunkData.chunkTrees[i].TreePrototype.prefab, pos[j], Quaternion.identity);
                        tmp.transform.parent = EndlessPath.pool.transform;
                        tmpStack.Push(tmp);
                    }
                }
                instantiatedGameObjects.Add(dictKey, tmpStack);
            }
            else {
                Stack<GameObject> tmpStack = new Stack<GameObject>(pos.Length);
                int poolCount = gameObjectsPool[dictKey].Count;
                for (int j = 0; j < pos.Length && j < poolCount; j++) 
                {
                    if (pos[j] != Vector3.zero)
                    {
                        GameObject instancedGameObject = gameObjectsPool[dictKey].Pop();
                        instancedGameObject.transform.position = pos[j];
                        tmpStack.Push(instancedGameObject);
                    }
                }
                instantiatedGameObjects.Add(dictKey, tmpStack);
            }
            
        }

        

        for (int i = 0; i < chunkData.chunkMultiDetails.Length; i++)
        {
            Vector3[] pos = chunkData.chunkMultiDetails[i].DetailPos;
            positionBuffer = new ComputeBuffer(pos.Length, sizeof(float) * 3);
            positionBuffer.SetData(pos);

            meshPointHeightFinder.SetBuffer(0, "_Vertices", positionBuffer);
            meshPointHeightFinder.SetTexture(0, "_HeightMap", heightmap);
            meshPointHeightFinder.SetTexture(0, "_SplatMap", splatmap);
            meshPointHeightFinder.SetFloat("_HeightMultiplier", EndlessPath.pathGenerator.meshHeightMultiplier);
            meshPointHeightFinder.SetFloat("_Dimension", PathGenerator.pathChunkSize - 1);
            meshPointHeightFinder.Dispatch(0, Mathf.CeilToInt(pos.Length / 64f), 1, 1);

            positionBuffer.GetData(pos);
            positionBuffer.Release();

            GameObject detailPrefab = chunkData.chunkMultiDetails[i].DetailPrototype.prototype;
            int dictKey = detailPrefab.name.GetHashCode();

            if (gameObjectsPool.Count == 0)
            {
                Stack<GameObject> tmpStack = new Stack<GameObject>(pos.Length);
                for (int j = 0; j < pos.Length; j++)
                {
                    if (pos[j] != Vector3.zero)
                    {
                        GameObject tmp = GameObject.Instantiate(detailPrefab, pos[j], Quaternion.identity);
                        tmp.transform.parent = EndlessPath.pool.transform;
                        tmpStack.Push(tmp);
                    }
                }
                instantiatedGameObjects.Add(dictKey, tmpStack);
            }
            else
            {
                Stack<GameObject> tmpStack = new Stack<GameObject>(pos.Length);
                int poolCount = gameObjectsPool[dictKey].Count;
                for (int j = 0; j < pos.Length && j < poolCount; j++)
                {
                    if (pos[j] != Vector3.zero)
                    {
                        GameObject instancedGameObject = gameObjectsPool[dictKey].Pop();
                        instancedGameObject.transform.position = pos[j];
                        tmpStack.Push(instancedGameObject);
                    }
                }
                instantiatedGameObjects.Add(dictKey, tmpStack);
            }
        }
    }

    private void ApplyMeshData(MeshData meshData)
    {
        meshData.DisplaceMesh(meshRenderer.material);
        MakeStitch(meshData);
        meshFilter.mesh = meshData.CreateMesh();
        meshCollider.sharedMesh = meshFilter.mesh;
        bounds = meshCollider.bounds;

        pathChunks.Add(this);
    }

    private void MakeStitch(MeshData meshData)
    {
        if (pathChunks.Items.Count > 0) meshData.stitchTo = pathChunks.Get(EndlessPath.pathGenerator.LastIndex - 2).MeshVertices;
        if (meshData.stitchTo != null) meshData.vertices = MeshGenerator.StitchMeshes(meshData.vertices, meshData.stitchTo, meshData.meshWidth);
    }

    private void SetupGrassInstantiator(ChunkData chunkData, Texture2D heightmap, Texture2D splatmap, Vector3Variable playerPosition)
    {
        GrassInstantiator grassInstantiator = meshObject.AddComponent<GrassInstantiator>();
        grassInstantiator.grassSettings = chunkData.grassSettings;
        grassInstantiator.heightMap = heightmap;
        grassInstantiator.splatMap = splatmap;
        grassInstantiator.playerPosition = playerPosition;

        float zOffset = chunkData.chunkIndex * (PathGenerator.pathChunkSize - 1) + ((PathGenerator.pathChunkSize - 1) / 2f);
        float xOffset = (PathGenerator.pathChunkSize - 1) / 2f;
        grassInstantiator.offset = new Vector2(xOffset, zOffset);
    }

    void onChunkDataReceived(ChunkData chunkData)
    {
        ApplyChunkData(chunkData);
        EndlessPath.pathGenerator.RequestMeshDataAsync(onMeshDataReceived, chunkData);
    }

    void onMeshDataReceived(MeshData meshData)
    {
        ApplyMeshData(meshData);
    }

    public static DetailPrototype BuildDetailProto(Detail detail)
    {
        DetailPrototype tmpDetailPrototype = new DetailPrototype();

        DetailPrototype tmpDetail = new DetailPrototype();
        tmpDetail.prototype = detail.gameObject;
        tmpDetail.usePrototypeMesh = true;
        tmpDetail.renderMode = DetailRenderMode.VertexLit;
        tmpDetail.useInstancing = true;
        tmpDetail.maxHeight = 1.1f;
        tmpDetail.maxWidth = 1;
        tmpDetail.minHeight = 0.9f;
        tmpDetail.minWidth = 0.7f;
        tmpDetailPrototype = tmpDetail;
        
        return tmpDetailPrototype;
    }

    public void Destroy()
    {
        GameObject.Destroy(meshObject);
    }

}

