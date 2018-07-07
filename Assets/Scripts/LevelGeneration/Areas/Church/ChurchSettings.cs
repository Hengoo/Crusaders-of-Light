using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class ChurchSettings : AreaSettings
{
    public readonly GameObject ChestPrefab;
    public readonly GameObject[] MiniBosses;
    public readonly GameObject ChurchPrefab;
    public readonly float AngleTolerance;
    public readonly float PathOffset;
    public readonly GameObject[] GravePrefabs;
    public readonly float GraveAngleTolerance;
    public readonly Vector2 GraveDistance;
    public readonly Vector3 RotationNoise;
    public readonly Vector3 PositionNoise;
    public readonly GameObject[] Trees;
    public readonly float TreeAngleTolerance;
    public readonly float TreeDistance;

    public ChurchSettings(Graph<AreaData> areaDataGraph, IEnumerable<Vector2[]> clearPolygons, Vector2[] borderPolygon, GameObject churchPrefab, float angleTolerance, float pathOffset, GameObject[] gravePrefabs, float graveAngleTolerance, Vector2 graveDistance, Vector3 rotationNoise, Vector3 positionNoise, GameObject[] trees, float treeDistance, float treeAngleTolerance, GameObject[] miniBosses, GameObject chestPrefab, string type = "")
    {
        Name = "Forest " + type + " Area";
        AreaDataGraph = areaDataGraph;
        ClearPolygons = clearPolygons != null ? clearPolygons.ToList() : new List<Vector2[]> { };
        BorderPolygon = borderPolygon.ToList();

        Trees = trees;
        TreeAngleTolerance = treeAngleTolerance;
        MiniBosses = miniBosses;
        ChestPrefab = chestPrefab;
        GraveDistance = graveDistance;
        PathOffset = pathOffset;
        GravePrefabs = gravePrefabs;
        GraveAngleTolerance = graveAngleTolerance;
        ChurchPrefab = churchPrefab;
        AngleTolerance = angleTolerance;
        TreeDistance = treeDistance;
        RotationNoise = rotationNoise;
        PositionNoise = positionNoise;
    }

    public override GameObject GenerateAreaScenery(Terrain terrain)
    {
        var result = new GameObject(Name);
        var data = AreaDataGraph.GetNodeData(0);

        // Create church
        var church = PlaceChurch(data);
        if (church)
        {
            church.transform.position += new Vector3(0, terrain.SampleHeight(church.transform.position), 0);
            church.transform.rotation = terrain.GetNormalRotation(church.transform.position) * church.transform.rotation;
            church.CorrectAngleTolerance(AngleTolerance);
            church.transform.parent = result.transform;
        }

        // Place chest
        var chestPosition = new Vector3(data.Center.x, 0, data.Center.y);
        var chestPositionHeight = chestPosition + new Vector3(0, terrain.SampleHeight(chestPosition), 0);
        var pathPosition = new Vector3(data.Paths[0][0].x, 0, data.Paths[0][0].y);
        if ((pathPosition - chestPosition).sqrMagnitude < 1)
            pathPosition = new Vector3(data.Paths[0][1].x, 0, data.Paths[0][1].y);
        var chest = Object.Instantiate(ChestPrefab, chestPositionHeight, terrain.GetNormalRotation(chestPositionHeight) * Quaternion.LookRotation(pathPosition - chestPosition, Vector3.up));
        chest.transform.parent = result.transform;
        chest.GetComponent<ChestBossTrigger>().MiniBoss = MiniBosses[Random.Range(0, MiniBosses.Length)];

        // Place cemetary
        var cemetery = PlaceCemetery(data, terrain);
        cemetery.transform.parent = result.transform;

        // Fill spaces with trees
        PoissonDiskFillData poissonData = new PoissonDiskFillData(Trees, BorderPolygon.ToArray(), TreeDistance, TreeAngleTolerance, true);
        poissonData.AddClearPolygons(ClearPolygons);
        PoissonDataList.Add(poissonData);

        // Place arena
        PlaceArena(data, terrain).transform.parent = result.transform;

        return result;
    }

    private GameObject PlaceCemetery(AreaData areaData, Terrain terrain)
    {
        var result = new GameObject("Graveyard");
        var rectangles = GetValidRectangles(areaData);
        foreach (var rectangle in rectangles)
        {
            var tombs = PlaceTombstonesInGrid(rectangle, terrain);
            tombs.transform.parent = result.transform;
        }

        return result;
    }

    private List<Vector2[]> GetValidRectangles(AreaData areaData)
    {
        var result = new List<Vector2[]>();
        var count = 0;
        do
        {
            var fitPossible = false;
            Vector2 topLeft = Vector2.zero;
            Vector2 bottomRight = Vector2.zero;
            Vector2 topRight = Vector2.zero;
            Vector2 bottomLeft = Vector2.zero;
            Vector2 up = Vector2.zero;
            Vector2 left = Vector2.zero;

            // Find a suitable start point for the rectangle
            bool wasRight = false;
            foreach (var path in areaData.Paths)
            {
                if (path.Any(vtx => !vtx.IsInsidePolygon(areaData.Polygon)))
                    continue;

                Vector2 pathCenter = (path[0] + path[1]) / 2f;
                Vector2 leftNormal = Vector3.Cross(path[0] - path[1], Vector3.forward).normalized;
                Vector2 rightNormal = -leftNormal;
                float offset = (PathOffset + 3);

                // Try right side placement
                bottomRight = pathCenter + rightNormal * offset;
                bool rightInvalid = ClearPolygons.Any(poly => bottomRight.IsInsidePolygon(poly));
                if (!rightInvalid)
                {
                    up = rightNormal;
                    left = Vector3.Cross(leftNormal, Vector3.forward).normalized;
                    wasRight = true;
                    break;
                }

                // Try left side placement
                bottomRight = pathCenter + leftNormal * offset;
                bool leftInvalid = ClearPolygons.Any(poly => bottomRight.IsInsidePolygon(poly));
                if (!leftInvalid)
                {
                    up = leftNormal;
                    left = Vector3.Cross(leftNormal, Vector3.forward).normalized;
                    break;
                }

                bottomRight = Vector2.zero;
            }

            // Early break if previous search failed
            if (up == Vector2.zero || left == Vector2.zero)
                break;

            // Expand until not possible anymore in both directions
            bottomRight = bottomRight - up * 2 - left * 2;
            topLeft = bottomRight;
            topRight = bottomRight;
            bottomLeft = bottomRight;
            bool expandLeft, expandUp;
            do
            {
                expandLeft = false;
                expandUp = false;

                // Expand left and check
                topLeft += left;
                bottomLeft += left;
                if (!ClearPolygons.Any(poly => topLeft.IsInsidePolygon(poly)) &&
                    !ClearPolygons.Any(poly => bottomLeft.IsInsidePolygon(poly)) &&
                    topLeft.IsInsidePolygon(areaData.Polygon) &&
                    bottomLeft.IsInsidePolygon(areaData.Polygon))
                {
                    expandLeft = true;
                    fitPossible = true;
                }
                else
                {
                    topLeft -= left;
                    bottomLeft -= left;
                }

                // Expand right and check
                topLeft += up;
                topRight += up;
                if (!ClearPolygons.Any(poly => topLeft.IsInsidePolygon(poly)) &&
                    !ClearPolygons.Any(poly => topRight.IsInsidePolygon(poly)) &&
                    topLeft.IsInsidePolygon(areaData.Polygon) &&
                    topRight.IsInsidePolygon(areaData.Polygon))
                {
                    expandUp = true;
                    fitPossible = true;
                }
                else
                {
                    topLeft -= up;
                    topRight -= up;
                }
            } while (expandLeft || expandUp);

            // LOOP CONDITION BREAK
            if (!fitPossible)
                break;

            float upGuard = 4;
            var rectangle = new[] { bottomRight, bottomLeft, topLeft - up * upGuard, topRight - up * upGuard };
            result.Add(rectangle);
            if (wasRight)
            {
                ClearPolygons.Add(rectangle.OffsetToCenter(rectangle.GetPolygonCenter(), -10).ToArray());
            }
            else
            {
                var sortedRectangle = rectangle.OffsetToCenter(rectangle.GetPolygonCenter(), -10).ToList();
                sortedRectangle.SortVertices(sortedRectangle.GetPolygonCenter());
                ClearPolygons.Add(sortedRectangle.ToArray());
            }
            count++;
        } while (count < 5); // INFINITE LOOP SAFE GUARD

        return result;
    }

    // Assumed all polygons are rectangles and sorted
    private GameObject PlaceTombstonesInGrid(Vector2[] poly, Terrain terrain)
    {
        var result = new GameObject("SubGraveyard");
        var left = (poly[1] - poly[0]).normalized;
        var up = (poly[3] - poly[0]).normalized;

        if (Math.Abs(GraveDistance.x) < 1 || Math.Abs(GraveDistance.y) < 1)
            return null;

        for (int y = 0; y < (poly[0] - poly[3]).magnitude / GraveDistance.y; y++)
        {
            for (int x = 0; x < (poly[0] - poly[1]).magnitude / GraveDistance.x; x++)
            {
                GameObject prefab = GravePrefabs[Random.Range(0, GravePrefabs.Length)];
                if (!prefab)
                    continue;

                var position2D = poly[0] + (left * x * GraveDistance.x) / (x == 0 ? 2 : 1) + (up * y * GraveDistance.y) / (y == 0 ? 2 : 1);
                var position = new Vector3(position2D.x, 0, position2D.y);
                position += new Vector3(0, terrain.SampleHeight(position), 0);
                var tombstone = Object.Instantiate(prefab, position, terrain.GetNormalRotation(position) * Quaternion.LookRotation(-new Vector3(up.x, 0, up.y)));
                tombstone.transform.localPosition += new Vector3(Random.Range(-PositionNoise.x, PositionNoise.x), Random.Range(-PositionNoise.y, PositionNoise.y), Random.Range(-PositionNoise.z, PositionNoise.z));
                tombstone.transform.localRotation *= Quaternion.Euler(Random.Range(-RotationNoise.x, RotationNoise.x), Random.Range(-RotationNoise.y, RotationNoise.y), Random.Range(-RotationNoise.z, RotationNoise.z));
                tombstone.transform.parent = result.transform;
            }
        }

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
