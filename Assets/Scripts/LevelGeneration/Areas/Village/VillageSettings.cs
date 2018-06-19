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
        var forestArea = new ForestSettings(AreaDataGraph, ClearPolygons, BorderPolygon.ToArray(), Trees, TreeDistance, TreeAngleTolerance, "Village");
        var trees = forestArea.GenerateAreaScenery(terrain);
        trees.transform.parent = result.transform;

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
        var building2DPolygon = buildingPrefab.GetComponent<BoxCollider>().GetLocal2DPolygon();

        // Try to place the building on the side of each path
        foreach (var path in areaData.Paths)
        {

            var pathCenter = (path[0] + path[1]) / 2f;
            var leftNormal = (Vector2)Vector3.Cross(path[0] - path[1], Vector3.forward).normalized;
            var leftPoly = building2DPolygon.Select(vtx => vtx + pathCenter + leftNormal * PathOffset).ToArray();
            var rightNormal = -leftNormal;
            var rightPoly = building2DPolygon.Select(vtx => vtx + pathCenter + rightNormal * PathOffset).ToArray();

            // Try left side placement
            bool leftInvalid = false;
            foreach (Vector2[] clearPoly in ClearPolygons)
            {
                if (leftPoly.Any(vtx => vtx.IsInsidePolygon(clearPoly)))
                {
                    leftInvalid = true;
                    break;
                }
            }

            if (!leftInvalid)
            {
                ClearPolygons.Add(leftPoly);
                var position2D = pathCenter + leftNormal * PathOffset;
                var position = new Vector3(position2D.x, 0, position2D.y);
                var rotation = Quaternion.LookRotation(new Vector3(pathCenter.x, 0, pathCenter.y) - position,
                    Vector3.up);
                return Object.Instantiate(buildingPrefab, position, rotation);
            }

            // Try right side placement
            bool rightInvalid = false;
            foreach (Vector2[] clearPoly in ClearPolygons)
            {
                // Right side has no collisions
                if (rightPoly.All(vtx => !vtx.IsInsidePolygon(clearPoly)))
                {
                    rightInvalid = true;
                    break;
                }
            }
            if (!rightInvalid)
            {
                ClearPolygons.Add(rightPoly);
                var position2D = pathCenter + rightNormal * PathOffset;
                var position = new Vector3(position2D.x, 0, position2D.y);
                var rotation = Quaternion.LookRotation(new Vector3(pathCenter.x, 0, pathCenter.y) - position,
                    Vector3.up);
                return Object.Instantiate(buildingPrefab, position, rotation);
            }

        }

        return null;
    }
}