using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MapPreview : MonoBehaviour
{

    public enum DrawModeEnum
    {
        BiomeGraph,
        Terrain,
        AreaGraph
    }

    public DrawModeEnum DrawMode = DrawModeEnum.BiomeGraph;
    public BiomeConfiguration BiomeConfiguration;
    public List<BiomeSettings> AvailableBiomes;
    public int Seed = 0;
    public int NumberOfAreas = 3;
    public bool FillTerrain = true;


    /* Debug variables */
    private TerrainStructure _terrainStructure;
    private WorldStructureGraph _areaStructure;
    private SceneryStructure _sceneryStructure;

    void Start()
    {
        gameObject.SetActive(false);
    }

    /* Redraws preview in the scene editor */
    public void GeneratePreview()
    {
        if (Application.isPlaying)
            return;

        ClearDisplay();
        Random.InitState(Seed);
        _terrainStructure = new TerrainStructure(AvailableBiomes, BiomeConfiguration);
        _areaStructure = new WorldStructureGraph();
        switch (DrawMode)
        {
            case DrawModeEnum.BiomeGraph:
                DrawGraph();
                break;
            case DrawModeEnum.Terrain:
                DrawTerrain();
                break;
            case DrawModeEnum.AreaGraph:
                _areaStructure.GenerateAreaGraph(_terrainStructure.BiomeGraph, _terrainStructure.VoronoiDiagram, NumberOfAreas);
                DrawAreaGraph();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    void DrawTerrain()
    {
        var heightMap = TerrainDataGenerator.GenerateHeightMap(_terrainStructure, BiomeConfiguration);
        if (BiomeConfiguration.SmoothEdges)
        {
            //Smooth only navigable biome borders
            heightMap = TerrainDataGenerator.SmoothHeightMapWithLines(heightMap, BiomeConfiguration.MapSize / BiomeConfiguration.HeightMapResolution, _terrainStructure.GetBiomeSmoothBorders(), BiomeConfiguration.EdgeWidth, BiomeConfiguration.SquareSize);

            //Smooth all biome borders
            //heightMap = TerrainDataGenerator.SmoothHeightMapWithLines(heightMap, BiomeConfiguration.MapSize / BiomeConfiguration.HeightMapResolution, _terrainStructure.GetBiomeBorders(), 3, 2);

            //Overall smoothing
            if (BiomeConfiguration.OverallSmoothing > 0)
            {
                heightMap = TerrainDataGenerator.SmoothHeightMap(heightMap, BiomeConfiguration.OverallSmoothing);
                heightMap = TerrainDataGenerator.SmoothHeightMap(heightMap, BiomeConfiguration.OverallSmoothing);
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

        /* Create splat textures alphamap */
        var alphamap = TerrainDataGenerator.GenerateAlphaMap(_terrainStructure, BiomeConfiguration);
        alphamap = TerrainDataGenerator.SmoothAlphaMap(alphamap, 1);
        terrainData.SetAlphamaps(0, 0, alphamap);

        /* Create Terrain GameObject */
        var terrain = Terrain.CreateTerrainGameObject(terrainData);
        terrain.name = "Terrain";
        terrain.transform.parent = transform;
        terrain.transform.position = Vector3.zero;
        terrain.GetComponent<Terrain>().terrainData.SetHeights(0, 0, heightMap);
        terrain.GetComponent<Terrain>().materialType = Terrain.MaterialType.Custom;
        terrain.GetComponent<Terrain>().materialTemplate = BiomeConfiguration.TerrainMaterial;

        /* Fill terrain with scenery */
        if (FillTerrain)
        {
            var sceneryStructure = new SceneryStructure(_terrainStructure);
            var sceneryObjects = sceneryStructure.FillAllSceneryAreas(terrain.GetComponent<Terrain>());
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
        water.transform.localScale = new Vector3(terrainData.size.x / 10f, 1, terrainData.size.z / 10f);
        water.transform.parent = terrain.transform;
        water.transform.localPosition = new Vector3(terrainData.size.x / 2, (BiomeConfiguration.SeaHeight + 0.01f) * terrainData.size.y, terrainData.size.z / 2);
    }

    void DrawGraph()
    {
        var newGraphInstance = _terrainStructure.DrawBiomeGraph(BiomeConfiguration.HeightMapResolution / 500f);
        newGraphInstance.name = "Graph";
        newGraphInstance.transform.parent = transform;
    }

    void DrawAreaGraph()
    {
        var newAreaGraphInstance = _areaStructure.DrawAreaGraph(BiomeConfiguration.HeightMapResolution / 500f);
        newAreaGraphInstance.name = "Polygon Graph";
        newAreaGraphInstance.transform.parent = transform;
    }

    void ClearDisplay()
    {
        var toDelete = new List<GameObject>();
        foreach (var o in FindObjectsOfType(typeof(GameObject)))
        {
            var go = (GameObject)o;
            if (go.name == "Graph" || go.name == "Terrain" || go.name == "Polygon Graph")
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