using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.AI.Navigation.Samples;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

//Manages the continuous build of the path chunks
public class EndlessPath : MonoBehaviour
{
    public static PathGenerator pathGenerator;
    public static LocalNavMeshBuilder localNavMeshBuilder;
    [SerializeField]
    private PathChunksSet _pathChunks;
    public int concurrentChunks;
    public Vector3Variable playerPosition;
    public Material pathMaterial;

    public UnityEvent GeneratedPath;

    //Uses a pool for the trees/details so that it is not necessary to make new instances on chunk build
    public static GameObject pool;
    private Dictionary<int, Stack<GameObject>> _poolGameObjects;

    private void Awake()
    {
        localNavMeshBuilder = GetComponent<LocalNavMeshBuilder>();
        _poolGameObjects = new Dictionary<int, Stack<GameObject>>();
    }

    private void Start()
    {
        pathGenerator = FindObjectOfType<PathGenerator>();
        StartUp();
    }

    public void StartUp()
    {
        pool = new GameObject("GameObjectPool");
        pool.transform.position = Vector3.zero;
        _poolGameObjects = new Dictionary<int, Stack<GameObject>>();

        foreach (SplatHeight splat in pathGenerator.splatHeights)
        {
            pathMaterial.SetTexture("_MainTex" + splat.layerIndex, splat.texture);
            pathMaterial.SetTextureScale("_MainTex" + splat.layerIndex, new Vector2(80, 80));
        }
        StartCoroutine(InitFirstChunks());
    }

    public void BuildAdditionalChunk()
    {
        Dictionary<int, Stack<GameObject>> releasedGameObjects = DestroyFirstChunk();
        foreach (int key in releasedGameObjects.Keys)
        {
            Stack<GameObject> currentPoolKeyed;
            bool isKeyPresent = _poolGameObjects.TryGetValue(key,out currentPoolKeyed);
            _poolGameObjects[key] = (isKeyPresent) ? new Stack<GameObject>(currentPoolKeyed.Concat(releasedGameObjects[key])) : releasedGameObjects[key];
        }
        InitNewChunk();
    }

    IEnumerator InitFirstChunks()
    {
        for (int i = 0; i < concurrentChunks; i++)
        {
            yield return null;
            InitNewChunk(async: false);
        }
        GeneratedPath.Invoke();
    }

    public void InitNewChunk(bool async=true)
    {
        pathGenerator.offset = new Vector2(0, pathGenerator.LastIndex * (PathGenerator.pathChunkSize - 1));
        new PathChunk(pathGenerator.offset, pathMaterial, transform, playerPosition, _pathChunks, ref _poolGameObjects, async);
    }

    public Dictionary<int,Stack<GameObject>> DestroyFirstChunk()
    {
        PathChunk pc = _pathChunks.Get(pathGenerator.FirstIndex);
        Dictionary<int, Stack<GameObject>> releasedGameObjects = pc.ReleaseGameObjects(pool);
        _pathChunks.Remove(pathGenerator.FirstIndex);
        pc.Destroy();
        pathGenerator.FirstIndex += 1;

        return releasedGameObjects;
    }

    public void CleanUp()
    {
        _pathChunks.Destroy();
        playerPosition.Value = Vector3.zero;
        Destroy(pool);
        _poolGameObjects.Clear();
        pathGenerator.CleanUp();
    }

    public void OnDisable()
    {
        _pathChunks.Destroy();
    }
    

}


//TO-DO: move here chunk settings struct