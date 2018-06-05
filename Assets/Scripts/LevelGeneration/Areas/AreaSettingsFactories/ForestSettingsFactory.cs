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

    public override AreaSettings ProduceAreaSettings(Graph<AreaData> areaDataGraph, IEnumerable<Vector2[]> clearPolygons, Vector2[] borderPolygon)
    {
        return new ForestSettings(areaDataGraph, clearPolygons, borderPolygon, TreeDistance, AngleTolerance, SegmentType.ToString())
        {
            Trees = Trees
        };
    }
}

public class ForestSettings : AreaSettings
{
    public GameObject[] Trees;
    public readonly float AngleTolerance;
    public readonly float TreeDistance;

    public ForestSettings(Graph<AreaData> areaDataGraph, IEnumerable<Vector2[]> clearPolygons, Vector2[] borderPolygon, float treeDistance, float angleTolerance, string type)
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