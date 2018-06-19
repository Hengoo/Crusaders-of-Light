using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "VillageSettingsFactory", menuName = "Terrain/Areas/Village")]
public class VillageSettingsFactory : AreaSettingsFactory
{
    [Range(1, 50)] public int PathLength;
    public AreaSegment.EAreaSegmentEdgeType PathType;

    public GameObject[] GenericBuildings;
    public GameObject[] UniqueBuildings;
    [Range(0, 80)] public float BuildingAngleTolerance = 10;
    [Range(0, 20)] public float PathOffset = 5;
    public GameObject[] Trees;
    [Range(0, 80)] public float TreeAngleTolerance = 20;
    [Range(0.1f, 20)] public float TreeDistance = 6;

    public override Graph<AreaSegment> GetPatternGraph()
    {
        AreaSegment.EAreaSegmentType nodeType;
        switch (PathType)
        {
            case AreaSegment.EAreaSegmentEdgeType.NonNavigable:
                nodeType = AreaSegment.EAreaSegmentType.Empty;
                break;
            case AreaSegment.EAreaSegmentEdgeType.MainPath:
                nodeType = AreaSegment.EAreaSegmentType.MainPath;
                break;
            case AreaSegment.EAreaSegmentEdgeType.SidePath:
                nodeType = AreaSegment.EAreaSegmentType.SidePath;
                break;
            case AreaSegment.EAreaSegmentEdgeType.BossInnerPath:
                nodeType = AreaSegment.EAreaSegmentType.Boss;
                break;
            case AreaSegment.EAreaSegmentEdgeType.SpecialInnerPath:
                nodeType = AreaSegment.EAreaSegmentType.Special;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        Graph<AreaSegment> result = new Graph<AreaSegment>();
        int previous = result.AddNode(new AreaSegment(nodeType));
        for (int i = 1; i < PathLength; i++)
        {
            int current = result.AddNode(new AreaSegment(nodeType));
            result.AddEdge(previous, current, (int) PathType);
            previous = current;
        }

        return result;
    }

    public override AreaSettings[] ProduceAreaSettings(Graph<AreaData> areaDataGraph, IEnumerable<Vector2[]> clearPolygons, Vector2[] borderPolygon)
    {
        return new AreaSettings[] { new VillageSettings(areaDataGraph, clearPolygons, borderPolygon, GenericBuildings, UniqueBuildings, BuildingAngleTolerance, PathOffset, Trees, TreeAngleTolerance, TreeDistance) };
    }
}