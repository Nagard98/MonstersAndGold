using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

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

    public int Index { get { return chunkData.chunkIndex; } }

    public PathChunk(Vector2 coord, Material material, Transform parent, Vector3Variable playerPosition, bool async=true)
    {
        meshPointHeightFinder = Resources.Load<ComputeShader>("PositionFinderSurface");

        meshObject = new GameObject("Path Chunk");
        meshRenderer = meshObject.AddComponent<MeshRenderer>();
        meshFilter = meshObject.AddComponent<MeshFilter>();
        meshCollider = meshObject.AddComponent<MeshCollider>();
        meshObject.AddComponent<TriggerSpawn>();

        meshRenderer.material = material;
        meshObject.transform.position = new Vector3(coord.x, 0, coord.y);
        meshObject.transform.parent = parent;

        this.playerPosition = playerPosition;

        if (async) EndlessPath.pathGenerator.RequestChunkData(onChunkDataReceived);
        else BuildChunkSync();

    }

    internal float GetTerrainHeightAt(Vector3 spawnPosition)
    {
        int x = (int)spawnPosition.x;
        int y = (int)spawnPosition.z;
        return chunkData.heightMap[x, y];
    }

    private void BuildChunkSync()
    {
        chunkData = EndlessPath.pathGenerator.RequestChunkDataSync();
        ApplyChunkData(chunkData);
        MeshData meshData = EndlessPath.pathGenerator.RequestMeshDataSync(chunkData);
        ApplyMeshData(meshData);
    }

    private void ApplyChunkData(ChunkData chunkData)
    {
        Texture2D splatmap = new Texture2D(PathGenerator.pathChunkSize, PathGenerator.pathChunkSize, TextureFormat.RGBA32, false);
        splatmap.wrapMode = TextureWrapMode.Clamp;
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
            meshPointHeightFinder.SetFloat("_HeightMultiplier", EndlessPath.pathGenerator.meshHeightMultiplier);
            meshPointHeightFinder.SetFloat("_Dimension", PathGenerator.pathChunkSize - 1);
            meshPointHeightFinder.Dispatch(0, Mathf.CeilToInt(pos.Length / 64f), 1, 1);

            positionBuffer.GetData(pos);
            positionBuffer.Release();

            for (int j = 0; j < pos.Length; j++)
            {
                GameObject tmp = GameObject.Instantiate(chunkData.chunkTrees[i].TreePrototype.prefab, pos[j], Quaternion.identity);
                tmp.transform.parent = meshObject.transform;
            }
        }

        

        for (int i = 0; i < chunkData.chunkMultiDetails.Length; i++)
        {
            Vector3[] pos = chunkData.chunkMultiDetails[i].DetailPos;
            positionBuffer = new ComputeBuffer(pos.Length, sizeof(float) * 3);
            positionBuffer.SetData(pos);

            meshPointHeightFinder.SetBuffer(0, "_Vertices", positionBuffer);
            meshPointHeightFinder.SetTexture(0, "_HeightMap", heightmap);
            meshPointHeightFinder.SetFloat("_HeightMultiplier", EndlessPath.pathGenerator.meshHeightMultiplier);
            meshPointHeightFinder.SetFloat("_Dimension", PathGenerator.pathChunkSize - 1);
            meshPointHeightFinder.Dispatch(0, Mathf.CeilToInt(pos.Length / 64f), 1, 1);

            positionBuffer.GetData(pos);
            positionBuffer.Release();

            for (int j = 0; j < pos.Length; j++)
            {
                GameObject tmp = GameObject.Instantiate(chunkData.chunkMultiDetails[i].DetailPrototype.prototype, pos[j], Quaternion.identity);
                tmp.transform.parent = meshObject.transform;
            }
        }

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

    private void ApplyMeshData(MeshData meshData)
    {
        meshData.DisplaceMesh(meshRenderer.material);
        meshFilter.mesh = meshData.CreateMesh();
        meshCollider.sharedMesh = meshFilter.mesh;
        bounds = meshCollider.bounds;
    }

    void onChunkDataReceived(ChunkData chunkData)
    {
        ApplyChunkData(chunkData);
        EndlessPath.pathGenerator.RequestMeshData(onMeshDataReceived, chunkData);
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

