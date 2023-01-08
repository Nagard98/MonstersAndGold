using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class PathGenerator : MonoBehaviour
{
    private List<PathChunk> pathChunks;

    public SplatHeights[] splatHeights;
    public Tree[] trees;
    public Detail[] details;

    public Material material;

    public int concurrentChunkNumber;
    public int chunkXSize;
    public int chunkZSize;
    public float pathMaxWidth;
    public int numCurvesChunk;

    private int lastChunkIndex;
    private float[,] lastHeightmap;
    private static ChunkSettings chunkSettings;

    public BezierCurveVariable bezCurve;
    public UnityEvent generatedEvent;

    public AnimationCurve easingFunction;

    void Start()
    {
        numCurvesChunk = 10;
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
        bezCurve.Value.Reset();

        generatedEvent.Invoke();
    }

    public static ChunkSettings ChunkBuildSettings{
        get { return chunkSettings; }
    }

    private void BuildChunk(ChunkSettings chunkSettings)
    {
        Vector3[] curveControlPoints = GenerateWaypoints(chunkSettings.BeelineLength, chunkSettings.NumCurves);

        if (lastChunkIndex == 0) { bezCurve.Value = new BezierSpline(curveControlPoints, nSamples: 10); }
        else { bezCurve.Value.AddPathWaypoints(curveControlPoints, lastChunkIndex, numCurvesChunk); }

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

        double chunkDist = bezCurve.Value.GetPathChunkDist(chunkIndex, numCurvesChunk);
        double stepCurve = chunkDist / (chunkZSize * 4);

        bezCurve.Value._buildArcDist = .0f;
        bezCurve.Value._buildArcIndex = chunkIndex * numCurvesChunk;

        Vector3 p = bezCurve.Value.BuildAlong(0);
        float lastZ = p.z;
        float totalZTraversed = 0f;
        float zError = 0f;
        for (int z = 0; z <= chunkZSize*4; z++)
        {
            if (z > 0)
            {
                p = bezCurve.Value.BuildAlong((float)stepCurve + zError);
                float pzStep = p.z - lastZ;
                totalZTraversed += pzStep;
                zError = (z * 0.25f) - totalZTraversed;
                lastZ = p.z;
            }

            for (int x = 0; x <= chunkXSize*4; x++)
            {
                float noiseVal = Mathf.PerlinNoise(x * 0.01f, ((lastChunkIndex * chunkZSize*4) + z) * 0.01f);
                float t = MathUtils.Remap(x, 0f, (float)(chunkXSize*4), 0f, 1f);
                float pathT = MathUtils.Remap(MathUtils.Remap(p.x, 0f, (float)chunkXSize, 0f, (float)(chunkXSize * 4)), 0f, (float)(chunkXSize * 4), 0f, 1f);
                float y = noiseVal - (easingFunction.Evaluate(t + (0.5f - pathT)) * noiseVal);

                heightmap[z,x] = y;

                for(int i=0; i < splatHeights.Length; i++)
                {

                    //TODO: generalizza a più layer
                    float height = splatHeights[i].height;
                    float overlap = splatHeights[i].overlap;
                    
                    if(i == splatHeights.Length - 1)
                    {
                        splatmaps[z, x, splatHeights[i].layerIndex] = (y >= height) ? (y >= (height + splatHeights[i - 1].overlap) ? 1 : 0.5f) : 0;
    }
                    else
                    {
                        float nextHeight = splatHeights[i + 1].height;
                        splatmaps[z, x, splatHeights[i].layerIndex] = (y >= height && y <= nextHeight + overlap) ? (y >= nextHeight ? 0.5f : 1) : 0;
                    }
                }

                noiseVal = Mathf.Clamp01(Mathf.PerlinNoise(0.01f, ((lastChunkIndex * chunkZSize * 4) + z) * 0.002f));
                float scaledNoise = MathUtils.Remap(noiseVal, 0f, 1f, 0f, 0.5f);
                heightmap[z, x] += scaledNoise;
            }
            
        }

        ChunkTrees chunkTrees = SetupTrees(trees, chunkXSize*4, 35f);
        ChunkSplats tmpSplatmaps = SetupSplatmaps(splatHeights, splatmaps);
        ChunkDetail[] tmpDetails = SetupDetailMaps(details, splatmaps, chunkXSize*4);

        if (lastChunkIndex > 0) StitchHeightmaps(heightmap, lastHeightmap);
        lastHeightmap = heightmap;

        GameObject newChunk = new GameObject("chunk" + lastChunkIndex);
        newChunk.transform.parent = this.gameObject.transform;
        PathChunk chunkComponent = newChunk.AddComponent<PathChunk>();
        chunkComponent.InitChunk(chunkIndex, chunkXSize, chunkZSize, heightmap, tmpSplatmaps, chunkTrees, material, tmpDetails);
        pathChunks.Add(chunkComponent);
    }

    private ChunkDetail[] SetupDetailMaps(Detail[] tmpdetails, float[,,] splatmaps, int resolution)
    {
        ChunkDetail[] tmpChunkDetails = new ChunkDetail[tmpdetails.Length];
        for (int i = 0; i < tmpdetails.Length; i++) 
        {
            List<Vector2> tmpSampling = FastPoissonDiskSampling.Sampling(Vector2.zero, new Vector2(x: resolution, y: resolution), minimumDistance: tmpdetails[i].minDistance);
            int[,] detailMap = new int[resolution + 1, resolution + 1];

            for (int j = 0; j < tmpSampling.Count; j++)
            {
                int x = (int)(tmpSampling[j].x); 
                int y = (int)(tmpSampling[j].y);
                if (splatmaps[x, y, tmpdetails[i].layerIndex] == 1)
                {
                    detailMap[x, y] = 1;
                }
            }
            tmpChunkDetails[i] = new ChunkDetail(tmpdetails[i], detailMap);

        }
        return tmpChunkDetails;
    }

    private ChunkSplats SetupSplatmaps(SplatHeights[] splatHeights, float[,,] splatmaps)
    {
        ChunkSplats tmpSplatmaps = new ChunkSplats();
        tmpSplatmaps.splatHeights = splatHeights;
        tmpSplatmaps.Splatmaps = splatmaps;
        return tmpSplatmaps;
    }

    private ChunkTrees SetupTrees(Tree[] sceneTreeTypes, int chunkResolution, float treeMinDistance)
    {
        List<Vector2> treesPos = FastPoissonDiskSampling.Sampling(Vector2.zero, new Vector2(x: chunkResolution, y: chunkResolution), minimumDistance: treeMinDistance);

        int numTreeTypes = sceneTreeTypes.Length;
        TreePrototype[] treePrototypes = new TreePrototype[numTreeTypes];
        int[] treesIndex = new int[treesPos.Count];
        int[] treeLayer= new int[treesPos.Count];

        for (int i = 0; i < numTreeTypes; i++) 
        {
            TreePrototype tmpTreeProto = new TreePrototype();
            tmpTreeProto.prefab = sceneTreeTypes[i].treeObject;
            treePrototypes[i] = tmpTreeProto;
        }

        for (int i = 0; i < treesPos.Count; i++) 
        {
            int treeIndex = UnityEngine.Random.Range(0, numTreeTypes);
            treesIndex[i] = treeIndex;
            treeLayer[i] = sceneTreeTypes[treeIndex].layerIndex;
        }

        return new ChunkTrees(treePrototypes, treesPos, treesIndex, treeLayer);
    }

    private void StitchHeightmaps(float[,] heightmapToStich, float[,] stitchTo)
    {

        for(int x = 0; x <= chunkXSize*4; x++)
        {
            heightmapToStich[0, x] = stitchTo[chunkXSize*4, x];
        }

    }

}

public struct ChunkTrees
{
    public ChunkTrees(TreePrototype[] treePrototypes, List<Vector2> treesPos, int[] treeIndex, int [] treeLayer)
    {
        TreePrototypes = treePrototypes;
        TreesPos = treesPos;
        TreesIndex = treeIndex;
        TreeLayer = treeLayer;
        Count = treesPos.Count;
    }

    public TreePrototype[] TreePrototypes { get; }
    public List<Vector2> TreesPos { get; }
    public int[] TreesIndex { get; }
    public int[] TreeLayer { get; }
    public int Count { get; }

}


[Serializable]
public class Detail
{
    public GameObject detail;
    public float minDistance;
    public int layerIndex;
}

[Serializable]
public class Tree
{
    public GameObject treeObject;
    public int layerIndex;
}

[Serializable]
public class SplatHeights
{
    public TerrainLayer terrainLayer;
    public int layerIndex;
    public float height;
    public float overlap;
}

public class ChunkDetail
{
    public Detail detail;
    private int[,] detailMap;

    public ChunkDetail(Detail detail, int[,] detailMap)
    {
        this.detail = detail;
        this.detailMap = detailMap;
    }
    public int[,] DetailMap
    {
        get { return detailMap; }
        set { detailMap = value; }
    }
}

public class ChunkSplats
{
    public SplatHeights[] splatHeights;
    private float[,,] splatmaps;
    public float[,,] Splatmaps
    {
        get { return splatmaps; }
        set { splatmaps = value; }
    }
}