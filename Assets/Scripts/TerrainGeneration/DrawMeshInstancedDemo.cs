using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawMeshInstancedDemo : MonoBehaviour
{
    // Material to use for drawing the meshes.
    public Material material;
    public Mesh mesh;
    public Matrix4x4[][] matrices;
    public Vector3Variable playerPosition;

    private int[] population;
    private Vector3[] originPositions;
    private int batches;
    private MaterialPropertyBlock block;


    public static Matrix4x4 SetupMatrix(Vector3 position)
    {
        return Matrix4x4.TRS(position, Quaternion.identity, new Vector3(2,2,2));
    }

    private void Setup()
    {
        batches = matrices.Length;
        population = new int[batches];
        originPositions = new Vector3[batches];
        for(int i = 0; i < batches; i++)
        {
            originPositions[i] = new Vector3(matrices[i][0][0, 3], matrices[i][0][1, 3], matrices[i][0][2, 3]);
            population[i] = matrices[i].Length;
        }
        
        block = new MaterialPropertyBlock();
    }

    private void Start()
    {
        Setup();
    }

    private void Update()
    {
        // Draw a bunch of meshes each frame.
        for(int i = 0; i < batches; i++){
            if(Vector3.Distance(originPositions[i], playerPosition.Value) < 250f )Graphics.DrawMeshInstanced(mesh, 0, material, matrices[i], population[i], block);
        }
        
    }
}
