using UnityEngine;
using System;

public struct ChunkTree
{
    public ChunkTree(TreePrototype treePrototype, Vector3[] treePos, int treeLayer)
    {
        TreePrototype = treePrototype;
        TreePos = treePos;
        TreeLayer = treeLayer;
    }

    public TreePrototype TreePrototype { get; }
    public Vector3[] TreePos { get; }
    public int TreeLayer { get; }

}

public struct ChunkDetails
{
    public ChunkDetails(DetailPrototype detailPrototype, Vector3[] detailPos, int detailLayer)
    {
        DetailPrototype = detailPrototype;
        DetailPos = detailPos;
        DetailLayer = detailLayer;
    }

    public DetailPrototype DetailPrototype { get; }
    public Vector3[] DetailPos { get; }
    public int DetailLayer { get; }

}

public struct ChunkData
{
    public float[,] heightMap;
    public float[,,] splatMaps;
    public Color[] splatMapColors;
    public ChunkTree[] chunkTrees;
    public ChunkDetails[] chunkMultiDetails;
    public Grass grassSettings;
    public int chunkIndex;

    public ChunkData(float[,] heightMap, float[,,] splatMaps, Color[] splatMapColors, ChunkTree[] chunkTrees, ChunkDetails[] chunkMultiDetails, Grass grassSettings, int chunkIndex)
    {
        this.splatMapColors = splatMapColors;
        this.heightMap = heightMap;
        this.splatMaps = splatMaps;
        this.chunkTrees = chunkTrees;
        this.chunkMultiDetails = chunkMultiDetails;
        this.grassSettings = grassSettings;
        this.chunkIndex = chunkIndex;
    }

}

[Serializable]
public class Detail
{
    public GameObject gameObject;
    public float minDistance;
    public int layerIndex;
}

[Serializable]
public class Tree
{
    public GameObject treeObject;
    public int layerIndex;
    public float minDistance;
}

[Serializable]
public class Grass
{
    public int resolution = 100;
    public int scale = 1;
    public float displacementStrength = 200.0f;
    public Material grassMaterial;
    public Mesh grassMesh;
}

[Serializable]
public class SplatHeight
{
    public Texture2D texture;
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
    public SplatHeight[] splatHeights;
    private float[,,] splatmaps;
    public float[,,] Splatmaps
    {
        get { return splatmaps; }
        set { splatmaps = value; }
    }
}