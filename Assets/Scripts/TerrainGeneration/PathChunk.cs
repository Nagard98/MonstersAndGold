using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Unity.AI.Navigation;
using Unity.AI.Navigation.Samples;

public class PathChunk
{
    private GameObject _meshObject;
    private MeshRenderer _meshRenderer;
    private MeshFilter _meshFilter;
    private MeshCollider _meshCollider;

    private ComputeShader _meshPointHeightFinder;
    private ComputeBuffer _positionBuffer;

    private ChunkData _chunkData;
    public Bounds bounds;
    private Vector3Variable _playerPosition;
    private PathChunksSet _pathChunks;
    private Dictionary<int, Stack<GameObject>> _instantiatedGameObjects;
    private Dictionary<int, Stack<GameObject>> _gameObjectsPool;

    public int Index { get { return _chunkData.chunkIndex; } }

    public PathChunk(Vector2 coord, Material material, Transform parent, Vector3Variable playerPosition, PathChunksSet pathChunks, ref Dictionary<int,Stack<GameObject>> gameObjectsPool, bool async=true)
    {
        _meshPointHeightFinder = Resources.Load<ComputeShader>("PositionFinderSurface");

        _meshObject = new GameObject("Path Chunk");
        _meshRenderer = _meshObject.AddComponent<MeshRenderer>();
        _meshFilter = _meshObject.AddComponent<MeshFilter>();
        _meshCollider = _meshObject.AddComponent<MeshCollider>();
        _meshObject.AddComponent<NavMeshSourceTag>();
        _meshObject.AddComponent<TriggerSpawn>();

        _meshRenderer.material = material;
        _meshObject.transform.position = new Vector3(coord.x, 0, coord.y);
        _meshObject.transform.parent = parent;

        this._playerPosition = playerPosition;
        this._pathChunks = pathChunks;
        this._instantiatedGameObjects = new Dictionary<int, Stack<GameObject>>();
        this._gameObjectsPool = gameObjectsPool;

        if (async) BuildChunkAsync();
        else BuildChunkSync();

    }

    public Vector3[] MeshVertices { get { return _meshFilter.mesh.vertices; } }

    internal float GetTerrainHeightAt(Vector3 spawnPosition)
    {
        int x = (int)spawnPosition.x;
        int y = (int)spawnPosition.z;
        return _chunkData.heightMap[x, y];
    }

    internal Dictionary<int, Stack<GameObject>> ReleaseGameObjects(GameObject objectPool)
    {
        return _instantiatedGameObjects;
    }

    private void BuildChunkSync()
    {
        ApplyChunkData(EndlessPath.pathGenerator.RequestChunkDataSync());
        ApplyMeshData(EndlessPath.pathGenerator.RequestMeshDataSync(_chunkData));
    }

    private void BuildChunkAsync()
    {
        EndlessPath.pathGenerator.RequestChunkDataAsync(onChunkDataReceived);
    }

    private void ApplyChunkData(ChunkData chunkData)
    {
        this._chunkData = chunkData;

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

        SetupGrassInstantiator(chunkData, heightmap, splatmap, _playerPosition);

        _meshRenderer.material.SetTexture("_Mask", splatmap);
        _meshRenderer.material.SetTexture("_HeightMap", heightmap);

        BuildTrees(chunkData, heightmap, splatmap);
        BuildDetails(chunkData, heightmap, splatmap);       
    }

    private void RunComputeShaderHeightFinder(Vector3[] pos, ChunkData chunkData, Texture2D heightmap, Texture2D splatmap)
    {
        _positionBuffer = new ComputeBuffer(pos.Length, sizeof(float) * 3);
        _positionBuffer.SetData(pos);

        _meshPointHeightFinder.SetBuffer(0, "_Vertices", _positionBuffer);
        _meshPointHeightFinder.SetTexture(0, "_HeightMap", heightmap);
        _meshPointHeightFinder.SetTexture(0, "_SplatMap", splatmap);
        _meshPointHeightFinder.SetFloat("_HeightMultiplier", EndlessPath.pathGenerator.meshHeightMultiplier);
        _meshPointHeightFinder.SetFloat("_Dimension", PathGenerator.pathChunkSize - 1);
        _meshPointHeightFinder.Dispatch(0, Mathf.CeilToInt(pos.Length / 64f), 1, 1);

        _positionBuffer.GetData(pos);
        _positionBuffer.Release();
    }

    //Creates chunk details either by instantiating new ones or by taking from pool
    private void BuildDetails(ChunkData chunkData, Texture2D heightmap, Texture2D splatmap)
    {
        for (int i = 0; i < chunkData.chunkMultiDetails.Length; i++)
        {
            Vector3[] pos = chunkData.chunkMultiDetails[i].DetailPos;

            RunComputeShaderHeightFinder(pos, chunkData, heightmap, splatmap);

            GameObject detailPrefab = chunkData.chunkMultiDetails[i].DetailPrototype.prototype;
            int dictKey = detailPrefab.name.GetHashCode();

            //If there are no objects in the pool instantiate them
            if (_gameObjectsPool.Count == 0)
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
                _instantiatedGameObjects.Add(dictKey, tmpStack);
            }
            //Otherwise take necessary object instances from the pool
            else
            {
                Stack<GameObject> tmpStack = new Stack<GameObject>(pos.Length);
                int poolCount = _gameObjectsPool[dictKey].Count;
                for (int j = 0; j < pos.Length && j < poolCount; j++)
                {
                    if (pos[j] != Vector3.zero)
                    {
                        GameObject instancedGameObject = _gameObjectsPool[dictKey].Pop();
                        instancedGameObject.transform.position = pos[j];
                        tmpStack.Push(instancedGameObject);
                    }
                }
                _instantiatedGameObjects.Add(dictKey, tmpStack);
            }
        }
    }

    //Creates chunk trees either by instantiating new ones or by taking from pool
    private void BuildTrees(ChunkData chunkData, Texture2D heightmap, Texture2D splatmap)
    {
        for (int i = 0; i < chunkData.chunkTrees.Length; i++)
        {
            Vector3[] pos = chunkData.chunkTrees[i].TreePos;

            RunComputeShaderHeightFinder(pos, chunkData, heightmap, splatmap);

            GameObject treePrefab = chunkData.chunkTrees[i].TreePrototype.prefab;
            int dictKey = treePrefab.name.GetHashCode();

            //If there are no objects in the pool instantiate them
            if (_gameObjectsPool.Count == 0)
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
                _instantiatedGameObjects.Add(dictKey, tmpStack);
            }
            //Otherwise take necessary object instances from the pool
            else
            {
                Stack<GameObject> tmpStack = new Stack<GameObject>(pos.Length);
                int poolCount = _gameObjectsPool[dictKey].Count;
                for (int j = 0; j < pos.Length && j < poolCount; j++)
                {
                    if (pos[j] != Vector3.zero)
                    {
                        GameObject instancedGameObject = _gameObjectsPool[dictKey].Pop();
                        instancedGameObject.transform.position = pos[j];
                        tmpStack.Push(instancedGameObject);
                    }
                }
                _instantiatedGameObjects.Add(dictKey, tmpStack);
            }

        }
    }

    private void ApplyMeshData(MeshData meshData)
    {
        meshData.DisplaceMesh(_meshRenderer.material);
        MakeStitch(meshData);
        _meshFilter.mesh = meshData.CreateMesh();
        _meshCollider.sharedMesh = _meshFilter.mesh;
        bounds = _meshCollider.bounds;

        _pathChunks.Add(this);
    }

    private void MakeStitch(MeshData meshData)
    {
        if (_pathChunks.Items.Count > 0) meshData.stitchTo = _pathChunks.Get(EndlessPath.pathGenerator.LastIndex - 2).MeshVertices;
        if (meshData.stitchTo != null) meshData.vertices = MeshGenerator.StitchMeshes(meshData.vertices, meshData.stitchTo, meshData.meshWidth);
    }

    private void SetupGrassInstantiator(ChunkData chunkData, Texture2D heightmap, Texture2D splatmap, Vector3Variable playerPosition)
    {
        GrassInstantiator grassInstantiator = _meshObject.AddComponent<GrassInstantiator>();
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
        GameObject.Destroy(_meshObject);
    }

}

