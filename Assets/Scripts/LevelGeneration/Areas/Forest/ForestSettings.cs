using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ForestSettings : AreaSettings
{
    public GameObject[] Trees;
    public readonly float AngleTolerance;
    public readonly float TreeDistance;

    public ForestSettings(Graph<AreaData> areaDataGraph, IEnumerable<Vector2[]> clearPolygons, Vector2[] borderPolygon, GameObject[] trees, float treeDistance, float angleTolerance, string type = "")
    {
        Name = "Forest " + type + " Area";
        AreaDataGraph = areaDataGraph;
        ClearPolygons = clearPolygons != null ? clearPolygons.ToList() : new List<Vector2[]> { };
        BorderPolygon = borderPolygon.ToList();

        Trees = trees;
        AngleTolerance = angleTolerance;
        TreeDistance = treeDistance;
    }

    public override GameObject GenerateAreaScenery(Terrain terrain)
    {
        var result = new GameObject(Name);

        // Generate trees taking buildings into consideration
        foreach (var areaData in AreaDataGraph.GetAllNodeData())
        {
            // Poisson filling
            PoissonDiskFillData poissonData =
                new PoissonDiskFillData(Trees, areaData.Polygon, TreeDistance, AngleTolerance, true);
            poissonData.AddClearPolygons(ClearPolygons);
            PoissonDataList.Add(poissonData);

            // Place arenas - skip start position
            if(areaData.Segment.Type == AreaSegment.EAreaSegmentType.Start)
                continue;
            
            var arena = PlaceArena(areaData, terrain);
            arena.transform.parent = result.transform;
        }


        return result;
    }
}