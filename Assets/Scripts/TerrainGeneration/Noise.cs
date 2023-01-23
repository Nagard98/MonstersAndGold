using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise
{

    public enum NormalizeMode { Local,Global }

    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, float scale, float lacunarity, float persistance, int octaves, Vector2 offset, NormalizeMode normalizeMode, SplineDisplacementInfo splineDisplacementInfo, ref float[,] uncurvedNoiseMap, AnimationCurve heightCurve, int seed=0)
    {
        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];
        float maxPossibleHeight = 0;
        float amplitude = 1;
        float frequency = 1;

        for(int i=0; i<octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) + offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);

            maxPossibleHeight += amplitude;
            amplitude *= persistance;
        }

        float[,] noiseMap = new float[mapWidth, mapHeight];

        if (scale <= 0) scale = 0.0001f;

        float maxLocalNoiseHeight = float.MinValue;
        float minLocalNoiseHeight = float.MaxValue;

        for (int y=0; y < mapHeight; y++)
        {
            for(int x=0; x < mapWidth; x++)
            {
                amplitude = 1;
                frequency = 1;
                float noiseHeight = 0;

                for(int i=0; i < octaves; i++)
                {
                    float sampleX = (x + octaveOffsets[i].x) / scale * frequency;
                    float sampleY = (y + octaveOffsets[i].y) / scale * frequency;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                if (noiseHeight > maxLocalNoiseHeight) maxLocalNoiseHeight = noiseHeight;
                else if (noiseHeight < minLocalNoiseHeight) minLocalNoiseHeight = noiseHeight;

                noiseMap[x, y] = noiseHeight;
            }
        }

        double chunkDist = splineDisplacementInfo.displacementSpline.GetPathChunkDist(splineDisplacementInfo.chunkIndex, splineDisplacementInfo.numCurvesChunk);
        double stepCurve = chunkDist / (mapHeight - 1);

        splineDisplacementInfo.displacementSpline._buildArcDist = .0f;
        splineDisplacementInfo.displacementSpline._buildArcIndex = splineDisplacementInfo.numCurvesChunk * splineDisplacementInfo.chunkIndex;

        Vector3 splineCoord = splineDisplacementInfo.displacementSpline.BuildAlong(0);
        float lastZ = splineCoord.z;
        float totalZTraversed = 0f;
        float zError = 0f;

        for (int y = 0; y < mapHeight; y++)
        {
            if (y > 0)
            {
                splineCoord = splineDisplacementInfo.displacementSpline.BuildAlong((float)stepCurve + zError);
                float pzStep = splineCoord.z - lastZ;
                totalZTraversed += pzStep;
                zError = y - totalZTraversed;
                lastZ = splineCoord.z;
            }
            float scaledPathCurvature = 0f;
            if (uncurvedNoiseMap != null)
            {
                float pathCurvatureNoise = Mathf.Clamp01(Mathf.PerlinNoise(0.01f, ((splineDisplacementInfo.chunkIndex * mapHeight) + y) * 0.02f));
                scaledPathCurvature = MathUtils.Remap(pathCurvatureNoise, 0f, 1f, 0f, 0.5f);
            }

            for (int x = 0; x < mapWidth; x++)
            {
                

                if (normalizeMode == NormalizeMode.Local)
                {
                    float uncurvedLocalNormalizedHeight = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[x, y]);
                    float localNormalizedHeight = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[x, y] + scaledPathCurvature);
                    if (splineDisplacementInfo.apply == true) localNormalizedHeight += GetSplineDisplacement(localNormalizedHeight, splineCoord.x, mapWidth, x);
                    noiseMap[x, y] = localNormalizedHeight;
                    uncurvedNoiseMap[x, y] = uncurvedLocalNormalizedHeight;
                }
                else
                {
                    float normalizedHeight = (noiseMap[x, y] + 1) / (2f * (maxPossibleHeight+0.5f) / 1.9f);
                    if (splineDisplacementInfo.apply == true) normalizedHeight += GetSplineDisplacement(normalizedHeight, splineCoord.x, mapWidth, x);
                    noiseMap[x, y] = heightCurve.Evaluate(normalizedHeight) + scaledPathCurvature;
                    uncurvedNoiseMap[x, y] = heightCurve.Evaluate(normalizedHeight);
                }

            }
        }

        return noiseMap;
    }

    public static float[,] SumNoiseMaps(float[,] noiseMap1, float[,] noiseMap2)
    {
        int mapSize = noiseMap1.GetLength(0);
        float[,] sum = new float[mapSize, mapSize];

        for (int x = 0; x < mapSize; x++)
        {
            for (int y = 0; y < mapSize; y++)
            {
                sum[x, y] = noiseMap1[x, y] + noiseMap2[x, y];
            }
        }

        return sum;
    }

    /*public static float[,] UpdateWithCurvature(float[,] noiseMap, int mapSize, SplineDisplacementInfo splineDisplacementInfo)
    {
        double chunkDist = splineDisplacementInfo.displacementSpline.GetPathChunkDist(splineDisplacementInfo.chunkIndex, splineDisplacementInfo.numCurvesChunk);
        double stepCurve = chunkDist / (mapSize - 1);

        splineDisplacementInfo.displacementSpline._buildArcDist = .0f;
        splineDisplacementInfo.displacementSpline._buildArcIndex = splineDisplacementInfo.numCurvesChunk * splineDisplacementInfo.chunkIndex;

        Vector3 splineCoord = splineDisplacementInfo.displacementSpline.BuildAlong(0);
        float lastZ = splineCoord.z;
        float totalZTraversed = 0f;
        float zError = 0f;

        for (int x = 0; x < mapSize; x++)
        {
            for (int y = 0; y < mapSize; y++)
            {
                if (y > 0)
                {
                    splineCoord = splineDisplacementInfo.displacementSpline.BuildAlong((float)stepCurve + zError);
                    float pzStep = splineCoord.z - lastZ;
                    totalZTraversed += pzStep;
                    zError = y - totalZTraversed;
                    lastZ = splineCoord.z;
                }

                float pathCurvatureNoise = Mathf.Clamp01(Mathf.PerlinNoise(0.01f, ((splineDisplacementInfo.chunkIndex * mapSize) + y) * 0.02f));
                float scaledPathCurvature = MathUtils.Remap(pathCurvatureNoise, 0f, 1f, 0f, 0.5f);
                noiseMap[x, y] += scaledPathCurvature;
            }
        }
    }*/

    private static float GetSplineDisplacement(float noiseHeight, float splineX, int chunkWidth, int x)
    {
        float t = MathUtils.Remap(x, 0f, (float)(chunkWidth), 0f, 1f);
        float pathT = MathUtils.Remap(splineX, 0f, (float)(chunkWidth), 0f, 1f);
        return -(EasingFunctions.Spike(t + (0.5f - pathT)) * noiseHeight);
    }
}

public struct SplineDisplacementInfo
{
    public BezierSpline displacementSpline;
    public int chunkIndex;
    public int numCurvesChunk;
    public bool apply;

    public SplineDisplacementInfo(BezierSpline displacementSpline, int chunkIndex, int numCurvesChunk)
    {
        this.displacementSpline = displacementSpline;
        this.chunkIndex = chunkIndex;
        this.numCurvesChunk = numCurvesChunk;
        apply = true;
    }
}
