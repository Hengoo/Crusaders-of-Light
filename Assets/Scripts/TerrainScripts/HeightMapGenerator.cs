using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Comparers;

public static class HeightMapGenerator {

    public static float[,] GenerateHeightMap(TerrainStructure terrainStrucure, BiomeDistribution biomeDistribution)
    {
        var result = new float[biomeDistribution.MapResolution, biomeDistribution.MapResolution];

        /* Generate heightmap */
        for (var y = 0; y < biomeDistribution.MapResolution; y++)
        {
            for (var x = 0; x < biomeDistribution.MapResolution; x++)
            {
                result[x, y] = terrainStrucure.SampleBiomeHeight(new Vector2(x, y));
            }
        }

        return result;
    }
}
