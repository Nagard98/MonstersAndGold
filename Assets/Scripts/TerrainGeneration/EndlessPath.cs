using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

public class EndlessPath : MonoBehaviour
{
    public static PathGenerator pathGenerator;
    //List<PathChunk> pathChunks;
    [SerializeField]
    private PathChunksSet pathChunks;
    public Material pathMaterial;
    public UnityEvent GeneratedPath;

    private void Start()
    {
        pathGenerator = FindObjectOfType<PathGenerator>();
        //pathChunks = new List<PathChunk>();

        StartCoroutine(test());
    }

    IEnumerator test()
    {
        yield return null;
        for (int i = 0; i < 5; i++)
        {
            InitNewChunk(false);
        }
        GeneratedPath.Invoke();
        /*int i = 0;
        while (true)
        {
            pathGenerator.offset = new Vector2(0, i * (PathGenerator.pathChunkSize - 1));
            pathChunks.Add(new PathChunk(pathGenerator.offset, pathMaterial, transform));
            i++;
            yield return new WaitForSeconds(8);
        }*/

    }

    public void InitNewChunk(bool async=true)
    {
        pathGenerator.offset = new Vector2(0, pathGenerator.Index * (PathGenerator.pathChunkSize - 1));
        pathChunks.Add(new PathChunk(pathGenerator.offset, pathMaterial, transform, async));
    }

    
}


//TO-DO: move here chunk settings struct