﻿using System.Collections.Generic;
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
    public GameObject StreetLamp;
    public readonly float LampAngleTolerance;
    public readonly float LampPathOffset;

    public VillageSettings(Graph<AreaData> areaDataGraph, IEnumerable<Vector2[]> clearPolygons, Vector2[] borderPolygon,
        GameObject[] genericBuildings, GameObject[] uniqueBuildings, float uniqueBuildingChance, float buildingAngleTolerance, float pathOffset,
        GameObject[] trees, float treeAngleTolerance, float treeDistance, GameObject streetLamp, float lampAngleTolerance, float lampPathOffset, string type = "")
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

        StreetLamp = streetLamp;
        LampAngleTolerance = lampAngleTolerance;
        LampPathOffset = lampPathOffset;
    }

    public override GameObject GenerateAreaScenery(Terrain terrain)
    {
        var result = new GameObject(Name);

        // Place lamps
        var lamps = PlaceStreetLamps(terrain);
        lamps.transform.parent = result.transform;

        // Place buildings
        var buildings = PlaceBuildings(terrain);
        buildings.transform.parent = result.transform;

        // Generate trees taking buildings into consideration
        foreach (var areaData in AreaDataGraph.GetAllNodeData())
        {
            // Poisson filling
            PoissonDiskFillData poissonData = new PoissonDiskFillData(Trees, areaData.Polygon, TreeDistance, TreeAngleTolerance, true);
            poissonData.AddClearPolygons(ClearPolygons);
            PoissonDataList.Add(poissonData);

            // Place arenas
            var arena = PlaceArena(areaData, terrain);
            arena.transform.parent = result.transform;
        }

        return result;
    }

    private GameObject PlaceStreetLamps(Terrain terrain)
    {
        var result = new GameObject("Lamps");

        List<Vector2[]> pathLines = new List<Vector2[]>();

        // Collect all roads
        foreach (var areaData in AreaDataGraph.GetAllNodeData())
        {
            var data = areaData;
            var validPaths = areaData.Paths.Where(path => path.All(vtx => vtx.IsInsidePolygon(data.Polygon))).ToList();
            pathLines = pathLines.Union(validPaths).ToList();
        }

        // Place lamps along the paths, alternating side and skipping one
        bool spawnRight = true;
        bool skip = true;
        foreach (var line in pathLines)
        {
            skip = !skip;
            if (skip)
                continue;

            var pathCenter = (line[0] + line[1]) / 2f;
            var normal = spawnRight ? (Vector2)Vector3.Cross(line[0] - line[1], Vector3.back).normalized : (Vector2)Vector3.Cross(line[0] - line[1], Vector3.forward).normalized;
            var position2D = pathCenter + normal * LampPathOffset;
            var position = new Vector3(position2D.x, 0, position2D.y);
            var rotation = Quaternion.LookRotation(new Vector3(pathCenter.x, 0, pathCenter.y) - position, Vector3.up);

            position += new Vector3(0, terrain.SampleHeight(position), 0);

            var lamp = Object.Instantiate(StreetLamp, position, rotation);
            lamp.CorrectAngleTolerance(LampAngleTolerance);
            lamp.transform.parent = result.transform;
            var lampClear = lamp.GetComponent<BoxCollider>().Get2DPolygon();
            ClearPolygons.Add(lampClear.OffsetToCenter(lampClear.GetPolygonCenter(), -1).ToArray());

            // Flip side
            spawnRight = !spawnRight;
        }

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
                var go = Object.Instantiate(buildingPrefab, position, rotation);
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
                var go = Object.Instantiate(buildingPrefab, position, rotation);
                return go;
            }
        }

        return null;
    }
}