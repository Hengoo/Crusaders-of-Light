using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ChurchSettings : AreaSettings
{
    public readonly GameObject[] MiniBosses;
    public readonly GameObject ChurchPrefab;
    public readonly float AngleTolerance;
    public readonly float PathOffset;
    public readonly GameObject[] GravePrefabs;
    public readonly float GraveAngleTolerance;
    public readonly GameObject[] Trees;
    public readonly float TreeAngleTolerance;
    public readonly float TreeDistance;

    public ChurchSettings(Graph<AreaData> areaDataGraph, IEnumerable<Vector2[]> clearPolygons, Vector2[] borderPolygon, GameObject churchPrefab, float angleTolerance, float pathOffset, GameObject[] gravePrefabs, float graveAngleTolerance, GameObject[] trees, float treeDistance, float treeAngleTolerance, GameObject[] miniBosses, string type = "")
    {
        Name = "Forest " + type + " Area";
        AreaDataGraph = areaDataGraph;
        ClearPolygons = clearPolygons != null ? clearPolygons.ToList() : new List<Vector2[]> { };
        BorderPolygon = borderPolygon.ToList();

        Trees = trees;
        TreeAngleTolerance = treeAngleTolerance;
        MiniBosses = miniBosses;
        PathOffset = pathOffset;
        GravePrefabs = gravePrefabs;
        GraveAngleTolerance = graveAngleTolerance;
        ChurchPrefab = churchPrefab;
        AngleTolerance = angleTolerance;
        TreeDistance = treeDistance;
    }

    public override GameObject GenerateAreaScenery(Terrain terrain)
    {
        var result = new GameObject(Name);

        // Get cemetary direction from main path
        var church = PlaceChurch(AreaDataGraph.GetNodeData(0));
        church.transform.position += new Vector3(0, terrain.SampleHeight(church.transform.position), 0);
        church.transform.parent = result.transform;

        // Fill spaces with trees
        PoissonDiskFillData poissonData = new PoissonDiskFillData(Trees, BorderPolygon.ToArray(), TreeDistance, TreeAngleTolerance, true);
        poissonData.AddClearPolygons(ClearPolygons);
        PoissonDataList.Add(poissonData);

        return result;
    }

    private GameObject PlaceChurch(AreaData areaData)
    {
        var building2DPolygon = ChurchPrefab.GetComponent<BoxCollider>().Get2DPolygon();
        var border = areaData.Polygon;

        // Try to place the building on the side of each path
        foreach (var path in areaData.Paths)
        {
            if (path.Any(vtx => !vtx.IsInsidePolygon(areaData.Polygon)))
                continue;

            var pathCenter = (path[0] + path[1]) / 2f;
            var leftNormal = (Vector2)Vector3.Cross(path[0] - path[1], Vector3.forward).normalized;
            var rightNormal = -leftNormal;


            float rightAngle = Vector2.SignedAngle(Vector2.up, leftNormal);
            var rightPoly = building2DPolygon.Select(vtx => (Vector2)(Quaternion.AngleAxis(rightAngle, Vector3.forward) * vtx) + pathCenter + rightNormal * PathOffset).ToList();

            float leftAngle = Vector2.SignedAngle(Vector2.up, rightNormal);
            var leftPoly = building2DPolygon.Select(vtx => (Vector2)(Quaternion.AngleAxis(leftAngle, Vector3.forward) * vtx) + pathCenter + leftNormal * PathOffset).ToList();

            Vector2 leftCenter = leftPoly.GetPolygonCenter();
            Vector2 rightCenter = rightPoly.GetPolygonCenter();

            // Try right side placement
            bool rightInvalid = rightPoly.Any(vtx => !vtx.IsInsidePolygon(border)) ||
                ClearPolygons.Any(clearPoly => rightPoly.Any(vtx => vtx.IsInsidePolygon(clearPoly)) ||
                                    clearPoly.Any(vtx => vtx.IsInsidePolygon(rightPoly)) ||
                                    rightCenter.IsInsidePolygon(clearPoly) ||
                                    clearPoly.GetPolygonCenter().IsInsidePolygon(rightPoly));

            if (!rightInvalid)
            {
                ClearPolygons.Add(rightPoly.ToArray());
                var position2D = pathCenter + rightNormal * PathOffset;
                var position = new Vector3(position2D.x, 0, position2D.y);
                var rotation = Quaternion.LookRotation(new Vector3(pathCenter.x, 0, pathCenter.y) - position,
                    Vector3.up);
                var go = Object.Instantiate(ChurchPrefab, position, rotation);
                return go;
            }

            // Try left side placement
            bool leftInvalid = leftPoly.Any(vtx => !vtx.IsInsidePolygon(border)) ||
                ClearPolygons.Any(clearPoly => leftPoly.Any(vtx => vtx.IsInsidePolygon(clearPoly)) ||
                                    clearPoly.Any(vtx => vtx.IsInsidePolygon(leftPoly) ||
                                    leftCenter.IsInsidePolygon(clearPoly)) ||
                                    clearPoly.GetPolygonCenter().IsInsidePolygon(leftPoly));

            if (!leftInvalid)
            {
                ClearPolygons.Add(leftPoly.ToArray());
                var position2D = pathCenter + leftNormal * PathOffset;
                var position = new Vector3(position2D.x, 0, position2D.y);
                var rotation = Quaternion.LookRotation(new Vector3(pathCenter.x, 0, pathCenter.y) - position,
                    Vector3.up);
                var go = Object.Instantiate(ChurchPrefab, position, rotation);
                return go;
            }
        }

        return null;
    }
}
