using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class LevelDataGenerator
{
    // Generate a heightmap given terrain structure and biome configuration
    public static float[,] GenerateHeightMap(TerrainStructure terrainStructure)
    {
        var result = new float[terrainStructure.HeightMapResolution, terrainStructure.HeightMapResolution];
        var cellSize = terrainStructure.MapSize / terrainStructure.HeightMapResolution;

        var octavesOffset = new Vector2[terrainStructure.Octaves];
        for (var i = 0; i < octavesOffset.Length; i++)
            octavesOffset[i] = new Vector2(Random.Range(-100000f, 100000f), Random.Range(-100000f, 100000f));

        // Generate heightmap
        for (var y = 0; y < terrainStructure.HeightMapResolution; y++)
        {
            for (var x = 0; x < terrainStructure.HeightMapResolution; x++)
            {
                var biomeHeight = terrainStructure.SampleHeight(new Vector2(x * cellSize, y * cellSize));
                var amplitude = 1f;
                var frequency = 1f;
                var noiseHeight = 0f;

                for (int i = 0; i < octavesOffset.Length; i++)
                {
                    var sampleX = (x + octavesOffset[i].x) / terrainStructure.MapSize * frequency *
                                  (biomeHeight.Scale * cellSize);
                    var sampleY = (y + octavesOffset[i].y) / terrainStructure.MapSize * frequency *
                                  (biomeHeight.Scale * cellSize);

                    /* Noise between -1 and 1 */
                    noiseHeight += (Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1) * amplitude;

                    amplitude *= biomeHeight.Persistence;
                    frequency *= biomeHeight.Lacunarity;
                }
                float normalizedHeight = Mathf.InverseLerp(-1f, 1f, noiseHeight);
                float globalHeight = (biomeHeight.LocalMax - biomeHeight.LocalMin) * normalizedHeight +
                                     biomeHeight.LocalMin;
                result[y, x] = globalHeight;
            }
        }

        return result;
    }

    // Set alphamap texture based on biome configuration
    public static float[,,] GenerateAlphaMap(TerrainStructure terrainStructure)
    {
        var result = new float[terrainStructure.HeightMapResolution, terrainStructure.HeightMapResolution,
            terrainStructure.TextureCount];
        var cellSize = terrainStructure.MapSize / terrainStructure.HeightMapResolution;

        for (int y = 0; y < terrainStructure.HeightMapResolution; y++)
        {
            for (int x = 0; x < terrainStructure.HeightMapResolution; x++)
            {
                KeyValuePair<int, float> sample = terrainStructure.SampleTexture(new Vector2(x * cellSize, y * cellSize));
                result[y, x, sample.Key] = sample.Value;
            }
        }
        return result;
    }

    // Smooth every cell in the alphamap using squareSize neighbors in each direction
    public static float[,,] SmoothAlphaMap(float[,,] alphamap, int squareSize)
    {
        var result = (float[,,])alphamap.Clone();
        var length = alphamap.GetLength(0);

        for (var y = 0; y < length; y++)
        {
            for (var x = 0; x < length; x++)
            {
                for (var i = 0; i < alphamap.GetLength(2); i++)
                {
                    var count = 0;
                    var sum = 0.0f;
                    for (var yN = y - squareSize; yN < y + squareSize; yN++)
                    {
                        for (var xN = x - squareSize; xN <= x + squareSize; xN++)
                        {
                            if (xN < 0 || xN >= length || yN < 0 || yN >= length)
                                continue;

                            sum += alphamap[xN, yN, i];
                            count++;
                        }
                    }
                    result[x, y, i] = sum / count;
                }
            }
        }
        return result;
    }

    // Draw roads onto the alpha and height maps
    public static void DrawPathLines(float[,] heightmap, float[,,] alphamap, int splatSize, float mapSize, int heightMapResolution, List<Vector2[]> pathLines, int textureCount, int splatIndex)
    {
        var cellSize = mapSize / heightMapResolution;

        // Find cells covered by the road polygon
        var cellsToSmooth = DiscretizeLines(heightMapResolution, cellSize, pathLines, splatSize, true)
            .ToArray();

        // Set alphamap values to only road draw
        foreach (var index in cellsToSmooth)
        {
            // Other textures to 0
            for (var i = 0; i < textureCount; i++)
                alphamap[index.x, index.y, i] = 0;

            // Road texture to 1
            alphamap[index.x, index.y, splatIndex] = 1;
        }

        SmoothHeightMapCells(heightmap, cellsToSmooth, splatSize + 2);
    }

    // Smooth every cell in the heightmap using squareSize neighbors in each direction
    public static void SmoothHeightMap(float[,] heightMap, int squareSize, int passes)
    {
        var temp = (float[,])heightMap.Clone();
        var length = heightMap.GetLength(0);

        while (passes > 0)
        {
            passes--;

            for (var y = 0; y < length; y++)
            {
                for (var x = 0; x < length; x++)
                {
                    var count = 0;
                    var sum = 0.0f;
                    for (var yN = y - squareSize; yN < y + squareSize; yN++)
                    {
                        for (var xN = x - squareSize; xN <= x + squareSize; xN++)
                        {
                            if (xN < 0 || xN >= length || yN < 0 || yN >= length)
                                continue;

                            sum += temp[xN, yN];
                            count++;
                        }
                    }
                    heightMap[x, y] = sum / count;
                }
            }
        }
    }

    // Smooth a heightmap along given lines
    public static void SmoothHeightMapWithLines(float[,] heightMap, float cellSize, IEnumerable<Vector2[]> lines, int lineWidth, int squareSize)
    {
        var length = heightMap.GetLength(0);
        var cellsToSmooth =
            new HashSet<Vector2Int>(DiscretizeLines(heightMap.GetLength(0), cellSize, lines, lineWidth, false));

        // Add extra cells to the line thickness
        var tempCopy = new HashSet<Vector2Int>(cellsToSmooth);
        foreach (var current in tempCopy)
        {
            for (var y = current.y - lineWidth; y < current.y + lineWidth; y++)
            {
                for (var x = current.x - lineWidth; x <= current.x + lineWidth; x++)
                {
                    if (x < 0 || x >= length || y < 0 || y >= length)
                        continue;

                    cellsToSmooth.Add(new Vector2Int(x, y));
                }
            }
        }
        SmoothHeightMapCells(heightMap, cellsToSmooth, squareSize);
    }

    // Generate blocking gameobjects along the coast to prevent players from going into the water
    public static GameObject GenerateAreaWalls(Terrain terrain, TerrainStructure terrainStructure, GameObject blocker, float blockerLength)
    {
        var result = new GameObject("Area Blockers");

        // Iterate over all coastal borders
        foreach (var line in terrainStructure.AreaBlockerLines)
        {
            var p0 = line[0];
            var p1 = line[1];

            //Discretize line and get direction normalized
            var direction = (p1 - p0).normalized;
            var numberOfBlockers = Mathf.CeilToInt((p1 - p0).magnitude / blockerLength) + 1;
            var lineGO = new GameObject("Area Blocker Line");
            lineGO.transform.parent = result.transform;

            //Instatiate each blocker with correct positions and orientations
            Transform lastTransform = null;
            for (var j = 0; j < numberOfBlockers; j++)
            {
                var position2D = p0 + direction * blockerLength * j;
                var position = new Vector3(position2D.x, 0, position2D.y) - terrain.transform.position;
                position = new Vector3(position.x, terrain.SampleHeight(position), position.z) +
                           terrain.transform.position;

                GameObject go;
                if (lastTransform == null)
                    go = Object.Instantiate(blocker);
                else
                {
                    var orientation = lastTransform.position - position;
                    go = Object.Instantiate(blocker);
                    go.transform.rotation = Quaternion.LookRotation(orientation.normalized, Vector3.up);
                    go.transform.localScale =
                        new Vector3(1, 2, 1 + (orientation.magnitude - blockerLength) / blockerLength);
                }
                go.transform.position = position;
                go.transform.parent = lineGO.transform;

                lastTransform = go.transform;
            }
        }

        return result;
    }


    // Generate blocking gameobjects along the coast to prevent players from going into the water
    public static GameObject GenerateOuterFences(Terrain terrain, TerrainStructure terrainStructure, GameObject blocker, GameObject pole, float blockerLength)
    {
        var result = new GameObject("Coast Blockers");

        // Iterate over all coastal borders
        Transform lastTransform = null;
        for (var i = 0; i < terrainStructure.BorderBlockerLines.Count; i++)
        {
            var p0 = terrainStructure.BorderBlockerLines[i];
            var p1 = i != terrainStructure.BorderBlockerLines.Count - 1
                ? terrainStructure.BorderBlockerLines[i + 1]
                : terrainStructure.BorderBlockerLines[0];

            //Discretize line and get direction normalized
            var direction = (p1 - p0).normalized;
            var numberOfBlockers = Mathf.CeilToInt((p1 - p0).magnitude / blockerLength);
            var line = new GameObject("Coast Blocker Line");
            line.transform.parent = result.transform;

            //Instatiate each blocker with correct positions and orientations
            for (var j = 0; j < numberOfBlockers; j++)
            {
                var position2D = p0 + direction * blockerLength * j;
                var position = new Vector3(position2D.x, 0, position2D.y) - terrain.transform.position;
                position = new Vector3(position.x, terrain.SampleHeight(position), position.z) +
                           terrain.transform.position;

                GameObject go;
                if (lastTransform == null)
                    go = Object.Instantiate(pole);
                else
                {
                    var orientation = lastTransform.position - position;
                    go = Object.Instantiate(blocker);
                    go.transform.rotation = Quaternion.LookRotation(orientation.normalized, Vector3.up);
                    go.transform.localScale =
                        new Vector3(1, 1, 1 + (orientation.magnitude - blockerLength) / blockerLength);
                }
                go.transform.position = position;
                go.transform.parent = line.transform;

                lastTransform = go.transform;
            }
        }

        return result;
    }

    // Instantiate all gameobjects that are part of the scenery
    public static GameObject[] GenerateScenery(Terrain terrain)
    {
        GameObject[] result = new GameObject[0];

        //TODO: implement

        return result;
    }

    //---------------------------------------------------------------------
    // 
    // HELPER PRIVATE FUNCTIONS
    // 
    //---------------------------------------------------------------------

    // Smooth cells using a 2*neighborcount + 1 square around each cell
    private static void SmoothHeightMapCells(float[,] heightMap, IEnumerable<Vector2Int> cellsToSmooth, int squareSize)
    {
        var temp = (float[,])heightMap.Clone();
        var length = heightMap.GetLength(0);

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

                    sum += temp[x, y];
                    count++;
                }
            }
            heightMap[cell.x, cell.y] = sum / count;
        }
    }

    // Match multiple lines to cells in a grid
    private static IEnumerable<Vector2Int> DiscretizeLines(int resolution, float cellSize, IEnumerable<Vector2[]> lines, int lineWidth, bool addNoise)
    {
        var result = new HashSet<Vector2Int>();
        foreach (var line in lines)
        {
            result.UnionWith(BresenhamLine(resolution, cellSize, line, lineWidth, addNoise));
        }
        return result;
    }

    // Match a line to cells in a grid
    private static HashSet<Vector2Int> BresenhamLine(int resolution, float cellSize, IList<Vector2> line, int lineWidth,
        bool addNoise)
    {
        var result = new HashSet<Vector2Int>();

        // Iterate over the edges using Bresenham's Algorithm
        var startY = Mathf.Min(resolution - 1, Mathf.FloorToInt(line[0].x / cellSize));
        var startX = Mathf.Min(resolution - 1, Mathf.FloorToInt(line[0].y / cellSize));
        var current = new Vector2Int(startX, startY);

        var endY = Mathf.Min(resolution - 1, Mathf.FloorToInt(line[1].x / cellSize));
        var endX = Mathf.Min(resolution - 1, Mathf.FloorToInt(line[1].y / cellSize));
        var end = new Vector2Int(endX, endY);


        //https://stackoverflow.com/questions/11678693/all-cases-covered-bresenhams-line-algorithm
        int w = end.x - current.x;
        int h = end.y - current.y;

        int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;

        if (w < 0) dx1 = -1;
        else if (w > 0) dx1 = 1;
        if (h < 0) dy1 = -1;
        else if (h > 0) dy1 = 1;
        if (w < 0) dx2 = -1;
        else if (w > 0) dx2 = 1;

        int longest = Mathf.Abs(w);
        int shortest = Mathf.Abs(h);
        if (!(longest > shortest))
        {
            longest = Mathf.Abs(h);
            shortest = Mathf.Abs(w);
            if (h < 0) dy2 = -1;
            else if (h > 0) dy2 = 1;
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

        var temp = new HashSet<Vector2Int>(result);
        foreach (var cell in temp)
        {
            int random = Random.Range(0, 5);
            int top = 0, bottom = 0, right = 0, left = 0;
            switch (random)
            {
                case 0:
                    top = 1;
                    break;
                case 1:
                    bottom = 1;
                    break;
                case 2:
                    right = 1;
                    break;
                case 3:
                    left = 1;
                    break;
                default:
                    break;
            }

            for (var y = -(lineWidth + bottom); y < lineWidth + top; y++)
            {
                for (var x = -(lineWidth + left); x < lineWidth + right; x++)
                {
                    var neighbor = cell + new Vector2Int(x, y);
                    if (neighbor.x < 0 || neighbor.x >= resolution || neighbor.y < 0 ||
                        neighbor.x >= resolution) continue;

                    result.Add(neighbor);
                }
            }
        }

        return result;
    }
}