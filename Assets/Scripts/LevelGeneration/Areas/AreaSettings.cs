using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

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

    public ForestSettings(Graph<AreaData> areaDataGraph, IEnumerable<Vector2[]> clearPolygons, Vector2[] borderPolygon, float treeDistance, float angleTolerance, string type = "")
    {
        Name = "Forest " + type + " Area";
        AreaDataGraph = areaDataGraph;
        ClearPolygons = clearPolygons != null ? clearPolygons.ToArray() : new Vector2[][] { };
        BorderPolygon = borderPolygon;

        AngleTolerance = angleTolerance;
        TreeDistance = treeDistance;
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
    private readonly GameObject _gatePrefab;
    private readonly GameObject _towerPrefab;
    private readonly GameObject _rewardPedestalPrefab;
    private readonly GameObject[] _buildingPrefabs;
    //TODO boss and reward prefabs

    private Vector2[] _gateLine;

    public BossArenaSettings(Graph<AreaData> areaDataGraph, IEnumerable<Vector2[]> clearPolygons, Vector2[] borderPolygon, GameObject wallPrefab, float wallLength, float wallAngleLimit, Vector3 wallPositionNoise, Vector3 wallScaleNoise, GameObject gatePrefab, GameObject towerPrefab, GameObject rewardPedestalPrefab, GameObject[] buildingsPrefabs)
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
        _gatePrefab = gatePrefab;
    }

    public override GameObject GenerateAreaScenery(Terrain terrain)
    {
        var arena = new GameObject(Name);

        // Find boos area center
        Vector2 center = Vector2.zero;
        var allData = AreaDataGraph.GetAllNodeData();
        foreach (var areaData in allData)
        {
            center += areaData.Center;
        }
        center /= allData.Length;

        // Generate Walls
        BorderPolygon.OffsetToCenter(center, 8);
        var lines = BorderPolygon.PolygonToLines();
        SplitEntranceLine(lines);
        var walls = LevelDataGenerator.GenerateBlockerLine(terrain, lines, _wallLenght, _wallPositionNoise,
            _wallScaleNoise, _wallPrefab, false, _towerPrefab, _wallAngleLimit);
        walls.transform.parent = arena.transform;

        // Generate gate
        var line = _gateLine[0] - _gateLine[1];
        var position2D = (_gateLine[0] + _gateLine[1]) / 2;
        var position = new Vector3(position2D.x, 0, position2D.y);
        var gate = Object.Instantiate(_gatePrefab);
        var shape = gate.GetComponent<ParticleSystem>().shape;
        shape.scale += new Vector3(0, 0, line.magnitude - 1);
        gate.GetComponent<NavMeshObstacle>().size += new Vector3(0, 0, line.magnitude - 1);
        gate.transform.position = new Vector3(position.x, terrain.SampleHeight(position), position.z);
        gate.transform.rotation = Quaternion.LookRotation(new Vector3(line.x, 0, line.y), Vector3.up);
        gate.transform.parent = arena.transform;

        return arena;
    }

    private void SplitEntranceLine(List<Vector2[]> lines)
    {
        for (int i = 0; i < lines.Count; i++)
        {
            var p0 = lines[i][0];
            var p1 = lines[i][1];
            var center = (p1 + p0) / 2;

            foreach (var clearPolygon in ClearPolygons)
            {
                if (center.IsInsidePolygon(clearPolygon))
                {
                    var p00 = clearPolygon.ClosestPoint(p0);
                    p00 += (p0 - p00) * .2f;
                    lines.Add(new[] { p00, p0 });
                    lines.Add(new[] { p0, p00 });

                    var p10 = clearPolygon.ClosestPoint(p1);
                    p10 += (p1 - p10) * .2f;
                    lines.Add(new[] { p10, p1 });

                    _gateLine = new[] { p00, p10 };

                    lines.Remove(lines[i]);
                    return; // RETURN
                }
            }
        }
    }
}

public class VillageSettings : AreaSettings
{
    public GameObject[] Buildings;
    public readonly float BuildingAngleTolerance;
    public GameObject[] Trees;
    public readonly float TreeAngleTolerance;
    public readonly float TreeDistance;

    public VillageSettings(Graph<AreaData> areaDataGraph, IEnumerable<Vector2[]> clearPolygons, Vector2[] borderPolygon,
        GameObject[] buildings, float buildingAngleTolerance,
        GameObject[] trees, float treeAngleTolerance, float treeDistance, string type = "")
    {
        Name = "Village " + type + " Area";
        AreaDataGraph = areaDataGraph;
        ClearPolygons = clearPolygons != null ? clearPolygons.ToArray() : new Vector2[][] { };
        BorderPolygon = borderPolygon;

        Buildings = buildings;
        BuildingAngleTolerance = buildingAngleTolerance;

        Trees = trees;
        TreeAngleTolerance = treeAngleTolerance;
        TreeDistance = treeDistance;
    }


    public override GameObject GenerateAreaScenery(Terrain terrain)
    {
        throw new System.NotImplementedException();
    }
}