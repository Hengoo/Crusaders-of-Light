using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class VillageSettings : AreaSettings
{
    public GameObject[] GenericBuildings;
    public List<GameObject> UniqueBuildings;
    public float UniqueBuildingChance;
    public readonly float BuildingAngleTolerance;
    public readonly float PathOffset;
    public GameObject[] Trees;
    public readonly float TreeAngleTolerance;
    public readonly float TreeDistance;

    public VillageSettings(Graph<AreaData> areaDataGraph, IEnumerable<Vector2[]> clearPolygons, Vector2[] borderPolygon,
        GameObject[] genericBuildings, GameObject[] uniqueBuildings, float uniqueBuildingChance, float buildingAngleTolerance, float pathOffset,
        GameObject[] trees, float treeAngleTolerance, float treeDistance, string type = "")
    {
        Name = "Village " + type + " Area";
        AreaDataGraph = areaDataGraph;
        ClearPolygons = clearPolygons != null ? clearPolygons.ToList() : new List<Vector2[]>();
        BorderPolygon = borderPolygon.ToList();

        GenericBuildings = genericBuildings;
        UniqueBuildings = uniqueBuildings.ToList();
        UniqueBuildingChance = uniqueBuildingChance;
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
        var unique = new List<GameObject>(UniqueBuildings);

        // Fill each node with buildings around the roads
        foreach (var nodeData in AreaDataGraph.GetAllNodeData())
        {
            var fill = FillAreaSegment(nodeData, unique, terrain);
            fill.transform.parent = result.transform;
        }

        return result;
    }

    // Fill an area segment with buildings along the road
    private GameObject FillAreaSegment(AreaData areaData, List<GameObject> unique, Terrain terrain)
    {
        var result = new GameObject("AreaSegment Fill");
        var generic = new List<GameObject>(GenericBuildings);

        // Try to place as many buildings as possible
        while (generic.Count > 0)
        {
            bool tryUnique = Random.Range(0, 100) < UniqueBuildingChance;
            var buildingPrefab = tryUnique && unique.Count > 0 ? unique[Random.Range(0, unique.Count)] : generic[Random.Range(0, generic.Count)];
            var instance = FitBuilding(buildingPrefab, areaData);

            // No fit was possible
            if (!instance)
            {
                if (tryUnique)
                    unique.Remove(buildingPrefab);
                else
                    generic.Remove(buildingPrefab);
            }
            //Found a valid fit
            else
            {
                // Adjust height, orientation and reparent
                instance.GetComponent<BoxCollider>().enabled = false;
                var navMesh = instance.AddComponent<NavMeshModifier>();
                navMesh.overrideArea = true;
                navMesh.area = NavMesh.GetAreaFromName("Not Walkable");
                instance.transform.position += new Vector3(0, terrain.SampleHeight(instance.transform.position), 0);
                instance.transform.rotation = terrain.GetNormalRotation(instance.transform.position) * instance.transform.rotation;
                instance.CorrectAngleTolerance(BuildingAngleTolerance);
                instance.transform.parent = result.transform;

                // Remove from the unique list if this is a unique building
                if (tryUnique)
                    unique.Remove(buildingPrefab);
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
        var border = BorderPolygon.ToArray();

        // Try to place the building on the side of each path
        foreach (var path in areaData.Paths)
        {
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
                var go = Object.Instantiate(buildingPrefab, position, rotation);
                StructureDrawer.DrawPolygon(rightPoly, Color.red).transform.parent = go.transform;

                return go;
            }

            // Try left side placement
            bool leftInvalid = leftPoly.Any(vtx => !vtx.IsInsidePolygon(border)) || 
                ClearPolygons.Any(clearPoly => leftPoly.Any(vtx => vtx.IsInsidePolygon(clearPoly)) ||
                                    clearPoly.Any(vtx=> vtx.IsInsidePolygon(leftPoly) ||
                                    leftCenter.IsInsidePolygon(clearPoly)) ||
                                    clearPoly.GetPolygonCenter().IsInsidePolygon(leftPoly));

            if (!leftInvalid)
            {
                ClearPolygons.Add(leftPoly.ToArray());
                var position2D = pathCenter + leftNormal * PathOffset;
                var position = new Vector3(position2D.x, 0, position2D.y);
                var rotation = Quaternion.LookRotation(new Vector3(pathCenter.x, 0, pathCenter.y) - position,
                    Vector3.up);
                var go = Object.Instantiate(buildingPrefab, position, rotation);
                StructureDrawer.DrawPolygon(leftPoly, Color.green).transform.parent = go.transform;

                return go;
            }
        }

        return null;
    }
}