using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class AreaSettings
{
    public string Name = "Area";
    public List<PoissonDiskFillData> PoissonDataList = new List<PoissonDiskFillData>();

    public Graph<AreaData> AreaDataGraph { get; protected set; }
    public Vector2[][] ClearPolygons { get; protected set; }
    public Vector2[] BorderPolygon { get; protected set; }

    public abstract GameObject GenerateAreaScenery(Terrain terrain);
    }

public class AreaData
{
    public Vector2 Center;
    public AreaSegment Segment;
    public Vector2[] Polygon;
    public List<Vector2[]> BlockerLines;
}

//-------------------------------------------------------------------------------------
//
//                                AREA SETTINGS
//
//-------------------------------------------------------------------------------------


public class ForestSettings : AreaSettings
{
    public GameObject[] Trees;
    public readonly float AngleTolerance;
    public readonly float TreeDistance;

    public ForestSettings(Graph<AreaData> areaDataGraph, IEnumerable<Vector2[]> clearPolygons, Vector2[] borderPolygon, float treeDistance, float angleTolerance, string type)
    {
        Name = "Forest " + type + " Area";
        AngleTolerance = angleTolerance;
        TreeDistance = treeDistance;
        AreaDataGraph = areaDataGraph;
        ClearPolygons = clearPolygons != null ? clearPolygons.ToArray() : new Vector2[][] { };
        BorderPolygon = borderPolygon;
    }

    public override GameObject GenerateAreaScenery(Terrain terrain)
    {
        var areaData = AreaDataGraph.GetAllNodeData()[0];
        PoissonDiskFillData poissonData = new PoissonDiskFillData(Trees, areaData.Polygon, TreeDistance, AngleTolerance, true);
        poissonData.AddClearPolygons(ClearPolygons);
        PoissonDataList.Add(poissonData);
        return new GameObject(Name);
    }
}

public class BossArenaSettings : AreaSettings
{
    private readonly GameObject _wallPrefab;
    private readonly float _wallLenght;
    private readonly float _wallAngleLimit;
    private readonly Vector3 _wallPositionNoise;
    private readonly Vector3 _wallScaleNoise;
    private readonly GameObject _towerPrefab;
    private readonly GameObject _rewardPedestalPrefab;
    private readonly GameObject[] _buildingPrefabs;
    //TODO boss and reward prefabs

    public BossArenaSettings(Graph<AreaData> areaDataGraph, IEnumerable<Vector2[]> clearPolygons, Vector2[] borderPolygon, GameObject wallPrefab, float wallLength, float wallAngleLimit, Vector3 wallPositionNoise, Vector3 wallScaleNoise, GameObject towerPrefab, GameObject rewardPedestalPrefab, GameObject[] buildingsPrefabs)
    {

        Name = "Boss Arena";
        AreaDataGraph = areaDataGraph;
        ClearPolygons = clearPolygons != null ? clearPolygons.ToArray() : new Vector2[][] { };
        BorderPolygon = borderPolygon;

        _wallPrefab = wallPrefab;
        _wallPositionNoise = wallPositionNoise;
        _wallScaleNoise = wallScaleNoise;
        _wallLenght = wallLength;
        _rewardPedestalPrefab = rewardPedestalPrefab;
        _buildingPrefabs = buildingsPrefabs;
        _wallAngleLimit = wallAngleLimit;
        _towerPrefab = towerPrefab;
    }

    public override GameObject GenerateAreaScenery(Terrain terrain)
    {
        var arena = new GameObject(Name);

        Vector2 center = Vector2.zero;
        var allData = AreaDataGraph.GetAllNodeData();
        foreach (var areaData in allData)
        {
            center += areaData.Center;
        }
        center /= allData.Length;

        BorderPolygon.OffsetToCenter(center, 15);
        var lines = BorderPolygon.PolygonToLines();
        var walls = LevelDataGenerator.GenerateBlockerLine(terrain, lines, _wallLenght, _wallPositionNoise,
            _wallScaleNoise, _wallPrefab, false, _towerPrefab, _wallAngleLimit);
        walls.transform.parent = arena.transform;
        
        return arena;
    }
}