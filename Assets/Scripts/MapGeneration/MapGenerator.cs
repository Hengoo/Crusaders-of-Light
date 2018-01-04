using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class MapGenerator : MonoBehaviour
{

    public enum DrawModeEnum
    {
        BiomeGraph,
        GameMap,
        AreaGraph
    }

    public DrawModeEnum DrawMode = DrawModeEnum.BiomeGraph;
    public BiomeConfiguration BiomeConfiguration;
    public List<BiomeSettings> AvailableBiomes;
    
    public int NumberOfAreas = 3;
    public int ExtraEdges = 20;
    public bool FillTerrain = true;
    public float RoadHalfWidth = 10;
    public int Seed = 0;

    /* Debug variables */
    private TerrainStructure _terrainStructure;
    private WorldStructure _worldStructure;
    private SceneryStructure _sceneryStructure;

    /* Redraws preview in the scene editor */
    public void GeneratePreview()
    {
        if (Application.isPlaying)
            return;

        ClearDisplay();
        Random.InitState(Seed);

        _terrainStructure = new TerrainStructure(AvailableBiomes, BiomeConfiguration);
        _worldStructure = new WorldStructure(_terrainStructure, NumberOfAreas, ExtraEdges);
        _sceneryStructure = new SceneryStructure(_terrainStructure, _worldStructure, RoadHalfWidth);

        switch (DrawMode)
        {
            case DrawModeEnum.BiomeGraph:
                DrawGraph();
                break;
            case DrawModeEnum.GameMap:
                DrawGameMap();
                break;
            case DrawModeEnum.AreaGraph:
                DrawAreaGraph();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    void DrawGameMap()
    {
        /* Create heightmap */
        var heightMap = MapDataGenerator.GenerateHeightMap(_terrainStructure);
        
        /* Create splat textures alphamap */
        var alphamap = MapDataGenerator.GenerateAlphaMap(_terrainStructure);

        /* Draw borders */
        MapDataGenerator.EncloseAreas(_terrainStructure,  heightMap, _worldStructure.AreaBorders, 3);

        /* Draw roads onto alphamap */
        MapDataGenerator.DrawLineRoads(_terrainStructure, heightMap, alphamap, _sceneryStructure.RoadLines, 3);

        /* Smoothing passes */
        alphamap = MapDataGenerator.SmoothAlphaMap(alphamap, 1);
        if (BiomeConfiguration.SmoothEdges)
        {
            //Smooth only navigable biome borders
            MapDataGenerator.SmoothHeightMapWithLines(heightMap, BiomeConfiguration.MapSize / BiomeConfiguration.HeightMapResolution, _terrainStructure.GetBiomeSmoothBorders(), BiomeConfiguration.EdgeWidth, BiomeConfiguration.SquareSize);

            //Smooth all biome borders
            //heightMap = MapDataGenerator.SmoothHeightMapWithLines(heightMap, BiomeConfiguration.MapSize / BiomeConfiguration.HeightMapResolution, _terrainStructure.GetBiomeBorders(), 3, 2);

            //Overall smoothing
            if (BiomeConfiguration.OverallSmoothing > 0)
            {
                MapDataGenerator.SmoothHeightMap(heightMap, BiomeConfiguration.OverallSmoothing);
                MapDataGenerator.SmoothHeightMap(heightMap, BiomeConfiguration.OverallSmoothing);
            }
        }

        /* Create Terrain Data */
        var terrainData = new TerrainData
        {
            baseMapResolution = BiomeConfiguration.HeightMapResolution,
            heightmapResolution = Mathf.ClosestPowerOfTwo(BiomeConfiguration.HeightMapResolution) + 1,
            alphamapResolution = BiomeConfiguration.HeightMapResolution,
            splatPrototypes = _terrainStructure.GetSplatPrototypes()
        };
        terrainData.SetDetailResolution(BiomeConfiguration.HeightMapResolution, 32);
        terrainData.size = new Vector3(BiomeConfiguration.MapSize, BiomeConfiguration.MapHeight, BiomeConfiguration.MapSize);
        terrainData.SetAlphamaps(0, 0, alphamap);

        /* Create Terrain GameObject */
        var terrain = Terrain.CreateTerrainGameObject(terrainData);
        terrain.name = "Terrain";
        terrain.transform.parent = transform;
        terrain.transform.position = Vector3.zero;
        terrain.GetComponent<Terrain>().terrainData.SetHeights(0, 0, heightMap);
        //terrain.GetComponent<Terrain>().materialType = Terrain.MaterialType.Custom;
        //terrain.GetComponent<Terrain>().materialTemplate = BiomeConfiguration.TerrainMaterial; <-- TODO: fix to support more than 4 textures

        /* Fill terrain with scenery */
        if (FillTerrain)
        {
            var sceneryObjects = _sceneryStructure.FillAllSceneryAreas(terrain.GetComponent<Terrain>());
            var scenery = new GameObject("Scenery");
            scenery.transform.parent = terrain.transform;
            foreach (var obj in sceneryObjects)
            {
                obj.transform.parent = scenery.transform;
            }
        }

        /* Water Plane Placement */
        var water = GameObject.CreatePrimitive(PrimitiveType.Plane);
        water.GetComponent<Renderer>().material = BiomeConfiguration.WaterMaterial;
        water.transform.localScale = new Vector3(terrainData.size.x / 5f, 1, terrainData.size.z / 5f);
        water.transform.parent = terrain.transform;
        water.transform.localPosition = new Vector3(terrainData.size.x / 2f, (BiomeConfiguration.SeaHeight + 0.01f) * terrainData.size.y, terrainData.size.z / 2f);
    }

    void DrawGraph()
    {
        var newGraphInstance = _terrainStructure.DrawBiomeGraph(BiomeConfiguration.HeightMapResolution / 500f);
        newGraphInstance.name = "BiomeGraph";
        newGraphInstance.transform.parent = transform;
    }

    void DrawAreaGraph()
    {
        var newAreaGraphInstance = _worldStructure.DrawAreaGraph(BiomeConfiguration.HeightMapResolution / 500f);
        newAreaGraphInstance.name = "AreaGraph";
        newAreaGraphInstance.transform.parent = transform;
    }

    void ClearDisplay()
    {
        var toDelete = new List<GameObject>();
        foreach (var o in FindObjectsOfType(typeof(GameObject)))
        {
            var go = (GameObject)o;
            if (go.name == "BiomeGraph" || go.name == "Terrain" || go.name == "AreaGraph")
                toDelete.Add(go);
        }

        foreach (var go in toDelete)
        {
            StartCoroutine(DestroyInEditor(go));
        }
    }

    private static IEnumerator DestroyInEditor(GameObject obj)
    {
        yield return new WaitForSecondsRealtime(1);
        DestroyImmediate(obj, true);
    }
}