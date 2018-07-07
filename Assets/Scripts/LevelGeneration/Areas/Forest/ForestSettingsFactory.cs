using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "ForestSettingsFactory", menuName = "Terrain/Areas/Forest")]
public class ForestSettingsFactory : AreaSettingsFactory
{
    public AreaSegment.EAreaSegmentType SegmentType = AreaSegment.EAreaSegmentType.MainPath;
    public GameObject[] Trees;
    [Range(0.1f, 20)] public float TreeDistance = 5;
    [Range(0, 80)] public float AngleTolerance = 15;

    public override Graph<AreaSegment> GetPatternGraph()
    {
        Graph<AreaSegment> pattern = new Graph<AreaSegment>();
        pattern.AddNode(new AreaSegment(SegmentType));

        return pattern;
    }

    public override AreaSettings[] ProduceAreaSettings(Graph<AreaData> areaDataGraph, IEnumerable<Vector2[]> clearPolygons, Vector2[] borderPolygon)
    {
        AreaSettings forest = new ForestSettings(areaDataGraph, clearPolygons, borderPolygon,
            Trees, TreeDistance, AngleTolerance, SegmentType.ToString())
        {
            ArenaTriggerPrefab = this.ArenaTriggerPrefab,
            FogGatePrefab = this.FogGatePrefab
        };
        return new[] {forest};
    }
}