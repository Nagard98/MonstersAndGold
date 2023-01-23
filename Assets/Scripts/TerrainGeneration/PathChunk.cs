using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathChunk
{
    GameObject meshObject;
    MeshRenderer meshRenderer;
    MeshFilter meshFilter;
    MeshCollider meshCollider;

    private ChunkData chunkData;
    public Bounds bounds;

    private int _chunkIndex;

    public int Index { get { return _chunkIndex; } }

    public PathChunk(Vector2 coord, Material material, Transform parent, bool async=true)
    {
        
        meshObject = new GameObject("Path Chunk");
        meshRenderer = meshObject.AddComponent<MeshRenderer>();
        meshFilter = meshObject.AddComponent<MeshFilter>();
        meshCollider = meshObject.AddComponent<MeshCollider>();
        meshObject.AddComponent<TriggerSpawn>();

        meshRenderer.material = material;
        meshObject.transform.position = new Vector3(coord.x, 0, coord.y);
        meshObject.transform.parent = parent;

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
        Texture2D splatmap = new Texture2D(PathGenerator.pathChunkSize, PathGenerator.pathChunkSize);
        splatmap.SetPixels(chunkData.splatMapColors);
        splatmap.Apply();
        meshRenderer.material.SetTexture("_Mask", splatmap);

        for (int i = 0; i < chunkData.chunkMultiDetails.Length; i++)
        {
            DrawMeshInstancedDemo drmi = meshObject.AddComponent<DrawMeshInstancedDemo>();
            drmi.playerPosition = EndlessPath.pathGenerator.playerPosition;
            drmi.matrices = chunkData.chunkMultiDetails[i].Matrix;
            drmi.mesh = chunkData.chunkMultiDetails[i].DetailPrototype.prototype.GetComponent<MeshFilter>().sharedMesh;
            drmi.material = chunkData.chunkMultiDetails[i].DetailPrototype.prototype.GetComponent<MeshRenderer>().sharedMaterial;
        }

        for (int i = 0; i < chunkData.chunkTrees.Length; i++)
        {
            DrawMeshInstancedDemo drmi = meshObject.AddComponent<DrawMeshInstancedDemo>();
            drmi.playerPosition = EndlessPath.pathGenerator.playerPosition;
            drmi.matrices = chunkData.chunkTrees[i].Matrix;
            drmi.mesh = chunkData.chunkTrees[i].TreePrototype.prefab.GetComponent<MeshFilter>().sharedMesh;
            drmi.material = chunkData.chunkTrees[i].TreePrototype.prefab.GetComponent<MeshRenderer>().sharedMaterial;
        }
    }

    private void ApplyMeshData(MeshData meshData)
    {
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


    private static TreeInstance[] BuildTreeInstances(ChunkTrees chunkTrees, float[,,] splatmaps, int heightmapRes)
    {
        List<TreeInstance> tmpTreeInstances = new List<TreeInstance>();

        for (int i = 0; i < chunkTrees.Count; i++)
        {
            float x = MathUtils.Remap(chunkTrees.TreesPos[i].x, 0, heightmapRes, 0f, 1f);
            float z = MathUtils.Remap(chunkTrees.TreesPos[i].y, 0, heightmapRes, 0f, 1f);
            if (splatmaps[(int)chunkTrees.TreesPos[i].x, (int)chunkTrees.TreesPos[i].y, chunkTrees.TreeLayer[i]] == 1)
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

}

public struct ChunkTree
{
    public ChunkTree(TreePrototype treePrototype, Vector3[] treePos, int treeLayer, Matrix4x4[][] matrix)
    {
        TreePrototype = treePrototype;
        TreePos = treePos;
        TreeLayer = treeLayer;
        Matrix = matrix;
    }

    public TreePrototype TreePrototype { get; }
    public Vector3[] TreePos { get; }
    public int TreeLayer { get; }
    public Matrix4x4[][] Matrix { get; }

}

public struct ChunkDetails
{
    public ChunkDetails(DetailPrototype detailPrototype, Vector3[] detailPos, int detailLayer, Matrix4x4[][] matrix)
    {
        DetailPrototype = detailPrototype;
        DetailPos = detailPos;
        DetailLayer = detailLayer;
        Matrix = matrix;
    }

    public DetailPrototype DetailPrototype{ get; }
    public Vector3[] DetailPos { get; }
    public int DetailLayer { get; }
    public Matrix4x4[][] Matrix { get; }

}

