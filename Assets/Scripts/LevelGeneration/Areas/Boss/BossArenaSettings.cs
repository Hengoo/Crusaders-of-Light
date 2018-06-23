using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

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
        ClearPolygons = clearPolygons != null ? clearPolygons.ToList() : new List<Vector2[]>();
        BorderPolygon = borderPolygon.ToList();

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

        // Split gate line
        var skip = SplitEntranceLine(BorderPolygon);
        BorderPolygon = BorderPolygon.OffsetToCenter(center, 8, skip).ToList();

        // Generate gate
        var arenaCenter2D = BorderPolygon.GetPolygonCenter();
        var line = _gateLine[0] - _gateLine[1];
        var gatePosition2D = (_gateLine[0] + _gateLine[1]) / 2;
        var gatePosition = new Vector3(gatePosition2D.x, 0, gatePosition2D.y);
        var gate = Object.Instantiate(_gatePrefab);
        var shape = gate.GetComponent<ParticleSystem>().shape;
        shape.scale += new Vector3(0, 0, line.magnitude - 1);
        gate.GetComponent<BoxCollider>().size += new Vector3(0, 0, line.magnitude - 1);
        gate.GetComponent<NavMeshObstacle>().size += new Vector3(0, 0, line.magnitude - 1);
        gate.transform.position = new Vector3(gatePosition.x, terrain.SampleHeight(gatePosition), gatePosition.z);
        gate.transform.rotation = Quaternion.LookRotation(new Vector3(line.x, 0, line.y), Vector3.up);
        gate.transform.parent = arena.transform;
        gate.GetComponent<ArenaGateTrigger>().ArenaCenter = new Vector3(arenaCenter2D.x, 0, arenaCenter2D.y);
        gate.GetComponent<ArenaGateTrigger>().ArenaCenter += new Vector3(0, terrain.SampleHeight(gate.GetComponent<ArenaGateTrigger>().ArenaCenter), 0);

        // Generate Walls
        var lines = BorderPolygon.PolygonToLines(skip);
        var walls = LevelDataGenerator.GenerateBlockerLine(terrain, lines, _wallLenght, _wallPositionNoise,
            _wallScaleNoise, _wallPrefab, false, _towerPrefab, _wallAngleLimit);
        walls.transform.parent = arena.transform;

        // Generate last tower next to the gate
        var gateTower = Object.Instantiate(_towerPrefab);
        gateTower.transform.position = gatePosition + gate.transform.rotation * new Vector3(0, 0, gate.GetComponent<NavMeshObstacle>().size.z / 2);
        gateTower.transform.position += new Vector3(0, terrain.SampleHeight(gateTower.transform.position), 0);
        gateTower.transform.rotation = terrain.GetNormalRotation(gateTower.transform.position);
        gateTower.CorrectAngleTolerance(_wallAngleLimit);
        gateTower.transform.parent = arena.transform;
        var navMeshModifier = gateTower.AddComponent<NavMeshModifier>();
        navMeshModifier.overrideArea = true;
        navMeshModifier.area = NavMesh.GetAreaFromName("Not Walkable");

        return arena;
    }

    private List<int> SplitEntranceLine(List<Vector2> points)
    {

        for (int i = 0; i < points.Count; i++)
        {
            var p0 = points[i];
            var p1 = points[i != points.Count - 1 ? i + 1 : 0];
            var center = (p1 + p0) / 2;

            foreach (var clearPolygon in ClearPolygons)
            {
                if (center.IsInsidePolygon(clearPolygon))
                {
                    var p00 = clearPolygon.ClosestPoint(p0);
                    p00 += (p0 - p00) * .2f;
                    var p10 = clearPolygon.ClosestPoint(p1);
                    p10 += (p1 - p10) * .2f;
                    points.Insert(i + 1, p00);
                    points.Insert(i + 2, p10);

                    _gateLine = new[] { p00, p10 };
                    return new List<int> { i + 1, i + 2 }; // RETURN
                }
            }
        }
        return null;
    }
}