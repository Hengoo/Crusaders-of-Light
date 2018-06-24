using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "BossArenaSettingsFactory", menuName = "Terrain/Areas/Boss Arena")]
public class BossArenaSettingsFactory : AreaSettingsFactory {

    public GameObject WallPrefab;
    [Range(0.1f, 20f)] public float WallLenght = 2;
    [Range(0, 80)] public float WallAngleLimit = 15;
    public Vector3 WallPositionNoise;
    public Vector3 WallScaleNoise;
    public GameObject GatePrefab;
    public GameObject TowerPrefab;
    public GameObject PortalPrefab;
    public GameObject RewardPedestalPrefab;
    public GameObject[] BuildingPrefabs;
    public GameObject[] BossPrefabs;

    public override Graph<AreaSegment> GetPatternGraph()
    {
        Graph<AreaSegment> pattern = new Graph<AreaSegment>();

        pattern.AddNode(new AreaSegment(AreaSegment.EAreaSegmentType.Boss));
        pattern.AddNode(new AreaSegment(AreaSegment.EAreaSegmentType.Boss));
        pattern.AddNode(new AreaSegment(AreaSegment.EAreaSegmentType.Boss));
        pattern.AddEdge(0, 1, (int)AreaSegment.EAreaSegmentEdgeType.BossInnerPath);
        pattern.AddEdge(0, 2, (int)AreaSegment.EAreaSegmentEdgeType.BossInnerPath);
        pattern.AddEdge(1, 2, (int)AreaSegment.EAreaSegmentEdgeType.BossInnerPath);

        return pattern;
    }

    public override AreaSettings[] ProduceAreaSettings(Graph<AreaData> areaDataGraph, IEnumerable<Vector2[]> clearPolygons, Vector2[] borderPolygon)
    {
        BossPrefabs.Shuffle();
        var arenaWalls = new BossArenaSettings(areaDataGraph, clearPolygons, borderPolygon, WallPrefab, WallLenght, WallAngleLimit,
            WallPositionNoise, WallScaleNoise, GatePrefab, TowerPrefab, PortalPrefab, RewardPedestalPrefab, BuildingPrefabs, BossPrefabs[0]);

        return new AreaSettings[]{arenaWalls};
    }
}
