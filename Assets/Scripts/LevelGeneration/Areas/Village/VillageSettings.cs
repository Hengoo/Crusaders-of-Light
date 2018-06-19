using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VillageSettings : AreaSettings
{
    public GameObject[] GenericBuildings;
    public List<GameObject> UniqueBuildings;
    public readonly float BuildingAngleTolerance;
    public readonly float PathOffset;
    public GameObject[] Trees;
    public readonly float TreeAngleTolerance;
    public readonly float TreeDistance;

    public VillageSettings(Graph<AreaData> areaDataGraph, IEnumerable<Vector2[]> clearPolygons, Vector2[] borderPolygon,
        GameObject[] genericBuildings, GameObject[] uniqueBuildings, float buildingAngleTolerance, float pathOffset,
        GameObject[] trees, float treeAngleTolerance, float treeDistance, string type = "")
    {
        Name = "Village " + type + " Area";
        AreaDataGraph = areaDataGraph;
        ClearPolygons = clearPolygons != null ? clearPolygons.ToList() : new List<Vector2[]>();
        BorderPolygon = borderPolygon.ToList();

        GenericBuildings = genericBuildings;
        UniqueBuildings = uniqueBuildings.ToList();
        BuildingAngleTolerance = buildingAngleTolerance;
        PathOffset = pathOffset;

        Trees = trees;
        TreeAngleTolerance = treeAngleTolerance;
        TreeDistance = treeDistance;
    }

    public override GameObject GenerateAreaScenery(Terrain terrain)
    {
        var result = new GameObject(Name);

        // Place buildings
        var buildings = PlaceBuildings(terrain);
        buildings.transform.parent = result.transform;

        // Generate trees taking buildings into consideration
        PoissonDiskFillData poissonData = new PoissonDiskFillData(Trees, BorderPolygon.ToArray(), TreeDistance, TreeAngleTolerance, true);
        poissonData.AddClearPolygons(ClearPolygons);
        PoissonDataList.Add(poissonData);

        return result;
    }

    // Place buildings throughout all area segments
    private GameObject PlaceBuildings(Terrain terrain)
    {
        GameObject result = new GameObject("Buildings");

        // Fill each node with buildings around the roads
        foreach (var nodeData in AreaDataGraph.GetAllNodeData())
        {
            var fill = FillAreaSegment(nodeData, terrain);
            fill.transform.parent = result.transform;
        }

        return result;
    }

    // Fill an area segment with buildings along the road
    private GameObject FillAreaSegment(AreaData areaData, Terrain terrain)
    {
        var result = new GameObject("AreaSegment Fill");
        var fittable = new List<GameObject>(GenericBuildings).Union(UniqueBuildings).ToList();

        // Try to place as many buildings as possible
        while (fittable.Count > 0)
        {
            var buildingPrefab = fittable[Random.Range(0, fittable.Count)];
            var instance = FitBuilding(buildingPrefab, areaData);

            // No fit was possible
            if (!instance)
            {
                fittable.Remove(buildingPrefab);
            }
            //Found a valid fit
            else
            {
                // Adjust height, orientation and reparent
                instance.transform.position += new Vector3(0, terrain.SampleHeight(instance.transform.position), 0);
                instance.transform.rotation = terrain.GetNormalRotation(instance.transform.position) * instance.transform.rotation;
                instance.CorrectAngleTolerance(BuildingAngleTolerance);
                instance.transform.parent = result.transform;

                // Remove from the lists if this is a unique building
                if (UniqueBuildings.Contains(buildingPrefab))
                {
                    fittable.Remove(buildingPrefab);
                    UniqueBuildings.Remove(buildingPrefab);
                }
            }

        }

        return result;
    }

    // Loosely based on the "Road Conquest" in 
    // "Arnaud Emilien, Adrien Bernhardt, Adrien Peytavie, Marie-Paule Cani, Eric Galin. Procedural Generation
    // of Villages on Arbitrary Terrains.Visual Computer, Springer Verlag, 2012, 28 (6-8), pp.809-818.
    // <10.1007/s00371-012-0699-7>. <hal-00694525>"
    private GameObject FitBuilding(GameObject buildingPrefab, AreaData areaData)
    {
        var building2DPolygon = buildingPrefab.GetComponent<BoxCollider>().Get2DPolygon();

        // Try to place the building on the side of each path
        foreach (var path in areaData.Paths)
        {
            var pathCenter = (path[0] + path[1]) / 2f;
            var leftNormal = (Vector2)Vector3.Cross(path[0] - path[1], Vector3.forward).normalized;
            var rightNormal = -leftNormal;


            float angle = Vector2.SignedAngle(Vector2.up, leftNormal);
            var oriented2DPolygon = building2DPolygon.Select(vtx => (Vector2)(Quaternion.AngleAxis(angle, Vector3.forward) * vtx)).ToList();
            oriented2DPolygon.SortVertices(Vector2.zero);
            var leftPoly = oriented2DPolygon.Select(vtx => vtx + pathCenter + leftNormal * PathOffset).ToList();
            var rightPoly = oriented2DPolygon.Select(vtx => vtx + pathCenter + rightNormal * PathOffset).ToList();

            Vector2 leftCenter = Vector2.zero;
            foreach (var p in leftPoly)
            {
                leftCenter += p;
            }
            leftCenter /= 4;

            Vector2 rightCenter = Vector2.zero;
            foreach (var p in rightPoly)
            {
                rightCenter += p;
            }
            rightCenter /= 4;

            // Try right side placement
            bool rightInvalid = false;
            foreach (Vector2[] clearPoly in ClearPolygons)
            {
                // Right side has no collisions
                if (rightPoly.Any(vtx => vtx.IsInsidePolygon(clearPoly)) || rightCenter.IsInsidePolygon(clearPoly))
                {
                    rightInvalid = true;
                    break;
                }
            }
            if (!rightInvalid)
            {
                ClearPolygons.Add(rightPoly.ToArray());
                var position2D = pathCenter + rightNormal * PathOffset;
                var position = new Vector3(position2D.x, 0, position2D.y);
                var rotation = Quaternion.LookRotation(new Vector3(pathCenter.x, 0, pathCenter.y) - position,
                    Vector3.up);
                var go = Object.Instantiate(buildingPrefab, position, rotation);
                StructureDrawer.DrawPolygon(rightPoly, Color.cyan).transform.parent = go.transform;

                return go;
            }

            // Try left side placement
            bool leftInvalid = false;
            foreach (Vector2[] clearPoly in ClearPolygons)
            {
                if (leftPoly.Any(vtx => vtx.IsInsidePolygon(clearPoly)) || leftCenter.IsInsidePolygon(clearPoly))
                {
                    leftInvalid = true;
                    break;
                }
            }

            if (!leftInvalid)
            {
                ClearPolygons.Add(leftPoly.ToArray());
                var position2D = pathCenter + leftNormal * PathOffset;
                var position = new Vector3(position2D.x, 0, position2D.y);
                var rotation = Quaternion.LookRotation(new Vector3(pathCenter.x, 0, pathCenter.y) - position,
                    Vector3.up);
                var go = Object.Instantiate(buildingPrefab, position, rotation);
                StructureDrawer.DrawPolygon(leftPoly, Color.cyan).transform.parent = go.transform;
                return go;
            }

            

        }

        return null;
    }
}