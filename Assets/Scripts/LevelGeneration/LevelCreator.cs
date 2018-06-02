using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

public class LevelCreator : Singleton<LevelCreator>
{
    public enum DrawModeEnum
    {
        TerrainSkeleton,
        Terrain,
        ScenerySkeleton,
        Scenery,
        GameLevel
    }

    public int Seed = 0;
    public DrawModeEnum DrawMode = DrawModeEnum.TerrainSkeleton;
    public bool GenerateOnPlay = false;

    [Header("Story Settings")]
    public List<BiomeSettings> AvailableBiomes;
    public BiomeSettings BorderBiome;
    [Range(0f, 50f)] public float BorderBlockerOffset = 20f;

    [Header("Terrain Settings")]
    [Range(16, 1024)] public int HeightMapResolution = 512;
    [Range(16, 1024)] public float MapSize = 512;
    [Range(1, 1024)] public float MapHeight = 80;
    [Range(10, 1000)] public int VoronoiSamples = 80;
    [Range(0, 1f)] public float MaxHeight = 1;
    [Range(0, 50f)] public float EdgeNoise = 8f;
    [Range(0, 100)] public int LloydRelaxation = 20;
    [Range(1, 8)] public int Octaves = 3;
    [Range(0, 1f)] public float WaterHeight = 0.15f;
    public Material WaterMaterial;
    
    [Header("Smooth Settings")]
    [Range(0, 5)] public int OverallSmoothing = 2;
    public bool SmoothEdges = true;
    [Range(0, 20)] public int EdgeWidth = 3;
    [Range(0, 20)] public int SquareSize = 2;

    [Header("Path Settings")]
    public int MainPathNodeCount = 8;
    public int SidePathCount = 2;
    public int SidePathNodeCount = 1;

    public TerrainStructure MyTerrainStructure { get; private set; }
    public StoryStructure MyStoryStructure { get; private set; }
    public SceneryStructure MySceneryStructure { get; private set; }
    public Terrain Terrain { get; private set; }


    private float[,] _heightMap;
    private float[,,] _alphaMap;


    public void CreateMap()
    {
        DrawMode = DrawModeEnum.GameLevel;
        Seed = GameController.Instance.Seed;
        GenerateLevel();
    }

    // Redraws preview in the scene editor
    public void GenerateLevel()
    {
#if UNITY_EDITOR
        if (!GenerateOnPlay && Application.isPlaying)
            return;
#endif

        ClearDisplay();
        Random.InitState(Seed);

        MyStoryStructure = new StoryStructure(0, 1, MainPathNodeCount, SidePathCount, SidePathNodeCount, new CharacterEnemy[4]);
        MyTerrainStructure = new TerrainStructure(MyStoryStructure, AvailableBiomes, MapSize, HeightMapResolution, Octaves, BorderBiome, VoronoiSamples, LloydRelaxation, EdgeNoise, BorderBlockerOffset);

        if (DrawMode == DrawModeEnum.GameLevel)
            MySceneryStructure = new SceneryStructure(MyStoryStructure, MyTerrainStructure);


        switch (DrawMode)
        {
            case DrawModeEnum.TerrainSkeleton:
                DrawTerrainSkeleton();
                break;
            case DrawModeEnum.Terrain:
                DrawTerrain();
                break;
            case DrawModeEnum.ScenerySkeleton:
                DrawScenerySkeleton();
                break;
            case DrawModeEnum.Scenery:
                DrawTerrainAndScenery();
                break;
            case DrawModeEnum.GameLevel:
                DrawCompleteLevel();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    //---------------------------------------------------------------
    //                    GENERATING FUNCTIONS
    //---------------------------------------------------------------

    // Generate alpha and height maps
    private void GenerateAlphaAndHeightmaps()
    {
        // Create heightmap
        _heightMap = LevelDataGenerator.GenerateHeightMap(MyTerrainStructure);

        // Create splat textures alphamap
        _alphaMap = LevelDataGenerator.GenerateAlphaMap(MyTerrainStructure);

        // Smoothing passes
        _alphaMap = LevelDataGenerator.SmoothAlphaMap(_alphaMap, 1);
        if (SmoothEdges)
        {
            // Smooth only navigable biome borders
            LevelDataGenerator.SmoothHeightMapWithLines(_heightMap, MapSize / HeightMapResolution, MyTerrainStructure.MainPathLines, EdgeWidth, SquareSize);

            // Overall smoothing
            if (OverallSmoothing > 0)
            {
                LevelDataGenerator.SmoothHeightMap(_heightMap, OverallSmoothing, 2);
            }
        }
    }

    // Create Terrain
    private void GenerateTerrain()
    {
        // Create Terrain Data
        var terrainData = new TerrainData
        {
            baseMapResolution = HeightMapResolution,
            heightmapResolution = Mathf.ClosestPowerOfTwo(HeightMapResolution) + 1,
            alphamapResolution = HeightMapResolution,
            splatPrototypes = MyTerrainStructure.GetSplatPrototypes()
        };
        terrainData.SetDetailResolution(HeightMapResolution, 32);
        terrainData.size = new Vector3(MapSize, MapHeight, MapSize);
        terrainData.SetAlphamaps(0, 0, _alphaMap);

        // Create Terrain GameObject
        Terrain = Terrain.CreateTerrainGameObject(terrainData).GetComponent<Terrain>();
        Terrain.gameObject.layer = LayerMask.NameToLayer("Terrain");
        Terrain.name = "Terrain";
        Terrain.transform.parent = transform;
        Terrain.transform.position = Vector3.zero;
        Terrain.GetComponent<Terrain>().terrainData.SetHeights(0, 0, _heightMap);
        // terrain.GetComponent<Terrain>().materialType = Terrain.MaterialType.Custom;
        // terrain.GetComponent<Terrain>().materialTemplate = BiomeGlobalConfiguration.TerrainMaterial; <-- TODO: fix to support more than 4 textures
    }

    // Water Plane Placement
    private void GenerateWaterPlane()
    {
        var water = GameObject.CreatePrimitive(PrimitiveType.Plane);
        water.GetComponent<Collider>().enabled = false;
        water.GetComponent<Renderer>().material = WaterMaterial;
        water.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
        water.transform.localScale = new Vector3(Terrain.terrainData.size.x / 5f, 1, Terrain.terrainData.size.z / 5f);
        water.transform.parent = transform;
        water.transform.localPosition = new Vector3(Terrain.terrainData.size.x / 2f, (WaterHeight + 0.01f) * Terrain.terrainData.size.y, Terrain.terrainData.size.z / 2f);
    }

    // Add fences to coast and walls between non crossable area segment borders
    private void GenerateBlockers()
    {
        var borderSettings = MyTerrainStructure.BorderSettings;
        var fences = LevelDataGenerator.GenerateBlockerLine(Terrain, MyTerrainStructure.BorderBlockerLines,
            borderSettings.BlockerLength, borderSettings.BlockerPositionNoise, borderSettings.BlockerScaleNoise,
            borderSettings.Blocker, borderSettings.BlockerPole, borderSettings.BlockerAngleLimit);
        fences.transform.parent = transform;

        var biomeSettings = MyTerrainStructure.BiomeSettings;
        var walls = LevelDataGenerator.GenerateBlockerLine(Terrain, MyTerrainStructure.AreaBlockerLines,
            biomeSettings.BlockerLength, biomeSettings.BlockerPositionNoise, biomeSettings.BlockerScaleNoise,
            biomeSettings.Blocker, biomeSettings.BlockerPole, biomeSettings.BlockerAngleLimit);
        walls.transform.parent = transform;
    }

    // Fill terrain with scenery
    private void GenerateScenery()
    {
        var sceneryObjects = LevelDataGenerator.GenerateScenery(Terrain.GetComponent<Terrain>());
        var scenery = new GameObject("Scenery");
        scenery.transform.parent = transform;
        foreach (var obj in sceneryObjects)
        {
            obj.transform.parent = scenery.transform;
        }
    }

    // Draw roads on alpha map
    private void GeneratePaths()
    {
        LevelDataGenerator.DrawStraightPathLines(_heightMap, _alphaMap, MyTerrainStructure.BiomeSettings.SidePathSplatSize, MapSize, HeightMapResolution,
            MyTerrainStructure.SidePathLines, MyTerrainStructure.TextureCount, MyTerrainStructure.SidePathSplatIndex);
        LevelDataGenerator.DrawStraightPathLines(_heightMap, _alphaMap, MyTerrainStructure.BiomeSettings.MainPathSplatSize, MapSize, HeightMapResolution,
            MyTerrainStructure.MainPathLines, MyTerrainStructure.TextureCount, MyTerrainStructure.MainPathSplatIndex);
    }

    // Create navmesh using scenery information
    private void GenerateNavMesh()
    {
        GetComponent<NavMeshSurface>().BuildNavMesh();
    }

    //---------------------------------------------------------------
    //                     DRAWING FUNCTIONS
    //---------------------------------------------------------------
    private void DrawCompleteLevel()
    {
        DrawTerrainAndScenery();
    }

    private void DrawTerrainSkeleton()
    {
        var voronoi = StructureDrawer.DrawVoronoiDiagram(MyTerrainStructure.VoronoiDiagram, "Voronoi");
        var baseGraph = StructureDrawer.DrawAreaSegments(MyTerrainStructure, "Base Graph");
        var borderLines = StructureDrawer.DrawMultipleLines(MyTerrainStructure.BorderBlockerLines, "Border Blockers");
        var areaBlocker = StructureDrawer.DrawMultipleLines(MyTerrainStructure.AreaBlockerLines, "Area Blockers");

        voronoi.transform.parent = gameObject.transform;
        baseGraph.transform.parent = gameObject.transform;
        borderLines.transform.parent = gameObject.transform;
        areaBlocker.transform.parent = gameObject.transform;
    }

    private void DrawTerrain()
    {
        GenerateAlphaAndHeightmaps();
        GeneratePaths();
        GenerateTerrain();
        GenerateBlockers();
        GenerateWaterPlane();
    }

    private void DrawScenerySkeleton()
    {
        // TODO: create debug view
    }

    private void DrawTerrainAndScenery()
    {
        DrawTerrain();
        // TODO: populate the terrain with scenery
        GenerateNavMesh();
    }

    private void ClearDisplay()
    {
        // Start from the top, because Unity updates the children index after each destroy call
        for (int i = transform.childCount - 1; i >= 0; --i)
        {
            DestroyImmediate(transform.GetChild(i).gameObject, true);
        }
    }
}