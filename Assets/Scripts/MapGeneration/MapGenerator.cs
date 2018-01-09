﻿using System;
using System.Collections;
using System.Collections.Generic;
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
    public BiomeGlobalConfiguration BiomeGlobalConfiguration;
    public List<BiomeSettings> AvailableBiomes;
    
    public AreaBase[] NormalAreas;
    public AreaBase BossArea;


    public int ExtraEdges = 20;
    public bool FillTerrain = true;
    public float RoadHalfWidth = 10;
    public bool GenerateOnPlay = false;
    public int Seed = 0;

    private TerrainStructure _terrainStructure;
    private WorldStructure _worldStructure;
    private SceneryStructure _sceneryStructure;
    private Terrain _terrain;

    void Start()
    {
        DrawMode = DrawModeEnum.GameMap;
        Seed = GameController.Instance.Seed;
        LevelController.Instance.InitializeLevel();
        GeneratePreview();
    }

    /* Redraws preview in the scene editor */
    public void GeneratePreview()
    {
#if UNITY_EDITOR
        if (!GenerateOnPlay && Application.isPlaying)
            return;
#endif

        ClearDisplay();
        Random.InitState(Seed);

        _terrainStructure = new TerrainStructure(AvailableBiomes, BiomeGlobalConfiguration);
        _worldStructure = new WorldStructure(_terrainStructure, NormalAreas.Length + 1, ExtraEdges);
        if(DrawMode == DrawModeEnum.GameMap)
            _sceneryStructure = new SceneryStructure(_terrainStructure, _worldStructure, NormalAreas, BossArea, RoadHalfWidth);

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

        if(Application.isPlaying)
            LevelController.Instance.StartGame(_sceneryStructure, _terrain);
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
        MapDataGenerator.DrawLineRoads(_terrainStructure, heightMap, alphamap, _sceneryStructure.RoadLines, 1);

        /* Smoothing passes */
        alphamap = MapDataGenerator.SmoothAlphaMap(alphamap, 1);
        if (BiomeGlobalConfiguration.SmoothEdges)
        {
            //Smooth only navigable biome borders
            MapDataGenerator.SmoothHeightMapWithLines(heightMap, BiomeGlobalConfiguration.MapSize / BiomeGlobalConfiguration.HeightMapResolution, _terrainStructure.GetBiomeSmoothBorders(), BiomeGlobalConfiguration.EdgeWidth, BiomeGlobalConfiguration.SquareSize);

            //Smooth all biome borders
            //heightMap = MapDataGenerator.SmoothHeightMapWithLines(heightMap, BiomeGlobalConfiguration.MapSize / BiomeGlobalConfiguration.HeightMapResolution, _terrainStructure.GetBiomeBorders(), 3, 2);

            //Overall smoothing
            if (BiomeGlobalConfiguration.OverallSmoothing > 0)
            {
                MapDataGenerator.SmoothHeightMap(heightMap, BiomeGlobalConfiguration.OverallSmoothing);
                MapDataGenerator.SmoothHeightMap(heightMap, BiomeGlobalConfiguration.OverallSmoothing);
            }
        }

        /* Create Terrain Data */
        var terrainData = new TerrainData
        {
            baseMapResolution = BiomeGlobalConfiguration.HeightMapResolution,
            heightmapResolution = Mathf.ClosestPowerOfTwo(BiomeGlobalConfiguration.HeightMapResolution) + 1,
            alphamapResolution = BiomeGlobalConfiguration.HeightMapResolution,
            splatPrototypes = _terrainStructure.GetSplatPrototypes()
        };
        terrainData.SetDetailResolution(BiomeGlobalConfiguration.HeightMapResolution, 32);
        terrainData.size = new Vector3(BiomeGlobalConfiguration.MapSize, BiomeGlobalConfiguration.MapHeight, BiomeGlobalConfiguration.MapSize);
        terrainData.SetAlphamaps(0, 0, alphamap);

        /* Create Terrain GameObject */
        _terrain = Terrain.CreateTerrainGameObject(terrainData).GetComponent<Terrain>();
        _terrain.name = "Terrain";
        _terrain.transform.parent = transform;
        _terrain.transform.position = Vector3.zero;
        _terrain.GetComponent<Terrain>().terrainData.SetHeights(0, 0, heightMap);
        //terrain.GetComponent<Terrain>().materialType = Terrain.MaterialType.Custom;
        //terrain.GetComponent<Terrain>().materialTemplate = BiomeGlobalConfiguration.TerrainMaterial; <-- TODO: fix to support more than 4 textures

        /* Add fences to coast */
        var fences = MapDataGenerator.GenerateCoastBlockers(_terrain, _worldStructure,
            BiomeGlobalConfiguration.CoastBlocker, BiomeGlobalConfiguration.CoastBlockerPole, BiomeGlobalConfiguration.CoastBlockerLength);
        fences.transform.parent = _terrain.transform;

        /* Fill terrain with scenery */
        if (FillTerrain)
        {
            var sceneryObjects = _sceneryStructure.FillAllSceneryAreas(_terrain.GetComponent<Terrain>());
            var scenery = new GameObject("Scenery");
            scenery.transform.parent = _terrain.transform;
            foreach (var obj in sceneryObjects)
            {
                obj.transform.parent = scenery.transform;
            }
        }

        /* Water Plane Placement */
        var water = GameObject.CreatePrimitive(PrimitiveType.Plane);
        water.GetComponent<Renderer>().material = BiomeGlobalConfiguration.WaterMaterial;
        water.transform.localScale = new Vector3(terrainData.size.x / 5f, 1, terrainData.size.z / 5f);
        water.transform.parent = _terrain.transform;
        water.transform.localPosition = new Vector3(terrainData.size.x / 2f, (BiomeGlobalConfiguration.SeaHeight + 0.01f) * terrainData.size.y, terrainData.size.z / 2f);
    }

    void DrawGraph()
    {
        var newGraphInstance = _terrainStructure.DrawBiomeGraph(BiomeGlobalConfiguration.HeightMapResolution / 500f);
        newGraphInstance.name = "BiomeGraph";
        newGraphInstance.transform.parent = transform;
    }

    void DrawAreaGraph()
    {
        var newAreaGraphInstance = _worldStructure.DrawAreaGraph(BiomeGlobalConfiguration.HeightMapResolution / 500f);
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