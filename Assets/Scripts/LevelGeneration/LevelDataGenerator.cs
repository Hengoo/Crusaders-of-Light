using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

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

	/// <summary>
	/// Smoothes the 3d arary.
	/// </summary>
	/// <param name="IOArray">IO float array</param>
	/// <param name="squareSize"></param>
	/// <param name="loop">number of loops</param>
	public static void SmoothHeightMap(float[,,] IOArray, int squareSize, int loop)
	{
		//variables
		int arraySize = IOArray.GetLength(0);
		ComputeBuffer heightmap;
		ComputeBuffer heightmapTmp;

		//find compute shader:
		ComputeShader computeShader = (ComputeShader)Resources.Load("Smoothing");

		//set kernel ids:
		int kernelSmooth = computeShader.FindKernel("Smooth");
		int kernelPrepArray = computeShader.FindKernel("PrepArray");

		heightmap = new ComputeBuffer(arraySize * arraySize, sizeof(float), ComputeBufferType.Default);
		heightmapTmp = new ComputeBuffer(arraySize * arraySize, sizeof(float), ComputeBufferType.Default);


		float[] mapTmp = new float[arraySize * arraySize];


		for (int j =0; j< IOArray.GetLength(2); j++)
		{
			//i found no better war for this...
			for (int x = 0; x < arraySize; x++)
			{
				for (int y = 0; y < arraySize; y++)
				{
					mapTmp[x + y * arraySize] = IOArray[x, y, j];

				}
			}

			//better version of above but wrong oder. (it has color1,color2,color3 color1,color2,color3,...instead of a full array with color1
			//System.Buffer.BlockCopy(arr, arraySize* arraySize * sizeof(float)*j, mapTmp, 0, arraySize* arraySize * sizeof(float));

			heightmap.SetData(mapTmp);

			computeShader.SetInt("arraySize", arraySize);
			computeShader.SetInt("squareSize", squareSize);
			computeShader.SetBuffer(kernelPrepArray, "heightmapBuffer", heightmap);
			computeShader.SetBuffer(kernelPrepArray, "heightmapTmpBuffer", heightmapTmp);
			computeShader.SetBuffer(kernelSmooth, "heightmapBuffer", heightmap);
			computeShader.SetBuffer(kernelSmooth, "heightmapTmpBuffer", heightmapTmp);

			for (int i = 0; i < loop; i++)
			{
				computeShader.Dispatch(kernelPrepArray, (arraySize * arraySize) / 64, 1, 1);

				computeShader.Dispatch(kernelSmooth, arraySize / 16, arraySize / 16, 1);
			}


			//read data: skip this for performance testing on compoute code
			heightmap.GetData(mapTmp);


			//System.Buffer.BlockCopy(mapTmp, 0, arr, arraySize* arraySize * sizeof(float)*j, arraySize* arraySize * sizeof(float));

			for (int x = 0; x < arraySize; x++)
			{
				for (int y = 0; y < arraySize; y++)
				{
					IOArray[x, y, j] = mapTmp[x + y * arraySize];
				}
			}
		}





		//cleanup afterwards:
		heightmap.Release();
		heightmapTmp.Release();
	}

	/// <summary>
	/// Smoothes the float array. Expects squared arrays
	/// </summary>
	/// <param name="IOArray">IO float array</param>
	/// <param name="squareSize"></param>
	/// <param name="loop">number of loops</param>
	public static void SmoothHeightMap(float[,] IOArray, int squareSize, int loop)
    {
        //variables
        int arraySize = IOArray.GetLength(0);
        ComputeBuffer heightmap;
        ComputeBuffer heightmapTmp;

        //find compute shader:
        ComputeShader computeShader = (ComputeShader)Resources.Load("Smoothing");

		//set kernel ids:
		int kernelSmooth = computeShader.FindKernel("Smooth");
        int kernelPrepArray = computeShader.FindKernel("PrepArray");

        heightmap = new ComputeBuffer(arraySize * arraySize, sizeof(float), ComputeBufferType.Default);
        heightmapTmp = new ComputeBuffer(arraySize * arraySize, sizeof(float), ComputeBufferType.Default);


        float[] mapTmp = new float[arraySize * arraySize];
        //for (int i = 0; i < arraySize; i++)
        //{
        //	for (int j = 0; j < arraySize; j++)
        //	{
        //		mapTmp[j * arraySize + i] = IOArray[i, j];
        //	}
        //}

        System.Buffer.BlockCopy(IOArray, 0, mapTmp, 0, IOArray.Length * sizeof(float));

        heightmap.SetData(mapTmp);

        computeShader.SetInt("arraySize", arraySize);
        computeShader.SetInt("squareSize", squareSize);
        computeShader.SetBuffer(kernelPrepArray, "heightmapBuffer", heightmap);
        computeShader.SetBuffer(kernelPrepArray, "heightmapTmpBuffer", heightmapTmp);
        computeShader.SetBuffer(kernelSmooth, "heightmapBuffer", heightmap);
        computeShader.SetBuffer(kernelSmooth, "heightmapTmpBuffer", heightmapTmp);
        for (int i = 0; i < loop; i++)
        {
            computeShader.Dispatch(kernelPrepArray, (arraySize * arraySize) / 64, 1, 1);

            computeShader.Dispatch(kernelSmooth, arraySize / 16, arraySize / 16, 1);
        }

        //read data: skip this for performance testing on compoute code
        heightmap.GetData(mapTmp);


        //for (int i = 0; i < arraySize; i++)
        //{
        //	for (int j = 0; j < arraySize; j++)
        //	{
        //		IOArray[j, i] = mapTmp[j * arraySize + i];
        //	}
        //}
        System.Buffer.BlockCopy(mapTmp, 0, IOArray, 0, mapTmp.Length * sizeof(float));

        //cleanup afterwards:
        heightmap.Release();
        heightmapTmp.Release();
    }

    /// <summary>
    /// smoothes the array with the weights defined in mask
    /// </summary>
    /// <param name="IOArray">IO float array</param>
    /// <param name="mask">mask array. same sice as IOArray</param>
    /// <param name="squareSize"></param>
    /// <param name="loop">number of loops</param>
    public static void SmoothHeightMap(float[,] IOArray, float[,] mask, int squareSize, int loop)
    {
        //variables
        int arraySize = IOArray.GetLength(0);
        ComputeBuffer heightmap;
        ComputeBuffer heightmapTmp;
        ComputeBuffer maskBuffer;

        //find compute shader:
        ComputeShader computeShader = (ComputeShader)Resources.Load("Smoothing");

        //set kernel ids:
        int kernelSmooth = computeShader.FindKernel("SmoothMask");
        int kernelPrepArray = computeShader.FindKernel("PrepArray");

        heightmap = new ComputeBuffer(arraySize * arraySize, sizeof(float), ComputeBufferType.Default);
        heightmapTmp = new ComputeBuffer(arraySize * arraySize, sizeof(float), ComputeBufferType.Default);
        maskBuffer = new ComputeBuffer(arraySize * arraySize, sizeof(float), ComputeBufferType.Default);


        float[] mapTmp = new float[arraySize * arraySize];
        //for (int i = 0; i < arraySize; i++)
        //{
        //	for (int j = 0; j < arraySize; j++)
        //	{
        //		mapTmp[j * arraySize + i] = IOArray[i, j];
        //	}
        //}

        System.Buffer.BlockCopy(IOArray, 0, mapTmp, 0, IOArray.Length * sizeof(float));

        float[] maskTmp = new float[arraySize * arraySize];
        System.Buffer.BlockCopy(mask, 0, maskTmp, 0, mask.Length * sizeof(float));

        //set buffers
        heightmap.SetData(mapTmp);
        maskBuffer.SetData(maskTmp);

        computeShader.SetInt("arraySize", arraySize);
        computeShader.SetInt("squareSize", squareSize);
        computeShader.SetBuffer(kernelPrepArray, "heightmapBuffer", heightmap);
        computeShader.SetBuffer(kernelPrepArray, "heightmapTmpBuffer", heightmapTmp);
        computeShader.SetBuffer(kernelSmooth, "heightmapBuffer", heightmap);
        computeShader.SetBuffer(kernelSmooth, "heightmapTmpBuffer", heightmapTmp);
        computeShader.SetBuffer(kernelSmooth, "maskBuffer", maskBuffer);

        for (int i = 0; i < loop; i++)
        {
            computeShader.Dispatch(kernelPrepArray, (arraySize * arraySize) / 64, 1, 1);

            computeShader.Dispatch(kernelSmooth, arraySize / 16, arraySize / 16, 1);
        }

        //read data: skip this for performance testing on compoute code
        heightmap.GetData(mapTmp);


        //for (int i = 0; i < arraySize; i++)
        //{
        //	for (int j = 0; j < arraySize; j++)
        //	{
        //		IOArray[j, i] = mapTmp[j * arraySize + i];
        //	}
        //}
        System.Buffer.BlockCopy(mapTmp, 0, IOArray, 0, mapTmp.Length * sizeof(float));

        //cleanup afterwards:
        heightmap.Release();
        heightmapTmp.Release();
        maskBuffer.Release();
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
    public static void DrawStraightPathLines(float[,] heightmap, float[,,] alphamap, int splatSize, float mapSize, int heightMapResolution, List<Vector2[]> pathLines, int textureCount, int splatIndex)
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
    public static void SmoothHeightMap(float[,] heightMap, int squareSize)
    {
        var temp = (float[,])heightMap.Clone();
        var length = heightMap.GetLength(0);

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
    public static GameObject GenerateBlockerLine(Terrain terrain, List<Vector2[]> blockerLines, float blockerLength, Vector3 positionNoise, Vector3 scaleNoise, GameObject blocker, bool useTerrainNormal = false, GameObject pole = null, float angleLimit = 35)
    {
        var result = new GameObject("Blockers");
        var polePrefab = pole ? pole : blocker;

        // Iterate over all border blockers
        foreach (var line in blockerLines)
        {
            var p0 = line[0];
            var p1 = line[1];
            float lineLength = (p0 - p1).magnitude;

            //Discretize line and get direction normalized
            Vector2 direction = (p1 - p0).normalized;
            int numberOfBlockers;
            if (lineLength < blockerLength)
                numberOfBlockers = 1;
            else
              numberOfBlockers = Mathf.FloorToInt((p1 - p0).magnitude / blockerLength);
            float lengthCorrection = ((p1 - p0).magnitude - numberOfBlockers * blockerLength) / numberOfBlockers;
            GameObject areaSegmentLine = new GameObject("Blocker Line");
            areaSegmentLine.transform.parent = result.transform;

            //Instatiate each blocker with correct positions and orientations
            Transform lastTransform = null;
            for (var j = 0; j < numberOfBlockers + 1; j++)
            {
                var position2D = p0 + direction * (blockerLength + lengthCorrection) * j;
                var position = new Vector3(position2D.x, 0, position2D.y);

                position -= terrain.transform.position;
                position = new Vector3(position.x, terrain.SampleHeight(position), position.z) +
                           terrain.transform.position;

                var extraScale = new Vector3(Random.Range(-scaleNoise.x, scaleNoise.x), Random.Range(-scaleNoise.y, scaleNoise.y), Random.Range(-scaleNoise.z, scaleNoise.z));
                var extraPosition = new Vector3(Random.Range(-positionNoise.x, positionNoise.x), Random.Range(-positionNoise.y, positionNoise.y), Random.Range(-positionNoise.z, positionNoise.z));

                GameObject go;
                if (lastTransform == null)
                {
                    go = Object.Instantiate(polePrefab);
                    go.transform.rotation = useTerrainNormal ?
                        terrain.GetNormalRotation(position) :
                        Quaternion.Euler(blocker.transform.eulerAngles + Quaternion.LookRotation(new Vector3(direction.x, 0, direction.y).normalized, Vector3.up).eulerAngles);
                }
                else
                {
                    var orientation = lastTransform.position - position;
                    go = Object.Instantiate(blocker);

                    go.transform.rotation = useTerrainNormal ?
                        terrain.GetNormalRotation(position) :
                        Quaternion.Euler(blocker.transform.eulerAngles + Quaternion.LookRotation(orientation.normalized, Vector3.up).eulerAngles);

                    go.transform.localScale = blocker.transform.localScale +
                        new Vector3(0, 0, lengthCorrection) / blockerLength +
                        new Vector3(Random.Range(0, 0.01f), 0, Mathf.Clamp((orientation.magnitude - (blockerLength + lengthCorrection)) / (blockerLength + lengthCorrection), 0, .2f));
                }
                go.transform.localScale += extraScale;
                go.transform.position = position + extraPosition;
                go.transform.parent = areaSegmentLine.transform;
                go.CorrectAngleTolerance(angleLimit);
                var navMod = go.AddComponent<NavMeshModifier>();
                navMod.overrideArea = true;
                navMod.area = NavMesh.GetAreaFromName("Not Walkable");
                lastTransform = go.transform;
            }
        }
        return result;
    }

    // Instantiate all gameobjects that are part of the scenery
    public static GameObject[] GenerateScenery(Terrain terrain, IEnumerable<AreaSettings> areas)
    {
        List<GameObject> result = new List<GameObject>();

        foreach (var area in areas)
        {
            var areaGO = area.GenerateAreaScenery(terrain);
            foreach (var data in area.PoissonDataList)
            {
                var poissonGO = PoissonDiskFill(terrain, data, area.Name + " Fill");
                poissonGO.transform.parent = areaGO.transform;
            }
            result.Add(areaGO);
        }

        return result.ToArray();
    }


    //---------------------------------------------------------------------
    // 
    // HELPER PRIVATE FUNCTIONS
    // 
    //---------------------------------------------------------------------

    // Fill an area with prefabs 
    private static GameObject PoissonDiskFill(Terrain terrain, PoissonDiskFillData poissonDiskFillData, string name = "Scenery Area")
    {
        var result = new GameObject(name);
        result.transform.position = Vector3.zero;
        result.transform.rotation = Quaternion.identity;

        if (poissonDiskFillData.Prefabs == null || poissonDiskFillData.Prefabs.Length <= 0)
            return result;

        var levelCreator = terrain.transform.parent.GetComponent<LevelCreator>();
        var size = poissonDiskFillData.FrameSize;
        PoissonDiskGenerator.minDist = poissonDiskFillData.MinDist;
        PoissonDiskGenerator.sampleRange = (size.x > size.y ? size.x : size.y);
        PoissonDiskGenerator.Generate();
        foreach (var sample in PoissonDiskGenerator.ResultSet)
        {
            var point = sample + poissonDiskFillData.FramePosition;
            var height = terrain.SampleHeight(new Vector3(point.x, 0, point.y) - terrain.transform.position);
            if (height <= (levelCreator.WaterHeight + 0.01f) * terrain.terrainData.size.y || // not underwater
                !point.IsInsidePolygon(poissonDiskFillData.Polygon) || //not outside of the area
                !poissonDiskFillData.ClearPolygons.TrueForAll(a => !point.IsInsidePolygon(a)) //not inside of any clear polygon
            )
                continue;

            var go = Object.Instantiate(poissonDiskFillData.Prefabs[Random.Range(0, poissonDiskFillData.Prefabs.Length)]);
            go.transform.position = new Vector3(point.x, height, point.y) + terrain.transform.position;
            go.transform.rotation = terrain.GetNormalRotation(go.transform.position) * Quaternion.Euler(go.transform.rotation.eulerAngles.x, Random.Range(0, 360f), go.transform.rotation.eulerAngles.z);
            go.CorrectAngleTolerance(poissonDiskFillData.AngleTolerance);
            go.transform.parent = result.transform;

            if (poissonDiskFillData.NotWalkable)
            {
                var navMod = go.AddComponent<NavMeshModifier>();
                navMod.overrideArea = true;
                navMod.area = NavMesh.GetAreaFromName("Not Walkable");
            }
        }

        return result;
    }

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