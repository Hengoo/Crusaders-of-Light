using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ChurchSettingsFactory", menuName = "Terrain/Areas/Church")]
public class ChurchSettingsFactory : AreaSettingsFactory
{
    public GameObject ChestPrefab;
    public GameObject[] MiniBosses;
    public GameObject ChurchPrefab;
    [Range(0, 80)] public float AngleTolerance;
    public GameObject[] GravePrefabs;
    [Range(0, 80)] public float GraveAngleTolerance;
    [Range(0, 20)] public float PathOffset;
    public GameObject[] Trees;
    [Range(0, 80)] public float TreeAngleTolerance;
    [Range(1, 50)] public float TreeDistance;

    public override Graph<AreaSegment> GetPatternGraph()
    {
        var result = new Graph<AreaSegment>();
        result.AddNode(new AreaSegment(AreaSegment.EAreaSegmentType.Special));

        return result;
    }

    public override AreaSettings[] ProduceAreaSettings(Graph<AreaData> areaDataGraph, IEnumerable<Vector2[]> clearPolygons, Vector2[] borderPolygon)
    {
        return new[]
        {
            new ChurchSettings(areaDataGraph, clearPolygons, borderPolygon, ChurchPrefab, AngleTolerance, PathOffset, GravePrefabs,
                GraveAngleTolerance, Trees, TreeAngleTolerance, TreeDistance, MiniBosses, ChestPrefab, "Church")
        };
    }
}
