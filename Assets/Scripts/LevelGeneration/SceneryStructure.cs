using System.Collections.Generic;
using System.Linq;
using csDelaunay;
using UnityEngine;

public class SceneryStructure
{
    public List<PoissonDiskFill> SceneryAreas { get; private set; }
    public List<Vector2[]> RoadPolygons { get; private set; }

    public AreaBase[] SpecialAreas { get; private set; }
    public AreaBase BossArea { get; private set; }

    public SceneryStructure(StoryStructure storyStructure, TerrainStructure terrainStructure, AreaBase[] specialAreas, AreaBase bossArea, float roadWidth)
    {
        SceneryAreas = new List<PoissonDiskFill>();
        RoadPolygons = new List<Vector2[]>();

        SpecialAreas = specialAreas;
        BossArea = bossArea;

        // TODO: fill roads and paths - place elements along roads and keep roads clear
        // TODO: fill special areas - build structures
        // TODO: fill boss area - place logic elements
    }

    private static List<Vector2[]> GenerateRoadPolygons(TerrainStructure terrainStructure, float roadWidth)
    {
        var result = new List<Vector2[]>();
        foreach (var line in terrainStructure.MainPathLines)
        {
            var start = line[0];
            var end = line[1];

            var direction = (end - start).normalized;
            var normal = (Vector2)Vector3.Cross(direction, Vector3.forward).normalized;

            var p0 = start - direction * roadWidth + normal * roadWidth;
            var p1 = start - direction * roadWidth - normal * roadWidth;
            var p2 = end + direction * roadWidth + normal * roadWidth;
            var p3 = end + direction * roadWidth - normal * roadWidth;
            var origin = (p0 + p1 + p2 + p3) / 4;

            var poly = new List<Vector2> { p0, p1, p2, p3 };
            poly.SortVertices(origin);
            result.Add(poly.ToArray());
        }

        return result;
    }

    private static HashSet<int> GetAllNodesInArea(int currentNode, Graph<Area> biomeGraph, HashSet<int> set)
    {
        set.Add(currentNode);
        foreach (var neighbor in biomeGraph.GetNeighbours(currentNode))
        {
            if (set.Contains(neighbor)) continue;
            set.UnionWith(GetAllNodesInArea(neighbor, biomeGraph, set));
        }

        return set;
    }

    private void GenerateOuterBorderPolygon(TerrainStructure terrainStructure, List<KeyValuePair<Edge, Vector2>> outerBorderEdges)
    {
        var levelCreator = LevelCreator.Instance;
        var coastBlockerPolygon = new List<Vector2>();
        var coastLines = outerBorderEdges.Select(pair => pair.Key).ToList().EdgesToSortedLines();
        foreach (var line in coastLines)
        {

            //Offset borders towards biome center
            var left = line[0];
            var right = line[1];
            var center = Vector2.zero;
            outerBorderEdges.ForEach(e =>
            {
                var l = e.Key.ClippedEnds[LR.LEFT].ToUnityVector2();
                var r = e.Key.ClippedEnds[LR.RIGHT].ToUnityVector2();
                if ((l == left || l == right) && (r == left || r == right))
                    center = e.Value;
            });

            left += (center - left).normalized * levelCreator.BorderInlandOffset;
            right += (center - right).normalized * levelCreator.BorderInlandOffset;

            //Offsetting can give duplicated points
            if (!coastBlockerPolygon.Contains(left))
                coastBlockerPolygon.Add(left);
            if (!coastBlockerPolygon.Contains(right))
                coastBlockerPolygon.Add(right);
        }
    }


    private static void CreateAreaPolygon(TerrainStructure terrainStructure, int numberOfAreas, HashSet<int>[] AreaBiomes, Vector2[][] AreaPolygons)
    {
        //Create area polygon
        for (var i = 0; i < numberOfAreas; i++)
        {
            var areaEdges = new List<Edge>();

            //Get the edges of this area
            foreach (var edge in terrainStructure.VoronoiDiagram.Edges)
            {
                var biomeRight = terrainStructure.GetNodeIDFromSite(edge.RightSite.Coord);
                var biomeLeft = terrainStructure.GetNodeIDFromSite(edge.LeftSite.Coord);

                //Discard nodes in the same area or not in this area
                if (!AreaBiomes[i].Contains(biomeRight) && !AreaBiomes[i].Contains(biomeLeft) ||
                    AreaBiomes[i].Contains(biomeRight) && AreaBiomes[i].Contains(biomeLeft))
                    continue;

                areaEdges.Add(edge);
            }

            // Area Polygon
            AreaPolygons[i] = areaEdges.EdgesToPolygon().ToArray();
        }
    }

    /* Fill an area with prefabs */
    private static GameObject PoissonDiskFill(TerrainStructure terrainStructure, PoissonDiskFill poissonDiskFill, Terrain terrain)
    {
        var result = new GameObject("SceneryAreaFill");
        result.transform.position = Vector3.zero;
        result.transform.rotation = Quaternion.identity;

        if (poissonDiskFill.Prefabs == null || poissonDiskFill.Prefabs.Length <= 0)
            return result;

        var levelCreator = LevelCreator.Instance;
        var size = poissonDiskFill.FrameSize;
        PoissonDiskGenerator.minDist = poissonDiskFill.MinDist;
        PoissonDiskGenerator.sampleRange = (size.x > size.y ? size.x : size.y);
        PoissonDiskGenerator.Generate();
        foreach (var sample in PoissonDiskGenerator.ResultSet)
        {
            var point = sample + poissonDiskFill.FramePosition;
            var height = terrain.SampleHeight(new Vector3(point.x, 0, point.y) - terrain.transform.position);
            if (height <= (levelCreator.WaterHeight + 0.01f) * terrain.terrainData.size.y || // not underwater
                !point.IsInsidePolygon(poissonDiskFill.Polygon) || //not outside of the area
                !poissonDiskFill.ClearPolygons.TrueForAll(a => !point.IsInsidePolygon(a)) //not inside of any clear polygon
            )
                continue;

            var go = Object.Instantiate(poissonDiskFill.Prefabs[Random.Range(0, poissonDiskFill.Prefabs.Length)]);
            go.transform.position = new Vector3(point.x, height, point.y) + terrain.transform.position;
            go.transform.rotation = Quaternion.Euler(go.transform.rotation.eulerAngles.x, Random.Range(0, 360f),
                go.transform.rotation.eulerAngles.z);
            go.transform.parent = result.transform;
        }

        return result;
    }

    private static void GenerateBorderEdges(TerrainStructure terrainStructure, int numberOfAreas, HashSet<int>[] areaBiomes, List<Vector2Int> areaCrossingNavigationEdges, List<Vector2[]> areaBorders, Vector2[][] areaCrossingBorders)
    {
        var innerBorderEdges = new List<Edge>(128);
        var crossableEdges = new Edge[numberOfAreas - 1];
        var outerBorderEdges = new List<KeyValuePair<Edge, Vector2>>();

        foreach (var edge in terrainStructure.VoronoiDiagram.Edges)
        {

            //Check if this edge is visible before continuing
            if (!edge.Visible()) continue;

            var biomeRight = terrainStructure.GetNodeIDFromSite(edge.RightSite.Coord);
            var biomeLeft = terrainStructure.GetNodeIDFromSite(edge.LeftSite.Coord);
            var areaRight = -1;
            var areaLeft = -1;

            //Check in which area each biome is
            for (var i = 0; i < numberOfAreas; i++)
            {
                if (areaBiomes[i].Contains(biomeRight))
                    areaRight = i;
                if (areaBiomes[i].Contains(biomeLeft))
                    areaLeft = i;
            }

            //Check if areas differ
            if (areaLeft == areaRight) continue;

            //Check if any areas is a coastal area
            if (areaLeft != -1 && areaRight != -1)
            {
                //Check if edge is crossable between areas or not
                if (areaCrossingNavigationEdges.Contains(new Vector2Int(biomeLeft, biomeRight))
                    || areaCrossingNavigationEdges.Contains(new Vector2Int(biomeRight, biomeLeft)))
                {
                    //Find lowest area id
                    crossableEdges[areaLeft < areaRight ? areaLeft : areaRight] = edge;
                }
                else
                    innerBorderEdges.Add(edge);
            }
            else
            {
                // Add coast edge with the biome center to scale inwards later
                outerBorderEdges.Add(new KeyValuePair<Edge, Vector2>(edge, areaRight != -1 ? edge.RightSite.Coord.ToUnityVector2() : edge.LeftSite.Coord.ToUnityVector2()));
            }
        }

        //Add local variables to global variables
        foreach (var edge in innerBorderEdges)
        {
            if (!edge.Visible()) continue;
            areaBorders.Add(new[] { edge.ClippedEnds[LR.LEFT].ToUnityVector2(), edge.ClippedEnds[LR.RIGHT].ToUnityVector2() });
        }

        for (var i = 0; i < areaCrossingBorders.Length; i++)
        {
            var edge = crossableEdges[i];
            areaCrossingBorders[i] = new[] { edge.ClippedEnds[LR.LEFT].ToUnityVector2(), edge.ClippedEnds[LR.RIGHT].ToUnityVector2() };
        }
    }
}

/* Class for a standard poisson disk fill */
public class PoissonDiskFill
{
    public readonly Vector2[] Polygon;
    public readonly List<Vector2[]> ClearPolygons;
    public readonly GameObject[] Prefabs;
    public readonly Vector2 FrameSize;
    public readonly Vector2 FramePosition;
    public readonly float MinDist;

    public PoissonDiskFill(GameObject[] prefabs, Vector2[] polygon, float minDist)
    {
        ClearPolygons = new List<Vector2[]>();

        Prefabs = prefabs;
        Polygon = polygon;
        MinDist = minDist;

        var minX = float.MaxValue;
        var minY = float.MaxValue;
        var maxX = float.MinValue;
        var maxY = float.MinValue;

        foreach (var point in polygon)
        {
            minX = Mathf.Min(point.x, minX);
            minY = Mathf.Min(point.y, minY);
            maxX = Mathf.Max(point.x, maxX);
            maxY = Mathf.Max(point.y, maxY);
        }

        FramePosition = new Vector2(minX, minY);
        FrameSize = new Vector2(maxX - minX, maxY - minY);
    }

    public void AddClearPolygon(Vector2[] polygon)
    {
        ClearPolygons.Add(polygon);
    }
}