using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "ForestSettingsFactory", menuName = "Terrain/Areas/Forest")]
public class ForestSettingsFactory : AreaSettingsFactory
{
    public AreaSegment.EAreaSegmentType SegmentType;
    public GameObject[] Trees;

    public override Graph<AreaSegment> GetPatternGraph()
    {
        Graph<AreaSegment> pattern = new Graph<AreaSegment>();
        pattern.AddNode(new AreaSegment(SegmentType));

        return pattern;
    }

    public override AreaSettings ProduceAreaSettings(IEnumerable<Vector2> centers, Graph<Vector2[]> polygonGraph, IEnumerable<Vector2[]> clearPolygons)
    {
        return new ForestSettings(centers, polygonGraph, clearPolygons)
        {
            Trees = Trees
        };
    }
}

public class ForestSettings : AreaSettings
{
    public GameObject[] Trees;

    public ForestSettings(IEnumerable<Vector2> centers, Graph<Vector2[]> polygonGraph, IEnumerable<Vector2[]> clearPolygons)
    {
        Name = "Forest Area";
        Centers = centers.ToArray();
        PolygonGraph = polygonGraph;
        ClearPolygons = clearPolygons.ToArray();
    }

    public override GameObject GenerateAreaScenery(Terrain terrain)
    {
        var poly = PolygonGraph.GetAllNodeData()[0];
        PoissonDiskFillData data = new PoissonDiskFillData(Trees, poly, 5);
        data.AddClearPolygons(ClearPolygons);
        PoissonData.Add(data);
        return new GameObject(Name);
    }
}