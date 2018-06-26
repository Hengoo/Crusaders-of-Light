using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ChurchSettings : AreaSettings
{
    public readonly GameObject[] MiniBosses;
    public readonly GameObject ChurchPrefab;
    public readonly float AngleTolerance;
    public readonly GameObject[] GravePrefabs;
    public readonly float GraveAngleTolerance;
    public readonly GameObject[] Trees;
    public readonly float TreeAngleTolerance;
    public readonly float TreeDistance;

    public ChurchSettings(Graph<AreaData> areaDataGraph, IEnumerable<Vector2[]> clearPolygons, Vector2[] borderPolygon, GameObject churchPrefab, float angleTolerance, GameObject[] gravePrefabs, float graveAngleTolerance, GameObject[] trees, float treeDistance, float treeAngleTolerance, GameObject[] miniBosses, string type = "")
    {
        Name = "Forest " + type + " Area";
        AreaDataGraph = areaDataGraph;
        ClearPolygons = clearPolygons != null ? clearPolygons.ToList() : new List<Vector2[]> { };
        BorderPolygon = borderPolygon.ToList();

        Trees = trees;
        TreeAngleTolerance = treeAngleTolerance;
        MiniBosses = miniBosses;
        GravePrefabs = gravePrefabs;
        GraveAngleTolerance = graveAngleTolerance;
        ChurchPrefab = churchPrefab;
        AngleTolerance = angleTolerance;
        TreeDistance = treeDistance;
    }

    public override GameObject GenerateAreaScenery(Terrain terrain)
    {
        var result = new GameObject(Name);

        PoissonDiskFillData poissonData = new PoissonDiskFillData(Trees, BorderPolygon.ToArray(), TreeDistance, TreeAngleTolerance, true);
        poissonData.AddClearPolygons(ClearPolygons);
        PoissonDataList.Add(poissonData);

        return result;
    }
}
