using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "ForestSettingsFactory", menuName = "Terrain/Areas/Forest")]
public class ForestSettingsFactory : AreaSettingsFactory
{
    public AreaSegment.EAreaSegmentType SegmentType = AreaSegment.EAreaSegmentType.MainPath;
    public GameObject[] Trees;

    public override Graph<AreaSegment> GetPatternGraph()
    {
        Graph<AreaSegment> pattern = new Graph<AreaSegment>();
        pattern.AddNode(new AreaSegment(SegmentType));

        return pattern;
    }

    public override AreaSettings ProduceAreaSettings(Graph<AreaData> areaDataGraph, IEnumerable<Vector2[]> clearPolygons, Vector2[] borderPolygon)
    {
        return new ForestSettings(areaDataGraph, clearPolygons, borderPolygon, SegmentType.ToString())
        {
            Trees = Trees
        };
    }
}

public class ForestSettings : AreaSettings
{
    public GameObject[] Trees;

    public ForestSettings(Graph<AreaData> areaDataGraph, IEnumerable<Vector2[]> clearPolygons, Vector2[] borderPolygon, string type)
    {
        Name = "Forest " + type + " Area";
        AreaDataGraph = areaDataGraph;
        ClearPolygons = clearPolygons != null ? clearPolygons.ToArray() : new Vector2[][] { };
        BorderPolygon = borderPolygon;
    }

    public override GameObject GenerateAreaScenery(Terrain terrain)
    {
        var areaData = AreaDataGraph.GetAllNodeData()[0];
        PoissonDiskFillData poissonData = new PoissonDiskFillData(Trees, areaData.Polygon, 5);
        poissonData.AddClearPolygons(ClearPolygons);
        PoissonDataList.Add(poissonData);
        return new GameObject(Name);
    }
}