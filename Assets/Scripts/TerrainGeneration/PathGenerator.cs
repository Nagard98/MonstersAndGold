using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;

public class PathGenerator : MonoBehaviour
{

    public Vector3Variable playerPosition;
    public enum DrawMode { NoiseMap, ColourMap, Mesh };
    public DrawMode drawMode;
    public Noise.NormalizeMode normalizeMode;

    public SplatHeight[] splatHeights;
    public Tree[] trees;
    public Detail[] details;

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
    private int index;
    public BezierCurveVariable bezCurve;
    //public BezierSpline spline;

    private static System.Random rpng;

    public bool autoupdate;

    Queue<MapThreadInfo<ChunkData>> chunkDataThreadInfoQueue = new Queue<MapThreadInfo<ChunkData>>();
    Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

    private void Start()
    {
        index = 0;
        rpng = new System.Random();
    }

    public int Index { get { return index; } set { index = value; } }


    public void DrawChunkInEditor()
    {
        index = 0;
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

    public void RequestChunkData(Action<ChunkData> callback)
    {
        ThreadStart threadStart = delegate
        {
            ChunkDataThread(callback);
        };

        new Thread(threadStart).Start();
    }

    public ChunkData RequestChunkDataSync()
    {
        return GenerateChunkData();
    }

    private void ChunkDataThread(Action<ChunkData> callback)
    {
        ChunkData chunkData = GenerateChunkData();
        lock (chunkDataThreadInfoQueue)
        {
            chunkDataThreadInfoQueue.Enqueue(new MapThreadInfo<ChunkData>(callback, chunkData));
        }
    }

    public void RequestMeshData(Action<MeshData> callback, ChunkData chunkData)
    {
        ThreadStart threadStart = delegate
        {
            MeshDataThread(callback, chunkData);
        };

        new Thread(threadStart).Start();
    }

    public MeshData RequestMeshDataSync(ChunkData chunkData)
    {
        return MeshGenerator.GenerateTerrainMesh(chunkData.heightMap, meshHeightMultiplier, levelOfDetail);
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
        int numCurvesChunk = 5;
        Vector3[] curveControlPoints = OldPathGenerator.GenerateWaypoints(pathChunkSize - 1, numCurvesChunk, index, pathChunkSize - 1, pathChunkSize -1, 100);
        if (index == 0) { bezCurve.Value = new BezierSpline(curveControlPoints, nSamples: 10); }
        else { bezCurve.Value.AddPathWaypoints(curveControlPoints, index, numCurvesChunk); }
        
        SplineDisplacementInfo splineDisplacementInfo = new SplineDisplacementInfo(bezCurve.Value, index, numCurvesChunk);

        float[,] uncurvedNoiseMap = new float[PathGenerator.pathChunkSize, PathGenerator.pathChunkSize];
        float[,] noiseMap = Noise.GenerateNoiseMap(pathChunkSize, pathChunkSize, noiseScale, lacunarity, persistance, octaves, offset, normalizeMode, splineDisplacementInfo, ref uncurvedNoiseMap, heightCurve, seed);
        
        //Use uncurved noise map because in that case the entire path is at the same height
        float[,,] splatMaps = SplatMap.GenerateSplatMap(PathGenerator.pathChunkSize, splatHeights, uncurvedNoiseMap);
        ChunkTree[] chunkTrees = PathGenerator.BuildChunkTrees(trees, pathChunkSize, index, noiseMap, splatMaps);
        ChunkDetails[] chunkMultiDetails = PathGenerator.BuildChunkDetail(details, pathChunkSize, index, noiseMap, splatMaps);

        Color[] splatmapColors = SplatMap.ConvertToColors(splatMaps, PathGenerator.pathChunkSize, splatHeights);

        index += 1;
        return new ChunkData(noiseMap, splatMaps, splatmapColors, chunkTrees, chunkMultiDetails);
        
    }

    private static ChunkDetails[] BuildChunkDetail(Detail[] sceneDetailTypes, int chunkResolution, int chunkIndex, float[,] heightmap, float[,,] splatMaps)
    {
        ChunkDetails[] chunkMultiDetails = new ChunkDetails[sceneDetailTypes.Length];
        for(int i=0; i < sceneDetailTypes.Length; i++)
        {
            List<Vector2> detailsPos = FastPoissonDiskSampling.Sampling(Vector2.zero, new Vector2(x: chunkResolution, y: chunkResolution), minimumDistance: sceneDetailTypes[i].minDistance);
            detailsPos.Sort((a,b)=>a.y.CompareTo(b.y));
            DetailPrototype detailProto = PathChunk.BuildDetailProto(sceneDetailTypes[i]);
            int batches = (int)Math.Ceiling(detailsPos.Count / 1023f);
            Matrix4x4[][] matrixBatches = new Matrix4x4[batches][];
            for(int j = 0; j < batches; j++)
            {
                matrixBatches[j] = new Matrix4x4[1023];
            }
            Vector3[] detailAdjPos = new Vector3[detailsPos.Count];
            for(int j = 0, batch=0; j < detailsPos.Count; j++) 
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
                    matrixBatches[batch][j % 1023] = DrawMeshInstancedDemo.SetupMatrix(detailAdjPos[j]);
                    if ((j + 1) % 1023 == 0) batch += 1;
                }

            }

            ChunkDetails chunkDetails = new ChunkDetails(detailProto, detailAdjPos, sceneDetailTypes[i].layerIndex, matrixBatches);
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

            int batches = (int)Math.Ceiling(treesPos.Count / 1023f);
            Matrix4x4[][] matrixBatches = new Matrix4x4[batches][];
            for (int j = 0; j < batches; j++)
            {
                matrixBatches[j] = new Matrix4x4[1023];
            }

            Vector3[] treeAdjPos = new Vector3[treesPos.Count];
            for (int j = 0, batch = 0; j < treesPos.Count; j++)
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
                    matrixBatches[batch][j % 1023] = DrawMeshInstancedDemo.SetupMatrix(treeAdjPos[j]);
                    if ((j + 1) % 1023 == 0) batch += 1;
                }

            }
            chunkTrees[i] = new ChunkTree(tmpTreeProto, treeAdjPos, sceneTreeTypes[i].layerIndex, matrixBatches);
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

    void Update()
    {
        if (chunkDataThreadInfoQueue.Count > 0)
        {
            for(int i=0; i<chunkDataThreadInfoQueue.Count; i++)
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
}

public struct ChunkData
{
    public float[,] heightMap;
    public float[,,] splatMaps;
    public Color[] splatMapColors;
    public ChunkTree[] chunkTrees;
    public ChunkDetails[] chunkMultiDetails;

    public ChunkData(float[,] heightMap, float[,,] splatMaps, Color[] splatMapColors, ChunkTree[] chunkTrees, ChunkDetails[] chunkMultiDetails)
    {
        this.splatMapColors = splatMapColors;
        this.heightMap = heightMap;
        this.splatMaps = splatMaps;
        this.chunkTrees = chunkTrees;
        this.chunkMultiDetails = chunkMultiDetails;
    }

}
