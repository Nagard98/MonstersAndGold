using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;

public class PathGenerator : MonoBehaviour
{
    public enum DrawMode { NoiseMap, ColourMap, Mesh };
    public DrawMode drawMode;
    public Noise.NormalizeMode normalizeMode;

    public SplatHeight[] splatHeights;
    public Tree[] trees;
    public Detail[] details;
    public Grass grass;

    public const int pathChunkSize = 241;
    [Range(0, 6)] public int levelOfDetail;
    public int meshHeightMultiplier;
    public AnimationCurve heightCurve;

    public float noiseScale;
    public int octaves;
    [Range(0, 1)] public float persistance;
    public float lacunarity;

    public int seed;
    public Vector2 offset;

    private int lastChunkIndex;
    private int firstChunkIndex;
    public BezierCurveVariable bezCurve;
    
    private static System.Random rpng;

    public bool autoupdate;

    private Queue<MapThreadInfo<ChunkData>> chunkDataThreadInfoQueue;
    private Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue;

    public void OnEnable()
    {
        StartUp();
    }

    public void StartUp()
    {
        lastChunkIndex = 0;
        firstChunkIndex = 0;
        rpng = new System.Random();
        chunkDataThreadInfoQueue = new Queue<MapThreadInfo<ChunkData>>();
        meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();
    }

    public void CleanUp()
    {
        bezCurve.Destroy();
    }

    private void Update()
    {
        if (chunkDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < chunkDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<ChunkData> threadInfo = chunkDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }

        if (meshDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < meshDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
    }


    public int LastIndex { get { return lastChunkIndex; } set { lastChunkIndex = value; } }
    public int FirstIndex { get { return firstChunkIndex; } set { firstChunkIndex = value; } }


    public void DrawChunkInEditor()
    {
        lastChunkIndex = 0;
        ChunkData chunkData = GenerateChunkData();

        ChunkDisplay display = FindObjectOfType<ChunkDisplay>();
        if (drawMode == DrawMode.NoiseMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(chunkData.heightMap));
        }
        else if (drawMode == DrawMode.Mesh)
        {
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(chunkData.heightMap, meshHeightMultiplier, levelOfDetail), TextureGenerator.TextureFromColorMap(chunkData.splatMapColors,pathChunkSize,pathChunkSize));
        }
    }

    public ChunkData RequestChunkDataSync()
    {
        return GenerateChunkData();
    }

    public void RequestChunkData(Action<ChunkData> callback)
    {
        ThreadStart threadStart = delegate
        {
            ChunkDataThread(callback);
        };

        new Thread(threadStart).Start();
    }

    private void ChunkDataThread(Action<ChunkData> callback)
    {
        ChunkData chunkData = GenerateChunkData();
        lock (chunkDataThreadInfoQueue)
        {
            chunkDataThreadInfoQueue.Enqueue(new MapThreadInfo<ChunkData>(callback, chunkData));
        }
    }

    public MeshData RequestMeshDataSync(ChunkData chunkData)
    {
        return MeshGenerator.GenerateFlatMesh(pathChunkSize, levelOfDetail);
        //return MeshGenerator.GenerateTerrainMesh(chunkData.heightMap, meshHeightMultiplier, levelOfDetail);
    }

    public void RequestMeshData(Action<MeshData> callback, ChunkData chunkData)
    {
        ThreadStart threadStart = delegate
        {
            MeshDataThread(callback, chunkData);
        };

        new Thread(threadStart).Start();
    }

    private void MeshDataThread(Action<MeshData> callback, ChunkData chunkData)
    {
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(chunkData.heightMap, meshHeightMultiplier, levelOfDetail);
        lock (meshDataThreadInfoQueue)
        {
            meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
        }
    }

    private ChunkData GenerateChunkData()
    {
        int numCurvesChunk = 2;
        Vector3[] curveControlPoints = GenerateWaypoints(pathChunkSize - 1, numCurvesChunk, lastChunkIndex, pathChunkSize - 1, pathChunkSize -1, 50);
        if (lastChunkIndex == 0) { bezCurve.Value = new BezierSpline(curveControlPoints, nSamples: 10); }
        else { bezCurve.Value.AddPathWaypoints(curveControlPoints, lastChunkIndex, numCurvesChunk); }
        
        SplineDisplacementInfo splineDisplacementInfo = new SplineDisplacementInfo(bezCurve.Value, lastChunkIndex, numCurvesChunk);

        float[,] uncurvedNoiseMap = new float[PathGenerator.pathChunkSize, PathGenerator.pathChunkSize];
        float[,] noiseMap = Noise.GenerateNoiseMap(pathChunkSize, pathChunkSize, noiseScale, lacunarity, persistance, octaves, offset, normalizeMode, splineDisplacementInfo, ref uncurvedNoiseMap, heightCurve, seed);
        
        //Use uncurved noise map because in that case the entire path is at the same height
        float[,,] splatMaps = SplatMap.GenerateSplatMap(PathGenerator.pathChunkSize, splatHeights, uncurvedNoiseMap);
        ChunkTree[] chunkTrees = PathGenerator.BuildChunkTrees(trees, pathChunkSize, lastChunkIndex, noiseMap, splatMaps);
        ChunkDetails[] chunkMultiDetails = PathGenerator.BuildChunkDetail(details, pathChunkSize, lastChunkIndex, noiseMap, splatMaps);

        Color[] splatmapColors = SplatMap.ConvertToColors(splatMaps, PathGenerator.pathChunkSize, splatHeights);

        ChunkData chunkData = new ChunkData(noiseMap, splatMaps, splatmapColors, chunkTrees, chunkMultiDetails, grass, lastChunkIndex);
        lastChunkIndex += 1;

        return chunkData;
    }

    public static Vector3[] GenerateWaypoints(float beelineLength, int numCurves, int lastChunkIndex, int chunkZSize, int chunkXSize, float pathMaxWidth)
    {
        float step = (beelineLength / (float)numCurves) / 2f;
        Vector3[] waypoints = new Vector3[numCurves * 3];

        for (int i = 0, c = 0, d = 0; i < waypoints.Length; i++, c++, d++)
        {
            if (c == 3)
            {
                c = 0;
                d -= 1;
            }

            float horOffset = Mathf.PerlinNoise(.01f, (lastChunkIndex * chunkZSize) * .008f + step * d * .008f);
            float scaledOffset = MathUtils.Remap(horOffset, 0f, 1f, 0f, (float)(pathMaxWidth));

            waypoints[i] = new Vector3(x: scaledOffset + (((chunkXSize) - (pathMaxWidth)) / 2), y: 1, z: (lastChunkIndex * (chunkZSize)) + d * step);

        }
        return waypoints;
    }


    private static ChunkDetails[] BuildChunkDetail(Detail[] sceneDetailTypes, int chunkResolution, int chunkIndex, float[,] heightmap, float[,,] splatMaps)
    {
        ChunkDetails[] chunkMultiDetails = new ChunkDetails[sceneDetailTypes.Length];
        for(int i=0; i < sceneDetailTypes.Length; i++)
        {
            List<Vector2> detailsPos = FastPoissonDiskSampling.Sampling(Vector2.zero, new Vector2(x: chunkResolution, y: chunkResolution), minimumDistance: sceneDetailTypes[i].minDistance);
            detailsPos.Sort((a,b)=>a.y.CompareTo(b.y));
            DetailPrototype detailProto = PathChunk.BuildDetailProto(sceneDetailTypes[i]);

            Vector3[] detailAdjPos = new Vector3[detailsPos.Count];
            for(int j = 0; j < detailsPos.Count; j++) 
            {
                int x = (int)(detailsPos[j].x);
                int y = (int)(detailsPos[j].y);
                if(splatMaps[y,x,sceneDetailTypes[i].layerIndex] > 0f)
                {
                    float elevation = heightmap[x, y];

                    detailAdjPos[j] = new Vector3(
                        x,
                        elevation * EndlessPath.pathGenerator.meshHeightMultiplier,
                        chunkIndex * chunkResolution + y
                        );
                }
            }

            ChunkDetails chunkDetails = new ChunkDetails(detailProto, detailAdjPos, sceneDetailTypes[i].layerIndex);
            chunkMultiDetails[i] = chunkDetails;
        }
        
        return chunkMultiDetails;
    }

    private static ChunkTree[] BuildChunkTrees(Tree[] sceneTreeTypes, int chunkResolution, int chunkIndex,float[,] heightmap, float[,,] splatmaps)
    {
        ChunkTree[] chunkTrees = new ChunkTree[sceneTreeTypes.Length];
        for (int i = 0; i < sceneTreeTypes.Length; i++)
        {
            List<Vector2> treesPos = FastPoissonDiskSampling.Sampling(Vector2.zero, new Vector2(x: chunkResolution, y: chunkResolution), minimumDistance: sceneTreeTypes[i].minDistance);
            
            TreePrototype tmpTreeProto = new TreePrototype();
            tmpTreeProto.prefab = sceneTreeTypes[i].treeObject;

            Vector3[] treeAdjPos = new Vector3[treesPos.Count];
            for (int j = 0; j < treesPos.Count; j++)
            {
                int x = (int)(treesPos[j].x);
                int y = (int)(treesPos[j].y);
                if (splatmaps[y, x, sceneTreeTypes[i].layerIndex] > 0f)
                {
                    float elevation = heightmap[x, y];

                    treeAdjPos[j] = new Vector3(
                        x,
                        elevation * EndlessPath.pathGenerator.meshHeightMultiplier,
                        chunkIndex * chunkResolution + y
                        );
                }

            }
            chunkTrees[i] = new ChunkTree(tmpTreeProto, treeAdjPos, sceneTreeTypes[i].layerIndex);
        }

        return chunkTrees;
    }


    private void OnValidate()
    {
        if (octaves < 0) octaves = 0;
        if (lacunarity < 0) lacunarity = 1;
    }

    struct MapThreadInfo<T>
    {
        public readonly Action<T> callback;
        public readonly T parameter;

        public MapThreadInfo(Action<T> callback, T parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
        }
    }
}
