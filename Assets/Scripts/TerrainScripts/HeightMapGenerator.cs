using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Comparers;

public static class HeightMapGenerator {

    public static float[,] GenerateHeightMap(TerrainStructure terrainStrucure, BiomeDistribution biomeDistribution)
    {
        var result = new float[biomeDistribution.MapResolution, biomeDistribution.MapResolution];

        var octavesOffset = new Vector2[biomeDistribution.Octaves];
        for (var i = 0; i < octavesOffset.Length; i++)
            octavesOffset[i] = new Vector2(Random.Range(-100000f, 100000f), Random.Range(-100000f, 100000f));

        /* Generate heightmap */
        for (var y = 0; y < biomeDistribution.MapResolution; y++)
        {
            for (var x = 0; x < biomeDistribution.MapResolution; x++)
            {
                var biomeHeight = terrainStrucure.SampleBiomeHeight(new Vector2(x, y));
                var amplitude = 1f;
                var frequency = 1f;
                var noiseHeight = 0f;

                for(int i = 0; i < octavesOffset.Length; i++)
                {
                    var sampleX = (x + octavesOffset[i].x) / biomeDistribution.MapResolution * frequency * biomeHeight.Scale;
                    var sampleY = (y + octavesOffset[i].y) / biomeDistribution.MapResolution * frequency * biomeHeight.Scale;

                    /* Noise between -1 and 1 */
                    noiseHeight += (Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1) * amplitude;

                    amplitude *= biomeHeight.Persistence;
                    frequency *= biomeHeight.Lacunarity;
                }
                float normalizedHeight = Mathf.InverseLerp(-1f, 1f, noiseHeight);
                float globalHeight = (biomeHeight.LocalMax - biomeHeight.LocalMin) * normalizedHeight + biomeHeight.LocalMin;
                result[y, x] = globalHeight;
            }
        }

        return result;
    }
}
