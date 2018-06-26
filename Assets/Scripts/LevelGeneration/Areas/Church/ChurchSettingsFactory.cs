using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ChurchSettingsFactory", menuName = "Terrain/Areas/Church")]
public class ChurchSettingsFactory : AreaSettingsFactory {

    public GameObject[] MiniBosses;
    public GameObject ChurchPrefab;
    public float AngleTolerance;
    public GameObject[] GravePrefabs;
    public float GraveAngleTolerance;
    public GameObject[] Trees;
    public float TreeAngleTolerance;
    public float TreeDistance;

    public override Graph<AreaSegment> GetPatternGraph()
    {
        var result = new Graph<AreaSegment>();
        int main = result.AddNode(new AreaSegment(AreaSegment.EAreaSegmentType.MainPath));
        int special = result.AddNode(new AreaSegment(AreaSegment.EAreaSegmentType.Special));
        result.AddEdge(main, special, (int) AreaSegment.EAreaSegmentEdgeType.SidePath);

        return result;
    }

    public override AreaSettings[] ProduceAreaSettings(Graph<AreaData> areaDataGraph, IEnumerable<Vector2[]> clearPolygons, Vector2[] borderPolygon)
    {
        return new[]
        {
            new ChurchSettings(areaDataGraph, clearPolygons, borderPolygon, ChurchPrefab, AngleTolerance, GravePrefabs,
                GraveAngleTolerance, Trees, TreeAngleTolerance, TreeDistance, MiniBosses, "Church")
        };
    }
}
