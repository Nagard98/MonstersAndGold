using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SplatMap
{
    public static float[,,] GenerateSplatMap(int mapSize, SplatHeight[] splatHeights, float[,] noiseMap)
    {
        float[,,] splatmaps = new float[mapSize, mapSize, 2];

        for (int x = 0; x < mapSize; x++)
        {
            for (int y = 0;  y < mapSize;  y++)
            {
                for (int i = 0; i < splatHeights.Length; i++)
                {
                    //TODO: generalizza a più layer
                    float height = splatHeights[i].height;
                    float overlap = splatHeights[i].overlap;

                    float mapHeight = noiseMap[x, y];

                    if (i == splatHeights.Length - 1)
                    {
                        splatmaps[y, x, splatHeights[i].layerIndex] = (mapHeight >= height) ? (mapHeight >= (height + splatHeights[i - 1].overlap) ? 1 : 0.5f) : 0;
                    }
                    else
                    {
                        float nextHeight = splatHeights[i + 1].height;
                        splatmaps[y, x, splatHeights[i].layerIndex] = (mapHeight >= height && mapHeight <= nextHeight + overlap) ? (mapHeight >= nextHeight ? 0.5f : 1) : 0;
                    }
                }
            }
        }
        return splatmaps;

    }

    public static Color[] ConvertToColors(float[,,] splatMaps, int mapSize, SplatHeight[] splatHeights)
    {
        Color[] colors = new Color[mapSize * mapSize];

        Color[] splatmapColors = new Color[2];
        splatmapColors[0] = new Color(255, 0, 0, 0);
        splatmapColors[1] = new Color(0, 255, 0, 0);

        for (int x = 0; x < mapSize; x++)
        {
            for (int y = 0; y < mapSize; y++)
            {
                for (int i = 0; i < splatHeights.Length; i++)
                {
                    
                        colors[x * mapSize + y] += (splatmapColors[i] * splatMaps[x,y,i]);
                    
                }
            }
        }

        return colors;
    }
}
