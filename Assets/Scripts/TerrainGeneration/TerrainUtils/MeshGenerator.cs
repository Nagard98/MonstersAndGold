using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshGenerator
{
    public static ComputeShader displacePlane = Resources.Load<ComputeShader>("DisplacePlane");

    public static Vector3[] StitchMeshes(Vector3[] meshVertices, Vector3[] stitchTo, int meshSize)
    {
        for (int x = 0; x < meshSize; x++)
        {
            float frs = stitchTo[(meshSize*meshSize) - meshSize + x].y;
            //float sec = meshVertices[chunkXSize + 1 + x].y;
            //float avg = (frs + sec) / 2;
            meshVertices[x].y = frs;
            //firstVerts[((chunkXSize + 1) * (chunkZSize + 1)) - (chunkXSize + 1) + x].y = avg;
        }
        return meshVertices;

        //firstMesh.Invoke("UpdateMesh", 0);
        //secondMesh.Invoke("UpdateMesh", 0);
    }

    public static MeshData GenerateTerrainMesh(float[,] heightmap, float heightMultiplier, int levelOfDetail, Vector3[] stitchTo = null)
    {
        int width = heightmap.GetLength(0);
        int height = heightmap.GetLength(1);

        int meshSemplificationIncrement = levelOfDetail == 0 ? 1 : levelOfDetail * 2;
        int verticesPerLine = (width - 1) / meshSemplificationIncrement + 1;

        MeshData meshData = new MeshData(verticesPerLine, verticesPerLine);
        int vertexIndex = 0;

        for (int y = 0; y < height; y += meshSemplificationIncrement) 
        {
            for (int x = 0; x < width; x += meshSemplificationIncrement) 
            {
                meshData.vertices[vertexIndex] = new Vector3(x, heightmap[y, x]/** heightMultiplier*/, y);
                meshData.uvs[vertexIndex] = new Vector2(x / (float)width, y / (float)height);

                if (x < width - 1 && y < height - 1) 
                {
                    meshData.AddTriangle(vertexIndex, vertexIndex + verticesPerLine + 1, vertexIndex + verticesPerLine);
                    meshData.AddTriangle(vertexIndex + verticesPerLine + 1, vertexIndex, vertexIndex + 1);

                }

                vertexIndex++;
            }
        }

        if(stitchTo != null)StitchMeshes(meshData.vertices, stitchTo, width);

        return meshData;
    }

    public static MeshData GenerateFlatMesh(int size , int levelOfDetail)
    {
        int width = size;
        int height = size;

        int meshSemplificationIncrement = levelOfDetail == 0 ? 1 : levelOfDetail * 2;
        int verticesPerLine = (width - 1) / meshSemplificationIncrement + 1;

        MeshData meshData = new MeshData(verticesPerLine, verticesPerLine);
        int vertexIndex = 0;

        for (int y = 0; y < height; y += meshSemplificationIncrement)
        {
            for (int x = 0; x < width; x += meshSemplificationIncrement)
            {
                meshData.vertices[vertexIndex] = new Vector3(x, 0, y);
                meshData.uvs[vertexIndex] = new Vector2(x / (float)width, y / (float)height);

                if (x < width - 1 && y < height - 1)
                {
                    meshData.AddTriangle(vertexIndex, vertexIndex + verticesPerLine + 1, vertexIndex + verticesPerLine);
                    meshData.AddTriangle(vertexIndex + verticesPerLine + 1, vertexIndex, vertexIndex + 1);

                }
                vertexIndex++;
            }
        }
        return meshData;
    }
}

public class MeshData
{
    public Vector3[] vertices;
    public int[] triangles;
    public Vector2[] uvs;
    public Vector3[] stitchTo;
    public int meshWidth;
    public int meshHeight;

    int triangleIndex;

    public MeshData(int meshWidth, int meshHeight, Vector3[] stitchTo=null)
    {
        vertices = new Vector3[meshWidth * meshHeight];
        triangles = new int[(meshWidth - 1) * (meshHeight - 1) * 6];
        uvs = new Vector2[meshHeight * meshWidth];
        this.stitchTo = stitchTo;
        this.meshHeight = meshHeight;
        this.meshWidth = meshWidth;
    }

    public void AddTriangle(int a, int b, int c)
    {
        triangles[triangleIndex] = c;
        triangles[triangleIndex + 1] = b;
        triangles[triangleIndex + 2] = a;
        triangleIndex += 3;
    }

    private Vector3[] CalculateNormals()
    {
        Vector3[] vertexNormals = new Vector3[vertices.Length];
        int triangleCount = triangles.Length / 3;
        for(int i=0; i<triangleCount; i++)
        {
            int normalTriangleIndex = i * 3;
            int vertexIndexA = triangles[normalTriangleIndex];
            int vertexIndexB = triangles[normalTriangleIndex + 1];
            int vertexIndexC = triangles[normalTriangleIndex + 2];

            Vector3 triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);
            vertexNormals[vertexIndexA] += triangleNormal;
            vertexNormals[vertexIndexB] += triangleNormal;
            vertexNormals[vertexIndexC] += triangleNormal;
        }

        for (int i = 0; i < vertexNormals.Length; i++)
        {
            vertexNormals[i].Normalize();
        }

        return vertexNormals;
    }

    private Vector3 SurfaceNormalFromIndices(int indexA, int indexB, int indexC)
    {
        Vector3 pointA = vertices[indexA];
        Vector3 pointB = vertices[indexB];
        Vector3 pointC = vertices[indexC];

        Vector3 sideAB = pointB - pointC;
        Vector3 sideAC = pointC - pointA;
        return Vector3.Cross(sideAB, sideAC).normalized;
    }

    public void DisplaceMesh(Material terrainMat)
    {
        Vector3[] verts = vertices;
        Vector2[] uv = uvs;

        ComputeBuffer vertBuffer = new ComputeBuffer(verts.Length, 12);
        ComputeBuffer uvBuffer = new ComputeBuffer(uv.Length, 8);
        vertBuffer.SetData(verts);
        uvBuffer.SetData(uv);

        MeshGenerator.displacePlane.SetBuffer(0, "_Vertices", vertBuffer);
        MeshGenerator.displacePlane.SetBuffer(0, "_UVs", uvBuffer);
        MeshGenerator.displacePlane.SetTexture(0, "_HeightMap", terrainMat.GetTexture("_HeightMap"));
        MeshGenerator.displacePlane.SetFloat("_DisplacementStrength", terrainMat.GetFloat("_DisplacementStrength"));
        MeshGenerator.displacePlane.Dispatch(0, Mathf.CeilToInt(verts.Length / 128.0f), 1, 1);

        vertBuffer.GetData(verts);
        vertBuffer.Release();
        uvBuffer.Release();

        vertices = verts;
    }

    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        mesh.normals = CalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }
}
