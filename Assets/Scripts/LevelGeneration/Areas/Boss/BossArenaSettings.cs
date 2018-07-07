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
    private readonly float _gateMinimumLenght;
    private readonly GameObject _towerPrefab;
    private readonly GameObject _portalPrefab;
    private readonly GameObject _rewardPedestalPrefab;
    private readonly GameObject[] _buildingPrefabs;
    private readonly GameObject _bossPrefab;

    //TODO boss and reward prefabs

    private Vector2[] _gateLine;

    public BossArenaSettings(Graph<AreaData> areaDataGraph, IEnumerable<Vector2[]> clearPolygons, Vector2[] borderPolygon,
        GameObject wallPrefab, float wallLength, float wallAngleLimit, Vector3 wallPositionNoise, Vector3 wallScaleNoise,
        GameObject gatePrefab, float gateMinimumLength, GameObject towerPrefab, GameObject portalPrefab,
        GameObject rewardPedestalPrefab, GameObject[] buildingsPrefabs, GameObject bossPrefab)
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
        _portalPrefab = portalPrefab;
        _bossPrefab = bossPrefab;
        _gateMinimumLenght = gateMinimumLength;
    }

    public override GameObject GenerateAreaScenery(Terrain terrain)
    {
        var arena = new GameObject(Name);

        // Find boss area center
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
        var line = _gateLine[0] - _gateLine[1];
        var gatePosition2D = (_gateLine[0] + _gateLine[1]) / 2;
        var gatePosition = new Vector3(gatePosition2D.x, 0, gatePosition2D.y);

        // Set arena center on gate script
        var arenaCenter2D = center;

        // Place portal
        var portalPosition = new Vector3(arenaCenter2D.x, 0, arenaCenter2D.y);
        portalPosition += new Vector3(0, terrain.SampleHeight(portalPosition), 0);
        var portal = Object.Instantiate(_portalPrefab, portalPosition, terrain.GetNormalRotation(portalPosition) * Quaternion.LookRotation(gatePosition - new Vector3(portalPosition.x, 0, portalPosition.z) , Vector3.up));
        portal.transform.parent = arena.transform;

        // Generate Walls
        var lines = BorderPolygon.PolygonToLines(skip);
        var walls = LevelDataGenerator.GenerateBlockerLine(terrain, lines, _wallLenght, _wallPositionNoise,
            _wallScaleNoise, _wallPrefab, false, _towerPrefab, _wallAngleLimit);
        walls.transform.parent = arena.transform;

        // Generate last tower next to the gate
        var gateTower = Object.Instantiate(_towerPrefab);
        gateTower.transform.position = gatePosition + Quaternion.LookRotation(new Vector3(line.x, 0, line.y), Vector3.up) * new Vector3(0, 0, line.magnitude / 2);
        gateTower.transform.position += new Vector3(0, terrain.SampleHeight(gateTower.transform.position), 0);
        gateTower.transform.rotation = terrain.GetNormalRotation(gateTower.transform.position);
        gateTower.CorrectAngleTolerance(_wallAngleLimit);
        gateTower.transform.parent = arena.transform;
        var navMeshModifier = gateTower.AddComponent<NavMeshModifier>();
        navMeshModifier.overrideArea = true;
        navMeshModifier.area = NavMesh.GetAreaFromName("Not Walkable");

        // Spawn boss at the arena center
        var bossPosition = new Vector3(portalPosition.x, 0, portalPosition.z) + 5 * new Vector3(gatePosition.x - portalPosition.x, 0 , gatePosition.z - portalPosition.z).normalized;
        bossPosition += new Vector3(0, terrain.SampleHeight(bossPosition), 0);
        var boss = Object.Instantiate(_bossPrefab, bossPosition, Quaternion.identity);
        boss.transform.parent = arena.transform;
        var bossTrigger = boss.AddComponent<PortalActivateTrigger>();
        bossTrigger.Portal = portal;
        bossTrigger.Initialize();

        return arena;
    }

    private List<int> SplitEntranceLine(List<Vector2> points)
    {
        for (int i = 0; i < points.Count; i++)
        {
            var p0 = points[i];
            var p1 = points[(i + 1) % points.Count];
            var center = (p1 + p0) / 2;

            foreach (var clearPolygon in ClearPolygons)
            {
                if (center.IsInsidePolygon(clearPolygon))
                {
                    var newP0 = p0 + (p1 - p0) * .3f;
                    var newP1 = p1 + (p0 - p1) * .3f;

                    if ((newP0 - newP1).magnitude < _gateMinimumLenght)
                    {
                        _gateLine = new[] { p0, p1 };
                        return new List<int> { i, (i + 1) % points.Count }; // RETURN
                    }
                    else
                    {
                        points.Insert(i + 1, newP0);
                        points.Insert(i + 2, newP1);
                        _gateLine = new[] { newP0, newP1 };
                        return new List<int> { (i + 1) % points.Count, (i + 2) % points.Count }; // RETURN
                    }
                    
                }
            }
        }
        return null;
    }
}