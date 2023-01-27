using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.AI.Navigation.Samples;
using UnityEngine;
using UnityEngine.Events;

public class EndlessPath : MonoBehaviour
{
    public static PathGenerator pathGenerator;
    public static LocalNavMeshBuilder localNavMeshBuilder;
    [SerializeField]
    private PathChunksSet pathChunks;
    public Vector3Variable playerPosition;
    public Material pathMaterial;
    public UnityEvent GeneratedPath;

    public void Start()
    {
        localNavMeshBuilder = GetComponent<LocalNavMeshBuilder>();
        pathGenerator = FindObjectOfType<PathGenerator>();
        foreach (SplatHeight splat in pathGenerator.splatHeights)
        {
            pathMaterial.SetTexture("_MainTex" + splat.layerIndex, splat.texture);
            pathMaterial.SetTextureScale("_MainTex" + splat.layerIndex, new Vector2(80, 80));
        }
        StartCoroutine(test());
    }

    IEnumerator test()
    {
        for (int i = 0; i < 10; i++)
        {
            yield return null;
            InitNewChunk(false);
        }
        GeneratedPath.Invoke();
    }

    public void InitNewChunk(bool async=true)
    {
        pathGenerator.offset = new Vector2(0, pathGenerator.Index * (PathGenerator.pathChunkSize - 1));
        pathChunks.Add(new PathChunk(pathGenerator.offset, pathMaterial, transform, playerPosition, async));
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