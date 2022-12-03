using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(BoxCollider))]
public class PathChunk : MonoBehaviour
{
    private int _chunkIndex;
    private int _chunkXSize;
    private int _chunkZSize;
    public GameObject spawnCollider;

    private Material _material;
    Mesh _mesh;
    Vector3[] _vertices;
    int[] _triangles;
    Vector2[] _uvs;

    void Start()
    {
        _mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = _mesh;
        GetComponent<MeshRenderer>().material = _material;
        spawnCollider = new GameObject();
        spawnCollider.transform.parent = this.transform;
        spawnCollider.AddComponent<TriggerSpawn>();
        BoxCollider box = spawnCollider.AddComponent<BoxCollider>();
        box.transform.position = new Vector3(x: _chunkXSize/2, z: 100f, y: 1f);
        box.isTrigger = true;
        box.size = new Vector3(x: _chunkXSize, z: 5f, y: 10f);
        box.name = "collider" + _chunkIndex;
        UpdateMesh();
        transform.position = new Vector3(x: 0f, y: 0f, z: _chunkIndex * _chunkZSize);
    }

    public void InitChunk(Vector3[] vertices, Vector2[] uvs, int chunkIndex, int chunkXSize, int chunkZSize, Material material)
    {
        _vertices = vertices;
        _uvs = uvs;
        _chunkIndex = chunkIndex;
        _chunkXSize = chunkXSize;
        _chunkZSize = chunkZSize;
        _material = material;
        BuildMeshTriangles();
    }

    public Vector3[] Vertices
    {
        get { return _vertices; }
        set { _vertices = value; }
    }
    void BuildMeshTriangles()
    {
        _triangles = new int[_chunkXSize * _chunkZSize * 6];
        int vert = 0;
        int tris = 0;

        for (int z = 0; z < _chunkZSize; z++)
        {
            for (int x = 0; x < _chunkXSize; x++)
            {
                _triangles[tris + 0] = vert + 0;
                _triangles[tris + 1] = vert + _chunkXSize + 1;
                _triangles[tris + 2] = vert + 1;
                _triangles[tris + 3] = vert + 1;
                _triangles[tris + 4] = vert + _chunkXSize + 1;
                _triangles[tris + 5] = vert + _chunkXSize + 2;

                vert++;
                tris += 6;
            }
            vert++;
        }
    }

    public void UpdateMesh()
    {
        _mesh.Clear();

        _mesh.vertices = _vertices;
        _mesh.triangles = _triangles;
        _mesh.uv = _uvs;

        _mesh.RecalculateNormals();
    }
    void TriggerChunkBuild()
    {
        print("chunkIndex: " + _chunkIndex);
        SendMessageUpwards("BuildChunk", PathGenerator.ChunkBuildSettings);
    }
}

public struct ChunkSettings
{
    public ChunkSettings(int numCurves, float beelineLength)
    {
        NumCurves = numCurves;
        BeelineLength = beelineLength;
    }

    public int NumCurves { get; }
    public float BeelineLength { get; }

    public override string ToString() => $"({NumCurves}, {BeelineLength})";
}