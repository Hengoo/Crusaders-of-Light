using System.Collections;
using System.Collections.Generic;
using csDelaunay;
using TriangleNet.Smoothing;
using UnityEngine;
using UnityEngine.Assertions.Comparers;

public static class HeightMapManager
{
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

                for (int i = 0; i < octavesOffset.Length; i++)
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

    public static float[,] SmoothBiomeEdges(float[,] heightMap, float cellSize, IEnumerable<Edge> edges, int neighborCount)
    {
        var result = (float[,])heightMap.Clone();
        int length = heightMap.GetLength(0);

        var cellsToSmooth = new HashSet<Vector2Int>();


        // Find which cells to smooth (border cells)
        foreach (var edge in edges)
        {
            if (!edge.Visible())
                continue;

            var edgeVector = edge.ClippedEnds[LR.LEFT] - edge.ClippedEnds[LR.RIGHT];

            var x = edgeVector.x;
            var y = edgeVector.y;
            var baseX = Mathf.FloorToInt(edge.ClippedEnds[LR.RIGHT].x / cellSize);
            var baseY = Mathf.FloorToInt(edge.ClippedEnds[LR.RIGHT].y / cellSize);

            int offsetX = 0, offsetY = 0;
            int xDir = 1, yDir = 1;

            if (x < 0)
            {
                x = -x;
                xDir = -1;
                offsetX = Mathf.FloorToInt(edge.ClippedEnds[LR.LEFT].x / cellSize) - baseX;
            }
            if (y < 0)
            {
                y =  -y;
                yDir = -1;
                offsetY = Mathf.FloorToInt(edge.ClippedEnds[LR.LEFT].y / cellSize) - baseY;
            }

            while (x > 0 && y > 0)
            {
                cellsToSmooth.Add(new Vector2Int(baseX + offsetX, baseY + offsetY));
                if (x > y)
                {
                    x -= cellSize;
                    offsetX += xDir;
                }
                else
                {
                    y -= cellSize;
                    offsetY += yDir;
                }

                Debug.Log(baseX + offsetX + " " + baseY + offsetY);
            }

        }

        // Smooth cells using a 2*neighborcount + 1 square around each cell
        foreach (var cell in cellsToSmooth)
        {
            int count = 0;
            float sum = 0;
            for (int y = cell.y - neighborCount; y < cell.y + neighborCount; y++)
            {
                for (int x = cell.x - neighborCount; x <= cell.x + neighborCount; x++)
                {
                    if (x < 0 || x >= length || y < 0 || y >= length)
                        continue;

                    sum += heightMap[y, x];
                    count++;
                }
            }

            result[cell.y, cell.x] = sum / count;
        }
        return result;
    }
}
