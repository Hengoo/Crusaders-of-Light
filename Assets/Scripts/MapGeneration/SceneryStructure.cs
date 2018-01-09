﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SceneryStructure
{
    public List<SceneryAreaFill> SceneryAreas { get; private set; }
    public TerrainStructure TerrainStructure { get; private set; }
    public WorldStructure WorldStructure { get; private set; }
    public List<Vector2[]> RoadPolygons { get; private set; }
    public List<Vector2[]> RoadLines { get; private set; }

    public AreaBase[] NormalAreas { get; private set; }
    public AreaBase BossArea { get; private set; }

    private List<GameObject> _sceneryQuestObjects = new List<GameObject>();

    public SceneryStructure(TerrainStructure terrainStructure, WorldStructure worldStructure, AreaBase[] normalAreas, AreaBase bossArea, float roadWidth)
    {
        SceneryAreas = new List<SceneryAreaFill>();
        RoadPolygons = new List<Vector2[]>();
        RoadLines = new List<Vector2[]>();

        TerrainStructure = terrainStructure;
        WorldStructure = worldStructure;

        NormalAreas = normalAreas;
        BossArea = bossArea;

        CreateFill(roadWidth);
    }

    private void CreateFill(float roadWidth)
    {
        //Get the biome edges from the terrain structure and create areas to fill with prefabs and quests
        List<GameObject[]> prefabs;
        List<float> minDistances;
        var polygons = TerrainStructure.GetBiomePolygons(out prefabs, out minDistances);
        for (var i = 0; i < polygons.Count; i++)
        {
            SceneryAreas.Add(new SceneryAreaFill(prefabs[i], polygons[i], minDistances[i]));
        }

        //Fill areas 
        QuestBase[] quests = new QuestBase[0];
        for(int i = 0; i < NormalAreas.Length; i++)
            quests = quests.Union(NormalAreas[i].GenerateQuests(this, i)).ToArray();
        
        var levelController = LevelController.Instance;
        if (!levelController)
            levelController = GameObject.Find("LevelController").GetComponent<LevelController>();

        //Clear previously generated quests in editor when not playing
        if (!Application.isPlaying)
            levelController.QuestController.ClearQuests(); 

        //Add all quests
        foreach (var quest in quests)
            levelController.QuestController.AddQuest(quest);

        //Create road polygons and road lines
        foreach (var edge in WorldStructure.NavigationGraph.GetAllEdges().Union(WorldStructure.AreaCrossingNavigationEdges))
        {
            var start = TerrainStructure.BiomeGraph.GetNodeData(edge.x).Center;
            var end = TerrainStructure.BiomeGraph.GetNodeData(edge.y).Center;

            RoadLines.Add(new []{start, end});

            var line = (end - start).normalized;
            var normal = (Vector2)Vector3.Cross(line, Vector3.forward).normalized;

            var p0 = start - line * roadWidth + normal * roadWidth;
            var p1 = start - line * roadWidth - normal * roadWidth;
            var p2 = end + line * roadWidth + normal * roadWidth;
            var p3 = end + line * roadWidth - normal * roadWidth;
            var origin = (p0 + p1 + p2 + p3) / 4;

            var poly = new List<Vector2>{p0, p1, p2, p3};
            poly.SortVertices(origin);
            RoadPolygons.Add(poly.ToArray());
        }

        //Add removal polygon to affected area fill
        foreach (var polygon in RoadPolygons)
        {
            foreach (var vertex in polygon)
            {
                foreach (var sceneryArea in SceneryAreas)
                {
                    if(vertex.IsInsidePolygon(sceneryArea.Polygon))
                        sceneryArea.AddClearPolygon(polygon);
                }
            }
        }
    }

    /* Fill all areas with prefabs */
    public IEnumerable<GameObject> FillAllSceneryAreas(Terrain terrain)
    {
        var result = new List<GameObject>();

        var questObjects = new GameObject("Quest Objects");
        result.Add(questObjects);
        foreach (var obj in _sceneryQuestObjects)
        {
            obj.transform.position += new Vector3(0, terrain.SampleHeight(obj.transform.position), 0);
            obj.transform.parent = questObjects.transform;
        }

        foreach (var sceneryArea in SceneryAreas)
        {
            var fill = FillSceneryArea(sceneryArea, terrain);
            result.Add(fill);
        }
        return result;
    }

    /* Fill an area with prefabs */
    private GameObject FillSceneryArea(SceneryAreaFill sceneryAreaFill, Terrain terrain)
    {
        var result = new GameObject("SceneryAreaFill");
        result.transform.position = Vector3.zero;
        result.transform.rotation = Quaternion.identity;

        if (sceneryAreaFill.Prefabs == null || sceneryAreaFill.Prefabs.Length <= 0)
            return result;

        var size = sceneryAreaFill.Size;
        PoissonDiskGenerator.minDist = sceneryAreaFill.MinDist;
        PoissonDiskGenerator.sampleRange = (size.x > size.y ? size.x : size.y);
        PoissonDiskGenerator.Generate();
        foreach (var sample in PoissonDiskGenerator.ResultSet)
        {
            var point = sample + sceneryAreaFill.BoundMin;
            var height = terrain.SampleHeight(new Vector3(point.x, 0, point.y) - terrain.transform.position);
            if (height <= (TerrainStructure.BiomeGlobalConfiguration.SeaHeight + 0.01f) * terrain.terrainData.size.y || // not underwater
                !point.IsInsidePolygon(sceneryAreaFill.Polygon) || //not outside of the area
                !sceneryAreaFill.ClearPolygons.TrueForAll(a => !point.IsInsidePolygon(a))) //not inside of any clear polygon
                continue;

            var go = Object.Instantiate(sceneryAreaFill.Prefabs[Random.Range(0, sceneryAreaFill.Prefabs.Length)]);
            go.transform.position = new Vector3(point.x, height, point.y) + terrain.transform.position;
            go.transform.parent = result.transform;
        }

        return result;
    }

    // Add a scenery object that needs height adjustment when the terrain is generated
    public void AddSceneryQuestObject(GameObject questObject)
    {
        _sceneryQuestObjects.Add(questObject);
    }
}

public class SceneryAreaFill
{
    public readonly Vector2[] Polygon;
    public readonly List<Vector2[]> ClearPolygons;
    public readonly GameObject[] Prefabs;
    public readonly Vector2 Size;
    public readonly Vector2 BoundMin;
    public readonly Vector2 BoundMax;
    public readonly float MinDist;

    public SceneryAreaFill(GameObject[] prefabs, Vector2[] polygon, float minDist)
    {
        ClearPolygons = new List<Vector2[]>();

        Prefabs = prefabs;
        Polygon = polygon;
        MinDist = minDist;

        var minX = float.MaxValue;
        var minY = float.MaxValue;
        var maxX = float.MinValue;
        var maxY = float.MinValue;

        foreach (var point in polygon)
        {
            minX = Mathf.Min(point.x, minX);
            minY = Mathf.Min(point.y, minY);
            maxX = Mathf.Max(point.x, maxX);
            maxY = Mathf.Max(point.y, maxY);
        }

        BoundMin = new Vector2(minX, minY);
        BoundMax = new Vector2(maxX, maxY);
        Size = new Vector2(maxX - minX, maxY - minY);
    }

    public void AddClearPolygon(Vector2[] polygon)
    {
        ClearPolygons.Add(polygon);
    }
}
