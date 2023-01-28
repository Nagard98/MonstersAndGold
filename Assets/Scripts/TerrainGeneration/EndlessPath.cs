using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.AI.Navigation.Samples;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

public class EndlessPath : MonoBehaviour
{
    public static PathGenerator pathGenerator;
    public static LocalNavMeshBuilder localNavMeshBuilder;
    [SerializeField]
    private PathChunksSet pathChunks;
    public Vector3Variable playerPosition;
    public Material pathMaterial;
    public UnityEvent GeneratedPath;

    public static GameObject pool;
    private Dictionary<int, Stack<GameObject>> poolGameObjects;
    private Dictionary<int, Stack<GameObject>> test1;

    public void Start()
    {
        pool = new GameObject("GameObjectPool");
        pool.transform.position = Vector3.zero;
        poolGameObjects = new Dictionary<int, Stack<GameObject>>();

        localNavMeshBuilder = GetComponent<LocalNavMeshBuilder>();
        pathGenerator = FindObjectOfType<PathGenerator>();
        foreach (SplatHeight splat in pathGenerator.splatHeights)
        {
            pathMaterial.SetTexture("_MainTex" + splat.layerIndex, splat.texture);
            pathMaterial.SetTextureScale("_MainTex" + splat.layerIndex, new Vector2(80, 80));
        }
        StartCoroutine(test());
    }

    public void BuildAdditionalChunk()
    {
        Dictionary<int, Stack<GameObject>> releasedGameObjects = DestroyFirstChunk();
        foreach (int key in releasedGameObjects.Keys)
        {
            Stack<GameObject> currentPoolKeyed;
            bool isKeyPresent = poolGameObjects.TryGetValue(key,out currentPoolKeyed);
            poolGameObjects[key] = (isKeyPresent) ? new Stack<GameObject>(currentPoolKeyed.Concat(releasedGameObjects[key])) : releasedGameObjects[key];
        }
        InitNewChunk();
    }

    IEnumerator test()
    {
        for (int i = 0; i < 5; i++)
        {
            yield return null;
            InitNewChunk(async: false);
        }
        GeneratedPath.Invoke();
    }

    public void InitNewChunk(bool async=true)
    {
        pathGenerator.offset = new Vector2(0, pathGenerator.LastIndex * (PathGenerator.pathChunkSize - 1));
        new PathChunk(pathGenerator.offset, pathMaterial, transform, playerPosition, pathChunks, ref poolGameObjects, async);
    }

    public Dictionary<int,Stack<GameObject>> DestroyFirstChunk()
    {
        PathChunk pc = pathChunks.Get(pathGenerator.FirstIndex);
        Dictionary<int, Stack<GameObject>> releasedGameObjects = pc.ReleaseGameObjects(pool);
        pathChunks.Remove(pathGenerator.FirstIndex);
        pc.Destroy();
        pathGenerator.FirstIndex += 1;

        return releasedGameObjects;
    }

    public void OnDisable()
    {
        pathChunks.Destroy();
    }
    
    public void OnEnable()
    {
        if (pathGenerator != null)
        {
            StartCoroutine(test());
        }
    }

}


//TO-DO: move here chunk settings struct