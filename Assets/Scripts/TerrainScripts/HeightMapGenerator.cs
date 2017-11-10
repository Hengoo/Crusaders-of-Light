using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Comparers;

public static class HeightMapGenerator {

    public static float[,] GenerateHeightMap(TerrainStructure terrainStrucure, BiomeDistribution biomeDistribution)
    {
        var result = new float[biomeDistribution.MapResolution, biomeDistribution.MapResolution];
        var cellSize = new Vector2((float)biomeDistribution.MapResolution / biomeDistribution.XCells, (float)biomeDistribution.MapResolution / biomeDistribution.YCells);

        /* Create offsets in each perlin noise octave for variety */
        var octaveOffsets = new Vector2[8];
        octaveOffsets[0].x = biomeDistribution.PerlinOffset.x;
        octaveOffsets[0].y = biomeDistribution.PerlinOffset.y;
        for (var i = 1; i < octaveOffsets.Length; i++)
        {
            octaveOffsets[i].x = Random.Range(-100000, 100000) + biomeDistribution.PerlinOffset.x;
            octaveOffsets[i].y = Random.Range(-100000, 100000) - biomeDistribution.PerlinOffset.y;
        }

        /* Generate heightmap */
        for (var y = 0; y < biomeDistribution.MapResolution; y++)
        {
            for (var x = 0; x < biomeDistribution.MapResolution; x++)
            {
                var pos = new Vector2(x * cellSize.x, y * cellSize.y);

                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                var biomeNoise = terrainStrucure.SampleBiomeNoise(new Vector2(x, y));
                
                /* Calculate fractal perlin noise */
                for (int i = 0; i < biomeDistribution.Octaves; i++)
                {
                    float sampleX = (pos.x + octaveOffsets[i].x) / biomeDistribution.PerlinScale * frequency;
                    float sampleY = (pos.y + octaveOffsets[i].y) / biomeDistribution.PerlinScale * frequency;
                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;

                    noiseHeight += perlinValue * amplitude;

                    amplitude *= biomeNoise.Persistence;
                    frequency *= biomeNoise.Lacunarity;
                }

                /* Normalize noise accourding to global max height and local biome planarity*/
                Debug.Log("Before: " + noiseHeight + " - LocalMax: " + biomeNoise.LocalMax);
                noiseHeight = Mathf.Clamp(noiseHeight, -.707f, .707f);
                noiseHeight = Mathf.InverseLerp(-.707f, .707f, noiseHeight);
                result[x, y] = noiseHeight * (biomeNoise.LocalMax - biomeNoise.LocalMin) + biomeNoise.LocalMin;
                Debug.Log("After: " + result[x, y]);
            }
        }


        return result;
    }
}
