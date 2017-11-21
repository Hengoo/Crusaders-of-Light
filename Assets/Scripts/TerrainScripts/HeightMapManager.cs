using System.Collections;
using System.Collections.Generic;
using System.Linq;
using csDelaunay;
using TriangleNet.Smoothing;
using UnityEngine;
using UnityEngine.Assertions.Comparers;

public static class HeightMapManager
{
    public static float[,] GenerateHeightMap(TerrainStructure terrainStrucure, BiomeDistribution biomeDistribution)
    {
        var result = new float[biomeDistribution.HeightMapResolution, biomeDistribution.HeightMapResolution];

        var cellSize = biomeDistribution.MapSize / biomeDistribution.HeightMapResolution;

        var octavesOffset = new Vector2[biomeDistribution.Octaves];
        for (var i = 0; i < octavesOffset.Length; i++)
            octavesOffset[i] = new Vector2(Random.Range(-100000f, 100000f), Random.Range(-100000f, 100000f));

        /* Generate heightmap */
        for (var y = 0; y < biomeDistribution.HeightMapResolution; y++)
        {
            for (var x = 0; x < biomeDistribution.HeightMapResolution; x++)
            {
                var biomeHeight = terrainStrucure.SampleBiomeHeight(new Vector2(x * cellSize, y * cellSize));
                var amplitude = 1f;
                var frequency = 1f;
                var noiseHeight = 0f;

                for (int i = 0; i < octavesOffset.Length; i++)
                {
                    var sampleX = (x + octavesOffset[i].x) / biomeDistribution.MapSize * frequency * (biomeHeight.Scale * cellSize);
                    var sampleY = (y + octavesOffset[i].y) / biomeDistribution.MapSize * frequency * (biomeHeight.Scale * cellSize);

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

    public static float[,] SmoothHeightMapWithLines(float[,] heightMap, float cellSize, IEnumerable<LineSegment> lines, int lineWidth, int squareSize)
    {
        var result = (float[,])heightMap.Clone();
        int length = heightMap.GetLength(0);

        var cellsToSmooth = new HashSet<Vector2Int>(BresenhamLine(heightMap.GetLength(0), cellSize, lines, lineWidth));

        // Add extra cells to the line thickness
        var tempCopy = new HashSet<Vector2Int>(cellsToSmooth);
        foreach (var current in tempCopy)
        {
            for (int y = current.y - lineWidth; y < current.y + lineWidth; y++)
            {
                for (int x = current.x - lineWidth; x <= current.x + lineWidth; x++)
                {
                    if (x < 0 || x >= length || y < 0 || y >= length)
                        continue;

                    cellsToSmooth.Add(new Vector2Int(x, y));
                }
            }
        }

        // Smooth cells using a 2*neighborcount + 1 square around each cell
        foreach (var cell in cellsToSmooth)
        {
            var count = 0;
            var sum = 0.0f;
            for (int y = cell.y - squareSize; y < cell.y + squareSize; y++)
            {
                for (int x = cell.x - squareSize; x <= cell.x + squareSize; x++)
                {
                    if (x < 0 || x >= length || y < 0 || y >= length)
                        continue;

                    sum += heightMap[x, y];
                    count++;
                }
            }
            result[cell.x, cell.y] = sum / count;
        }
        return result;
    }

    private static IEnumerable<Vector2Int> BresenhamLine(int resolution, float cellSize, IEnumerable<LineSegment> lines, int lineWidth)
    {
        var result = new HashSet<Vector2Int>();

        // Iterate over the edges using Bresenham's Algorithm
        foreach (var line in lines)
        {
            var startY = Mathf.Min(resolution - 1, Mathf.FloorToInt(line.p0.x / cellSize));
            var startX = Mathf.Min(resolution - 1, Mathf.FloorToInt(line.p0.y / cellSize));
            var current = new Vector2Int(startX, startY);

            var endY = Mathf.Min(resolution - 1, Mathf.FloorToInt(line.p1.x / cellSize));
            var endX = Mathf.Min(resolution - 1, Mathf.FloorToInt(line.p1.y / cellSize));
            var end = new Vector2Int(endX, endY);


            //https://stackoverflow.com/questions/11678693/all-cases-covered-bresenhams-line-algorithm
            int w = end.x - current.x;
            int h = end.y - current.y;

            int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;

            if (w < 0) dx1 = -1; else if (w > 0) dx1 = 1;
            if (h < 0) dy1 = -1; else if (h > 0) dy1 = 1;
            if (w < 0) dx2 = -1; else if (w > 0) dx2 = 1;

            int longest = Mathf.Abs(w);
            int shortest = Mathf.Abs(h);
            if (!(longest > shortest))
            {
                longest = Mathf.Abs(h);
                shortest = Mathf.Abs(w);
                if (h < 0) dy2 = -1; else if (h > 0) dy2 = 1;
                dx2 = 0;
            }
            int numerator = longest >> 1;
            for (int i = 0; i <= longest; i++)
            {
                result.Add(current);
                numerator += shortest;
                if (!(numerator < longest))
                {
                    numerator -= longest;
                    current += new Vector2Int(dx1, dy1);
                }
                else
                {
                    current += new Vector2Int(dx2, dy2);
                }
            }
        }

        return result;
    }
}
