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
        BiomeGraph,
        GameMap,
        AreaGraph
    }

    public DrawModeEnum DrawMode = DrawModeEnum.BiomeGraph;
    public BiomeGlobalConfiguration BiomeGlobalConfiguration;
    public List<BiomeSettings> AvailableBiomes;

    public AreaBase[] NormalAreas;
    public AreaBase BossArea;
    
    public GameObject SpawnerPrefab;

    public int ExtraEdges = 20;
    public bool FillTerrain = true;
    public float RoadHalfWidth = 10;
    public bool GenerateOnPlay = false;
    public int Seed = 0;

    public TerrainStructure MyTerrainStructure { get; private set; }
    public StoryStructure MyStoryStructure { get; private set; }
    public SceneryStructure MySceneryStructure { get; private set; }
    public Terrain Terrain { get; private set; }

   public void CreateMap()
   {
        DrawMode = DrawModeEnum.GameMap;
        Seed = GameController.Instance.Seed;
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

        MyStoryStructure = new StoryStructure(AvailableBiomes, 0, 1, 20, BossArea, new CharacterEnemy[4]);
        if (DrawMode == DrawModeEnum.AreaGraph || DrawMode == DrawModeEnum.GameMap)
        {
            MyTerrainStructure = new TerrainStructure(MyStoryStructure, BiomeGlobalConfiguration);
            if (DrawMode == DrawModeEnum.GameMap)
                MySceneryStructure = new SceneryStructure(MyStoryStructure, MyTerrainStructure, NormalAreas, BossArea, RoadHalfWidth);
        }

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
        var heightMap = LevelDataGenerator.GenerateHeightMap(MyTerrainStructure);

        /* Create splat textures alphamap */
        var alphamap = LevelDataGenerator.GenerateAlphaMap(MyTerrainStructure);

        /* Draw roads onto alphamap */
        LevelDataGenerator.DrawLineRoads(MyTerrainStructure, heightMap, alphamap, 1);

        /* Smoothing passes */
        alphamap = LevelDataGenerator.SmoothAlphaMap(alphamap, 1);
        if (BiomeGlobalConfiguration.SmoothEdges)
        {
            //Smooth only navigable biome borders
            LevelDataGenerator.SmoothHeightMapWithLines(heightMap, BiomeGlobalConfiguration.MapSize / BiomeGlobalConfiguration.HeightMapResolution, MyTerrainStructure.GetNonBlendingBiomeBorders(), BiomeGlobalConfiguration.EdgeWidth, BiomeGlobalConfiguration.SquareSize);
            
            //Overall smoothing
            if (BiomeGlobalConfiguration.OverallSmoothing > 0)
            {
                LevelDataGenerator.SmoothHeightMap(heightMap, BiomeGlobalConfiguration.OverallSmoothing, 2);
            }
        }

        /* Create Terrain Data */
        var terrainData = new TerrainData
        {
            baseMapResolution = BiomeGlobalConfiguration.HeightMapResolution,
            heightmapResolution = Mathf.ClosestPowerOfTwo(BiomeGlobalConfiguration.HeightMapResolution) + 1,
            alphamapResolution = BiomeGlobalConfiguration.HeightMapResolution,
            splatPrototypes = MyTerrainStructure.GetSplatPrototypes()
        };
        terrainData.SetDetailResolution(BiomeGlobalConfiguration.HeightMapResolution, 32);
        terrainData.size = new Vector3(BiomeGlobalConfiguration.MapSize, BiomeGlobalConfiguration.MapHeight, BiomeGlobalConfiguration.MapSize);
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
        var fences = LevelDataGenerator.GenerateOuterFences(Terrain, MyTerrainStructure, BiomeGlobalConfiguration.CoastBlocker, BiomeGlobalConfiguration.CoastBlockerPole, BiomeGlobalConfiguration.CoastBlockerLength);
        fences.transform.parent = Terrain.transform;

        var walls = LevelDataGenerator.GenerateAreaWalls(Terrain, MyTerrainStructure, BiomeGlobalConfiguration.AreaBlocker, BiomeGlobalConfiguration.AreaBlockerLength);
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
        water.GetComponent<Renderer>().material = BiomeGlobalConfiguration.WaterMaterial;
        water.GetComponent<Renderer>().shadowCastingMode = ShadowCastingMode.Off;
        water.transform.localScale = new Vector3(terrainData.size.x / 5f, 1, terrainData.size.z / 5f);
        water.transform.parent = Terrain.transform;
        water.transform.localPosition = new Vector3(terrainData.size.x / 2f, (BiomeGlobalConfiguration.SeaHeight + 0.01f) * terrainData.size.y, terrainData.size.z / 2f);
    }

    void DrawGraph()
    {
        //TODO: debug class
    }

    void DrawAreaGraph()
    {
        //TODO: debug class
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