using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

public class LevelCreator : Singleton<LevelCreator>
{
    public enum DrawModeEnum
    {
        TerrainGraph,
        GameLevel,
        AreaGraph
    }

    public DrawModeEnum DrawMode = DrawModeEnum.TerrainGraph;
    public GlobalSettings GlobalSettings;
    public List<BiomeSettings> AvailableBiomes;

    public AreaBase[] SpecialAreas;
    public AreaBase BossArea;

    public GameObject SpawnerPrefab;

    public bool FillTerrain = true;
    public float RoadHalfWidth = 10;
    public bool GenerateOnPlay = false;
    public int MainPathLength = 6;
    public int SidePathCount = 2;
    public int SidePathLength = 2;
    public int Seed = 0;

    public TerrainStructure MyTerrainStructure { get; private set; }
    public StoryStructure MyStoryStructure { get; private set; }
    public SceneryStructure MySceneryStructure { get; private set; }
    public Terrain Terrain { get; private set; }

    public void CreateMap()
    {
        DrawMode = DrawModeEnum.GameLevel;
        Seed = GameController.Instance.Seed;
        GenerateLevel();
    }

    /* Redraws preview in the scene editor */
    public void GenerateLevel()
    {
#if UNITY_EDITOR
        if (!GenerateOnPlay && Application.isPlaying)
            return;
#endif

        ClearDisplay();
        Random.InitState(Seed);

        MyStoryStructure = new StoryStructure(0, 1, MainPathLength, SidePathCount, SidePathLength, BossArea, new CharacterEnemy[4]);
        MyTerrainStructure = new TerrainStructure(MyStoryStructure, GlobalSettings, AvailableBiomes);

        if (DrawMode == DrawModeEnum.GameLevel)
            MySceneryStructure = new SceneryStructure(MyStoryStructure, MyTerrainStructure, SpecialAreas, BossArea, RoadHalfWidth);


        switch (DrawMode)
        {
            case DrawModeEnum.TerrainGraph:
                DrawTerrainSkeleton();
                break;
            case DrawModeEnum.GameLevel:
                DrawGameMap();
                break;
            case DrawModeEnum.AreaGraph:
                DrawScenerySkeleton();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    void DrawGameMap()
    {
        /* Create heightmap */
        var heightMap = LevelDataGenerator.GenerateHeightMap(MyTerrainStructure);

        /* Create splat textures alphamap */
        var alphamap = LevelDataGenerator.GenerateAlphaMap(MyTerrainStructure);

        /* Draw roads onto alphamap */
        LevelDataGenerator.DrawLineRoads(MyTerrainStructure, heightMap, alphamap, 1);

        /* Smoothing passes */
        alphamap = LevelDataGenerator.SmoothAlphaMap(alphamap, 1);
        if (GlobalSettings.SmoothEdges)
        {
            //Smooth only navigable biome borders
            LevelDataGenerator.SmoothHeightMapWithLines(heightMap, GlobalSettings.MapSize / GlobalSettings.HeightMapResolution, MyTerrainStructure.GetPathLines(), GlobalSettings.EdgeWidth, GlobalSettings.SquareSize);

            //Overall smoothing
            if (GlobalSettings.OverallSmoothing > 0)
            {
                LevelDataGenerator.SmoothHeightMap(heightMap, GlobalSettings.OverallSmoothing, 2);
            }
        }

        /* Create Terrain Data */
        var terrainData = new TerrainData
        {
            baseMapResolution = GlobalSettings.HeightMapResolution,
            heightmapResolution = Mathf.ClosestPowerOfTwo(GlobalSettings.HeightMapResolution) + 1,
            alphamapResolution = GlobalSettings.HeightMapResolution,
            splatPrototypes = MyTerrainStructure.GetSplatPrototypes()
        };
        terrainData.SetDetailResolution(GlobalSettings.HeightMapResolution, 32);
        terrainData.size = new Vector3(GlobalSettings.MapSize, GlobalSettings.MapHeight, GlobalSettings.MapSize);
        terrainData.SetAlphamaps(0, 0, alphamap);

        /* Create Terrain GameObject */
        Terrain = Terrain.CreateTerrainGameObject(terrainData).GetComponent<Terrain>();
        Terrain.name = "Terrain";
        Terrain.transform.parent = transform;
        Terrain.transform.position = Vector3.zero;
        Terrain.GetComponent<Terrain>().terrainData.SetHeights(0, 0, heightMap);
        //terrain.GetComponent<Terrain>().materialType = Terrain.MaterialType.Custom;
        //terrain.GetComponent<Terrain>().materialTemplate = BiomeGlobalConfiguration.TerrainMaterial; <-- TODO: fix to support more than 4 textures

        /* Add fences to coast */
        var fences = LevelDataGenerator.GenerateOuterFences(Terrain, MyTerrainStructure, GlobalSettings.CoastBlocker, GlobalSettings.CoastBlockerPole, GlobalSettings.CoastBlockerLength);
        fences.transform.parent = Terrain.transform;

        var walls = LevelDataGenerator.GenerateAreaWalls(Terrain, MyTerrainStructure, GlobalSettings.AreaBlocker, GlobalSettings.AreaBlockerLength);
        walls.transform.parent = Terrain.transform;

        /* Fill terrain with scenery */
        if (FillTerrain)
        {
            var sceneryObjects = LevelDataGenerator.GenerateScenery(Terrain.GetComponent<Terrain>());
            var scenery = new GameObject("Scenery");
            scenery.transform.parent = Terrain.transform;
            foreach (var obj in sceneryObjects)
            {
                obj.transform.parent = scenery.transform;
            }
        }

        /* Water Plane Placement */
        var water = GameObject.CreatePrimitive(PrimitiveType.Plane);
        water.GetComponent<Collider>().enabled = false;
        water.GetComponent<Renderer>().material = GlobalSettings.WaterMaterial;
        water.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
        water.transform.localScale = new Vector3(terrainData.size.x / 5f, 1, terrainData.size.z / 5f);
        water.transform.parent = Terrain.transform;
        water.transform.localPosition = new Vector3(terrainData.size.x / 2f, (GlobalSettings.SeaHeight + 0.01f) * terrainData.size.y, terrainData.size.z / 2f);
    }

    void DrawTerrainSkeleton()
    {
        StructureDrawer.DrawVoronoiDiagram(MyTerrainStructure.VoronoiDiagram, "Voronoi").transform.parent = gameObject.transform;
        StructureDrawer.DrawAreaSegments(MyTerrainStructure, "Base Graph").transform.parent = gameObject.transform;
    }

    void DrawScenerySkeleton()
    {
        //TODO: debug class
    }

    void ClearDisplay()
    {
        // Start from the top, because Unity updates the children index after each destroy call
        for (int i = transform.childCount - 1; i >= 0; --i)
        {
            DestroyImmediate(transform.GetChild(i).gameObject, true);
        }
    }
}