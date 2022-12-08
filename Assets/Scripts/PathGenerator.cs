using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathGenerator : MonoBehaviour
{
    private List<PathChunk> pathChunks;

    public TerrainLayer[] terrainLayers;
    public GameObject[] trees;
    public Material material;

    public int concurrentChunkNumber;
    public int chunkXSize;
    public int chunkZSize;
    public float pathMaxWidth;
    public int numCurvesChunk;
    private int lastChunkIndex;
    private float[,] lastHeightmap;

    private GameObject chr;
    public float speed = 10f;

    public BezierSpline bezCurve;
    private static ChunkSettings chunkSettings;

    void Start()
    {
        chr = GameObject.CreatePrimitive(PrimitiveType.Cube);
        chr.name = "Character";
        chr.AddComponent<Rigidbody>();
        chr.AddComponent<Camera>();
        chr.GetComponent<Rigidbody>().isKinematic = true;

        numCurvesChunk = 40;
        concurrentChunkNumber = 5;
        chunkXSize = 256;
        chunkZSize = 256;
        pathMaxWidth = 100f;
        lastChunkIndex = 0;
        chunkSettings = new ChunkSettings(beelineLength: (float)chunkZSize, numCurves: numCurvesChunk);
        pathChunks = new List<PathChunk>();

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

        if (lastChunkIndex == 0) { bezCurve = new BezierSpline(curveControlPoints, nSamples: 10); }
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
            float scaledOffset = MathUtils.Remap(horOffset, 0f, 1f, 0f, (float)(pathMaxWidth));
            waypoints[i] = new Vector3(x: scaledOffset + (((chunkXSize) - (pathMaxWidth)) / 2), y: 1, z: (lastChunkIndex * (chunkZSize)) + d * step);

        }
        return waypoints;
    }

    private void GenerateChunk(int chunkIndex)
    {

        float[,] heightmap = new float[((chunkXSize*4) + 1), ((chunkZSize*4) + 1)];
        float[,,] splatmaps = new float[((chunkXSize*4) + 1), ((chunkZSize*4) + 1), 2];

        double chunkDist = bezCurve.GetPathChunkDist(chunkIndex, numCurvesChunk);
        double stepCurve = chunkDist / (chunkZSize * 4 );
        stepCurve += 0.005f;

        bezCurve._buildArcDist = .0f;
        bezCurve._buildArcIndex = chunkIndex * numCurvesChunk;

        for (int z = 0, i = 0; z <= chunkZSize*4; z++)
        {
            
            Vector3 p = bezCurve.BuildAlong(z == 0 ? 0 : (float)stepCurve);

            for (int x = 0; x <= chunkXSize*4; x++)
            {
                float noiseVal = Mathf.PerlinNoise(x * 0.01f, ((lastChunkIndex * chunkZSize*4) + z) * 0.01f);
                float t = MathUtils.Remap(x, 0f, (float)(chunkXSize*4), 0f, 1f);
                float pathT = MathUtils.Remap(MathUtils.Remap(p.x, 0f, (float)chunkXSize, 0f, (float)(chunkXSize * 4)), 0f, (float)(chunkXSize * 4), 0f, 1f);
                float y = noiseVal - EasingFunctions.Custom(t + (0.5f - pathT)) * noiseVal;

                heightmap[z,x] = y;

                splatmaps[z, x, 1] = y <= 0.003 ? (y>=0.002 ? 0.5f : 1) : 0;
                splatmaps[z, x, 0] = y >= 0.002 ? (y <=0.003 ? 0.5f : 1) : 0;
            }
            
        }

        ChunkTrees chunkTrees = SetupTrees(trees, chunkXSize*4, 40f);

        if (lastChunkIndex > 0)
        {
            StitchHeightmaps(heightmap, lastHeightmap);
        }

        GameObject newChunk = new GameObject("chunk" + lastChunkIndex);
        newChunk.transform.parent = this.gameObject.transform;
        PathChunk chunkComponent = newChunk.AddComponent<PathChunk>();
        chunkComponent.InitChunk(chunkIndex, chunkXSize, chunkZSize, heightmap,splatmaps, terrainLayers,  chunkTrees, material);
        pathChunks.Add(chunkComponent);

        lastHeightmap = heightmap;
    }

    private ChunkTrees SetupTrees(GameObject[] sceneTrees, int chunkResolution, float treeMinDistance)
    {
        List<Vector2> treesPos = FastPoissonDiskSampling.Sampling(Vector2.zero, new Vector2(x: chunkResolution, y: chunkResolution), minimumDistance: treeMinDistance);

        int numTrees = sceneTrees.Length;
        TreePrototype[] treePrototypes = new TreePrototype[numTrees];
        int[] treesIndex = new int[treesPos.Count];

        for (int i = 0; i < numTrees; i++) 
        {
            TreePrototype tmpTreeProto = new TreePrototype();
            tmpTreeProto.prefab = sceneTrees[i];
            treePrototypes[i] = tmpTreeProto;
        }

        for (int i = 0; i < treesPos.Count; i++) 
        {
            int treeIndex = UnityEngine.Random.Range(0, numTrees);
            treesIndex[i] = treeIndex;
        }

        return new ChunkTrees(treePrototypes, treesPos, treesIndex);
    }

    private void StitchHeightmaps(float[,] heightmapToStich, float[,] stitchTo)
    {

        for(int x = 0; x <= chunkXSize*4; x++)
        {
            heightmapToStich[0, x] = stitchTo[chunkXSize*4, x];
        }

    }
    
    void Update()
    {
        chr.transform.position = bezCurve.MoveAlong(speed * Time.deltaTime);
        chr.transform.forward = bezCurve.GetFirstDerivative();
    }


}

public struct ChunkTrees
{
    public ChunkTrees(TreePrototype[] treePrototypes, List<Vector2> treesPos, int[] treesIndex)
    {
        TreePrototypes = treePrototypes;
        TreesPos = treesPos;
        TreesIndex = treesIndex;
        Count = treesPos.Count;
    }

    public TreePrototype[] TreePrototypes { get; }
    public List<Vector2> TreesPos { get; }
    public int[] TreesIndex { get; }
    public int Count { get; }

}