using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GrassInstantiator : MonoBehaviour
{
    public Grass grassSettings;
    private Material grassMaterial;
    private int resolution;
    public Vector2 offset;
    public Texture heightMap, splatMap;

    public Vector3Variable playerPosition;
    private Bounds bounds;
    public bool updateGrass;

    private ComputeShader initializeGrassShader;
    private ComputeBuffer grassDataBuffer, argsBuffer;
    private Material grassMaterial2, grassMaterial3;

    private struct GrassData
    {
        public Vector4 position;
    }

    void Start()
    {
        bounds = new Bounds(Vector3.zero, new Vector3(-500.0f, 200.0f, 500.0f + ( offset.y * 2)));
        grassMaterial = new Material(grassSettings.grassMaterial);
        resolution = grassSettings.resolution * grassSettings.scale;
        initializeGrassShader = Resources.Load<ComputeShader>("GrassPoint");
        grassDataBuffer = new ComputeBuffer(resolution * resolution, 4 * 4);
        argsBuffer = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);

        updateGrassBuffer();
    }

    void Update()
    {
        //grassMaterial.SetBuffer("positionBuffer", grassDataBuffer);
        //grassMaterial.SetFloat("_Rotation", 0.0f);
        //grassMaterial.SetFloat("_DisplacementStrength", grassSettings.displacementStrength);
        /*grassMaterial2 = new Material(grassMaterial);
        grassMaterial2.SetBuffer("positionBuffer", grassDataBuffer);
        grassMaterial2.SetFloat("_Rotation", 50.0f);
        grassMaterial2.SetFloat("_DisplacementStrength", grassSettings.displacementStrength);
        grassMaterial3 = new Material(grassMaterial);
        grassMaterial3.SetBuffer("positionBuffer", grassDataBuffer);
        grassMaterial3.SetFloat("_Rotation", -50.0f);
        grassMaterial3.SetFloat("_DisplacementStrength", grassSettings.displacementStrength);*/

        if (offset.y - playerPosition.Value.z < 300f)
        {
            Graphics.DrawMeshInstancedIndirect(grassSettings.grassMesh, 0, grassMaterial, bounds, argsBuffer);
            //Graphics.DrawMeshInstancedIndirect(grassSettings.grassMesh, 0, grassMaterial2, bounds, argsBuffer);
            Graphics.DrawMeshInstancedIndirect(grassSettings.grassMesh, 0, grassMaterial3, bounds, argsBuffer);
        }

        if (updateGrass)
        {
            updateGrassBuffer();
            updateGrass = false;
        }
    }

    void updateGrassBuffer()
    {
        initializeGrassShader.SetInt("_Dimension", resolution);
        initializeGrassShader.SetInt("_Scale", grassSettings.scale);
        initializeGrassShader.SetBuffer(0, "_GrassDataBuffer", grassDataBuffer);
        initializeGrassShader.SetTexture(0, "_HeightMap", heightMap);
        initializeGrassShader.SetTexture(0, "_SplatMap", splatMap);
        initializeGrassShader.SetFloat("_DisplacementStrength", grassSettings.displacementStrength);
        initializeGrassShader.SetVector("_Offset", offset);
        initializeGrassShader.Dispatch(0, Mathf.CeilToInt(resolution / 8.0f), Mathf.CeilToInt(resolution / 8.0f), 1);

        uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
        // Arguments for drawing mesh.
        // 0 == number of triangle indices, 1 == population, others are only relevant if drawing submeshes.
        args[0] = (uint)grassSettings.grassMesh.GetIndexCount(0);
        args[1] = (uint)grassDataBuffer.count;
        args[2] = (uint)grassSettings.grassMesh.GetIndexStart(0);
        args[3] = (uint)grassSettings.grassMesh.GetBaseVertex(0);
        argsBuffer.SetData(args);

        grassMaterial.SetBuffer("positionBuffer", grassDataBuffer);
        grassMaterial.SetFloat("_Rotation", 0.0f);
        grassMaterial.SetFloat("_DisplacementStrength", grassSettings.displacementStrength);
        grassMaterial2 = new Material(grassMaterial);
        grassMaterial2.SetBuffer("positionBuffer", grassDataBuffer);
        grassMaterial2.SetFloat("_Rotation", 50.0f);
        grassMaterial2.SetFloat("_DisplacementStrength", grassSettings.displacementStrength);
        grassMaterial3 = new Material(grassMaterial);
        grassMaterial3.SetBuffer("positionBuffer", grassDataBuffer);
        grassMaterial3.SetFloat("_Rotation", -50.0f);
        grassMaterial3.SetFloat("_DisplacementStrength", grassSettings.displacementStrength);
    }



    void OnDisable()
    {
        grassDataBuffer.Release();
        argsBuffer.Release();
        grassDataBuffer = null;
        argsBuffer = null;
    }
}