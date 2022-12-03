using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathGenerator : MonoBehaviour
{
    private List<GameObject> pathChunks;
    public Material terrainMaterial;

    public int concurrentChunkNumber;
    public int chunkXSize = 100;
    public int chunkZSize = 100;
    public float pathMaxWidth;
    public int numCurvesChunk;
    private int lastChunkIndex = 0;

    public GameObject tree1;
    public GameObject tree2;

    private GameObject chr;
    public float speed = 10f;

    public QuadraticBezier bezCurve;
    private static ChunkSettings chunkSettings;
    void Start()
    {
        chr = GameObject.CreatePrimitive(PrimitiveType.Cube);
        chr.name = "Character";
        chr.AddComponent<Rigidbody>();
        chr.AddComponent<Camera>();
        chr.GetComponent<Rigidbody>().isKinematic = true;

        numCurvesChunk = 5;
        concurrentChunkNumber = 5;
        chunkXSize = 300;
        chunkZSize = 100;
        pathMaxWidth = 100f;
        chunkSettings = new ChunkSettings(beelineLength: (float)chunkZSize, numCurves: numCurvesChunk);

        pathChunks = new List<GameObject>();

        for (int j = 0; j < concurrentChunkNumber; j++)
        {
            BuildChunk(chunkSettings);
        }
        bezCurve.Reset();

    }
    public static ChunkSettings ChunkBuildSettings{
        get { return chunkSettings; }
    }
    private void BuildChunk(ChunkSettings chunkSettings)
    {
        Vector3[] curveControlPoints = GenerateWaypoints(chunkSettings.BeelineLength, chunkSettings.NumCurves);

        if (lastChunkIndex == 0) { bezCurve = new QuadraticBezier(curveControlPoints, nSamples: 5); }
        else { bezCurve.AddPathWaypoints(curveControlPoints, lastChunkIndex, numCurvesChunk); }

        GenerateChunk(lastChunkIndex);
        lastChunkIndex += 1;
    }

    private Vector3[] GenerateWaypoints(float beelineLength, int numCurves)
    {
        float step = (beelineLength / (float)numCurves) / 2f;
        Vector3[] waypoints = new Vector3[numCurves * 3];

        for (int i = 0, c = 0, d=0; i < waypoints.Length; i++,c++,d++)
        {
            if (c == 3)
            {
                c = 0;
                d -= 1;
            }
            
            float horOffset = Mathf.PerlinNoise(.01f, (lastChunkIndex * chunkZSize) * .01f + step * d * .01f);
            float scaledOffset = MathUtils.Remap(horOffset, 0f, 1f, 0f, (float)pathMaxWidth);
            waypoints[i] = new Vector3(x: scaledOffset + ((chunkXSize - pathMaxWidth) / 2), y: 1, z: (lastChunkIndex * chunkZSize) + d * step);

        }
        return waypoints;
    }

    private void GenerateChunk(int chunkIndex)
    {
        Vector3[] vertices = new Vector3[(chunkXSize + 1) * (chunkZSize + 1)];
        Vector2[] uvs = new Vector2[vertices.Length];

        double chunkDist = bezCurve.GetPathChunkDist(chunkIndex, numCurvesChunk);
        double stepCurve = chunkDist / chunkZSize;
        stepCurve += 0.02f;

        bezCurve._buildArcDist = .0f;
        bezCurve._buildArcIndex = chunkIndex * numCurvesChunk;

        for (int z = 0, i = 0; z <= chunkZSize; z++)
        {
            
            Vector3 p = bezCurve.BuildAlong(z == 0 ? 0 : (float)stepCurve);

            for (int x = 0; x <= chunkXSize; x++)
            {
                Vector3 vertPos;

                float noiseVal = Mathf.PerlinNoise(x * 0.04f, ((lastChunkIndex * chunkZSize) + z) * 0.03f);
                float t = MathUtils.Remap(x, 0f, (float)chunkXSize, 0f, 1f);
                float pathT = MathUtils.Remap(p.x, 0f, (float)chunkXSize, 0f, 1f);
                float y = noiseVal - EasingFunctions.Custom(t + (0.5f - pathT)) * noiseVal;
                vertPos = new Vector3(x, y * 20, z);

                vertices[i] = vertPos;
                uvs[i] = new Vector2(x, z);
                i++;
            }
            
        }

        GameObject newChunk = new GameObject("chunk" + lastChunkIndex);
        newChunk.transform.parent = this.gameObject.transform;
        PathChunk chunkComponent = newChunk.AddComponent<PathChunk>();
        chunkComponent.InitChunk(vertices, uvs, chunkIndex, chunkXSize, chunkZSize, terrainMaterial);

        pathChunks.Add(newChunk);

        if (lastChunkIndex > 0) 
        {
            PathChunk first = pathChunks[pathChunks.Count - 2].GetComponent<PathChunk>();
            PathChunk second = pathChunks[pathChunks.Count - 1].GetComponent<PathChunk>();
            StitchMeshes(ref first, ref second);
        }
    }

    private void StitchMeshes(ref PathChunk firstMesh, ref PathChunk secondMesh)
    {
        Vector3[] firstVerts = firstMesh.Vertices;
        Vector3[] secondVerts = secondMesh.Vertices;

        for(int x = 0; x <= chunkXSize; x++)
        {
            float frs = firstVerts[((chunkXSize + 1) * (chunkZSize + 1)) - (2 * (chunkXSize + 1)) + x].y;
            float sec = secondVerts[chunkXSize + 1 + x].y;
            float avg = (frs + sec) / 2;
            secondVerts[x].y = avg;
            firstVerts[((chunkXSize + 1) * (chunkZSize + 1)) - (chunkXSize + 1) + x].y = avg;
        }

        firstMesh.Invoke("UpdateMesh",0);
        secondMesh.Invoke("UpdateMesh", 0);
    }

    void Update()
    {
        chr.transform.position = bezCurve.MoveAlong(speed * Time.deltaTime);
        chr.transform.forward = bezCurve.GetFirstDerivative();
    }


}
