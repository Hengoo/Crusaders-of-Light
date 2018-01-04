using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using csDelaunay;
using UnityEngine;
using Random = UnityEngine.Random;

public class WorldStructure
{
    public Graph<Biome> NavigationGraph { get; private set; }
    public List<Vector2Int> AreaCrossingNavigationEdges { get; private set; }
    public List<Vector2[]> AreaCrossingBorders { get; private set; }
    public List<Vector2[]> AreaPolygons { get; private set; }
    public List<Vector2[]> AreaBorders { get; private set; }
    public List<Vector2> CoastBlockerPolygon { get; private set; }
    public int NumberOfAreas { get; private set; }
    private readonly TerrainStructure _terrainStructure;

    public WorldStructure(TerrainStructure terrainStructure, int numAreas, int extraEdges)
    {
        _terrainStructure = terrainStructure;
        NavigationGraph = new Graph<Biome>(_terrainStructure.MinimumSpanningTree);
        AreaCrossingNavigationEdges = new List<Vector2Int>(numAreas - 1);
        AreaPolygons = new List<Vector2[]>(numAreas);
        AreaBorders = new List<Vector2[]>();
        AreaCrossingBorders = new List<Vector2[]>();
        CoastBlockerPolygon = new List<Vector2>();
        NumberOfAreas = numAreas;

        GenerateAreas(extraEdges);
    }

    private void GenerateAreas(int extraEdges)
    {
        //Find largest path
        var greatestPath = GetLargestPathInMST(new Graph<Biome>(_terrainStructure.MinimumSpanningTree));
        _terrainStructure.EndBiomeNode = new KeyValuePair<Vector2f, int>(new Vector2f(_terrainStructure.BiomeGraph.GetNodeData(greatestPath.Last()).Center), greatestPath.Last());

        //Divide path in NumberOfAreas Areas
        var areaStartingNodes = new List<int>();
        var areaSize = greatestPath.Count / NumberOfAreas;
        for (var i = 0; i < NumberOfAreas - 1; i++)
        {
            var pos = areaSize * i + areaSize / (NumberOfAreas - 1);
            AreaCrossingNavigationEdges.Add(new Vector2Int(greatestPath[pos], greatestPath[pos + 1]));
            NavigationGraph.RemoveEdge(AreaCrossingNavigationEdges[i].x, AreaCrossingNavigationEdges[i].y);
            areaStartingNodes.Add(AreaCrossingNavigationEdges[i].x);
        }
        areaStartingNodes.Add(AreaCrossingNavigationEdges.Last().y);

        //Group nodes in each area
        var areaNodes = new HashSet<int>[NumberOfAreas];
        for (var i = 0; i < areaNodes.Length; i++)
        {
            areaNodes[i] = GetConnectedNodes(areaStartingNodes[i], NavigationGraph, new HashSet<int>());
        }

        //Create navigation graph for each area
        var tempGraph = new Graph<Biome>(_terrainStructure.BiomeGraph);
        foreach (var edge in NavigationGraph.GetAllEdges())
            tempGraph.RemoveEdge(edge.x, edge.y);

        var tries = extraEdges * 10;
        while (extraEdges > 0 && tries > 0)
        {
            var success = false;
            if (tempGraph.GetAllEdges().Length <= 0)
                break;
            var edge = tempGraph.GetAllEdges()[Random.Range(0, tempGraph.GetAllEdges().Length)];
            tempGraph.RemoveEdge(edge.x, edge.y);
            foreach (var set in areaNodes)
            {
                var xNeighbors = NavigationGraph.GetNeighbours(edge.x);
                var yNeighbors = NavigationGraph.GetNeighbours(edge.y);
                if (set.Contains(edge.x) && set.Contains(edge.y) && !xNeighbors.Intersect(yNeighbors).Any())
                {
                    NavigationGraph.AddEdge(edge.x, edge.y, 1);
                    success = true;
                }
            }

            if (success)
                extraEdges--;
            else
                tries--;
        }
        if (tries <= 0)
            Debug.LogWarning("Failed to add all extra edges. Remaining: " + extraEdges);

        //Create area polygon
        for (var i = 0; i < NumberOfAreas; i++)
        {
            var areaEdges = new List<Edge>();

            //Get the edges of this area
            foreach (var edge in _terrainStructure.VoronoiDiagram.Edges)
            {
                var biomeRight = _terrainStructure.GetNodeIDFromSite(edge.RightSite.Coord);
                var biomeLeft = _terrainStructure.GetNodeIDFromSite(edge.LeftSite.Coord);

                //Discard nodes in the same area or not in this area
                if (!areaNodes[i].Contains(biomeRight) && !areaNodes[i].Contains(biomeLeft) ||
                    areaNodes[i].Contains(biomeRight) && areaNodes[i].Contains(biomeLeft))
                    continue;

                areaEdges.Add(edge);
            }

            // Area Polygon
            AreaPolygons.Add(areaEdges.EdgesToPolygon().ToArray());
        }

        //Create border line segments
        var borderEdges = new List<Edge>(128);
        var crossableEdges = new List<Edge>(128);
        var coastEdges = new List<KeyValuePair<Edge, Vector2>>();
        foreach (var edge in _terrainStructure.VoronoiDiagram.Edges)
        {
            var biomeRight = _terrainStructure.GetNodeIDFromSite(edge.RightSite.Coord);
            var biomeLeft = _terrainStructure.GetNodeIDFromSite(edge.LeftSite.Coord);
            var areaRight = -1;
            var areaLeft = -1;

            //Check in which area each biome is
            for (var i = 0; i < NumberOfAreas; i++)
            {
                if (areaNodes[i].Contains(biomeRight))
                    areaRight = i;
                if (areaNodes[i].Contains(biomeLeft))
                    areaLeft = i;
            }

            //Check if areas differ
            if (areaLeft == areaRight) continue;

            //Check if any areas is a coastal area
            if (areaLeft != -1 && areaRight != -1)
            {
                //Check if edge is crossable between areas or not
                if (AreaCrossingNavigationEdges.Contains(new Vector2Int(biomeLeft, biomeRight))
                    || AreaCrossingNavigationEdges.Contains(new Vector2Int(biomeRight, biomeLeft)))
                    crossableEdges.Add(edge);
                else
                    borderEdges.Add(edge);
            }
            else
            {
                // Add coast edge with the biome center to scale inwards later
                coastEdges.Add(new KeyValuePair<Edge, Vector2>(edge, areaRight != -1 ? edge.RightSite.Coord.ToUnityVector2() : edge.LeftSite.Coord.ToUnityVector2()));
            }
        }

        //Add local variables to global variables
        foreach (var edge in borderEdges)
        {
            if (!edge.Visible()) continue;
            AreaBorders.Add(new []{edge.ClippedEnds[LR.LEFT].ToUnityVector2(), edge.ClippedEnds[LR.RIGHT].ToUnityVector2() });
        }

        foreach (var edge in crossableEdges)
        {
            if (!edge.Visible()) continue;
            AreaCrossingBorders.Add(new[] { edge.ClippedEnds[LR.LEFT].ToUnityVector2(), edge.ClippedEnds[LR.RIGHT].ToUnityVector2() });
        }

        var coastLines = coastEdges.Select(pair => pair.Key).ToList().EdgesToSortedLines();
        foreach (var line in coastLines)
        {

            //Offset borders towards biome center
            var left = line[0];
            var right = line[1];
            var center = Vector2.zero;
            coastEdges.ForEach(e =>
            {
                var l = e.Key.ClippedEnds[LR.LEFT].ToUnityVector2();
                var r = e.Key.ClippedEnds[LR.RIGHT].ToUnityVector2();
                if ((l == left || l == right) && (r == left || r == right))
                    center = e.Value;
            });

            left += (center - left).normalized * _terrainStructure.BiomeGlobalConfiguration.CoastInlandOffset;
            right += (center - right).normalized * _terrainStructure.BiomeGlobalConfiguration.CoastInlandOffset;

            //Offsetting can give duplicated points
            if (!CoastBlockerPolygon.Contains(left))
                CoastBlockerPolygon.Add(left);
            if (!CoastBlockerPolygon.Contains(right))
                CoastBlockerPolygon.Add(right);
        }
    }

    private List<int> GetLargestPathInMST(Graph<Biome> mst)
    {
        return GetLargestPathRecursion(_terrainStructure.StartBiomeNode.Value, -1, mst);
    }

    private List<int> GetLargestPathRecursion(int currentNode, int parent, Graph<Biome> graph)
    {
        var path = new List<int> { currentNode };
        var longest = new List<int>();
        var neighborhood = graph.GetNeighbours(currentNode);
        foreach (var neighbor in neighborhood)
        {
            if (neighbor == parent || neighbor == currentNode)
                continue;

            var newPath = GetLargestPathRecursion(neighbor, currentNode, graph);
            if (newPath.Count > longest.Count)
                longest = newPath;
        }

        return path.Concat(longest).ToList();
    }

    private HashSet<int> GetConnectedNodes(int currentNode, Graph<Biome> biomeGraph, HashSet<int> set)
    {
        set.Add(currentNode);
        foreach (var neighbor in biomeGraph.GetNeighbours(currentNode))
        {
            if (set.Contains(neighbor)) continue;
            set.UnionWith(GetConnectedNodes(neighbor, biomeGraph, set));
        }

        return set;
    }


    public GameObject DrawAreaGraph(float scale)
    {
        var result = new GameObject();
        
        var borders = new GameObject("Area Borders");
        borders.transform.parent = result.transform;

        var minimumSpanningTree = new GameObject("Minimum Spanning Tree");
        minimumSpanningTree.transform.parent = result.transform;

        var navigationEdges = new GameObject("Navigation Edges");
        navigationEdges.transform.parent = result.transform;

        var polygons = new GameObject("Area Polygon");
        polygons.transform.parent = result.transform;

        //Draw navigation edges
        foreach (var edge in NavigationGraph.GetAllEdges())
        {
            var biome1 = NavigationGraph.GetNodeData(edge.x);
            var biome2 = NavigationGraph.GetNodeData(edge.y);

            var p0 = new Vector3(biome1.Center.x, 0, biome1.Center.y);
            var p1 = new Vector3(biome2.Center.x, 0, biome2.Center.y);
            GameObject myLine = new GameObject("Nav Line");
            myLine.transform.position = p0;
            myLine.transform.parent = navigationEdges.transform;
            LineRenderer lr = myLine.AddComponent<LineRenderer>();
            lr.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
            lr.startColor = Color.white;
            lr.endColor = Color.white;
            lr.startWidth = 2 * scale;
            lr.endWidth = 2 * scale;
            lr.SetPosition(0, p0);
            lr.SetPosition(1, p1);
        }

        //Draw minimum spanning tree
        foreach (var edge in _terrainStructure.MinimumSpanningTree.GetAllEdges())
        {
            var biome1 = _terrainStructure.MinimumSpanningTree.GetNodeData(edge.x);
            var biome2 = _terrainStructure.MinimumSpanningTree.GetNodeData(edge.y);

            var p0 = new Vector3(biome1.Center.x, 0, biome1.Center.y);
            var p1 = new Vector3(biome2.Center.x, 0, biome2.Center.y);
            GameObject myLine = new GameObject("MST Line");
            myLine.transform.position = p0;
            myLine.transform.parent = minimumSpanningTree.transform;
            LineRenderer lr = myLine.AddComponent<LineRenderer>();
            lr.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
            lr.startColor = Color.white;
            lr.endColor = Color.white;
            lr.startWidth = 2 * scale;
            lr.endWidth = 2 * scale;
            lr.SetPosition(0, p0);
            lr.SetPosition(1, p1);

        }

        //Draw area polygon
        int count = 0;
        foreach (var polygon in AreaPolygons)
        {
            var poly = new GameObject("Area" + count);
            poly.transform.parent = polygons.transform;
            for (int i = 0; i < polygon.Length; i++)
            {
                var p0 = new Vector3(polygon[i].x, 0, polygon[i].y);
                var p1 = new Vector3(polygon[(i + 1) % polygon.Length].x, 0, polygon[(i + 1) % polygon.Length].y);
                GameObject myLine = new GameObject("Area Line");
                myLine.transform.position = p0;
                myLine.transform.parent = poly.transform;
                LineRenderer lr = myLine.AddComponent<LineRenderer>();
                lr.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
                lr.startColor = Color.white;
                lr.endColor = Color.white;
                lr.startWidth = 2 * scale;
                lr.endWidth = 2 * scale;
                lr.SetPosition(0, p0);
                lr.SetPosition(1, p1);
            }

            count++;
        }

        //Draw area border
        foreach (var line in AreaBorders)
        {
            var p0 = new Vector3(line[0].x, 0, line[0].y);
            var p1 = new Vector3(line[1].x, 0, line[1].y);
            GameObject myLine = new GameObject("Border Line");
            myLine.transform.position = p0;
            myLine.transform.parent = borders.transform;
            LineRenderer lr = myLine.AddComponent<LineRenderer>();
            lr.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
            lr.startColor = Color.white;
            lr.endColor = Color.white;
            lr.startWidth = 2 * scale;
            lr.endWidth = 2 * scale;
            lr.SetPosition(0, p0);
            lr.SetPosition(1, p1);
        }

        //Draw area crossing edges
        foreach (var line in AreaCrossingBorders)
        {
            var p0 = new Vector3(line[0].x, 0, line[0].y);
            var p1 = new Vector3(line[1].x, 0, line[1].y);
            GameObject myLine = new GameObject("Crossable Border Line");
            myLine.transform.position = p0;
            myLine.transform.parent = borders.transform;
            LineRenderer lr = myLine.AddComponent<LineRenderer>();
            lr.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
            lr.startColor = (Color.red + Color.yellow) / 2;
            lr.endColor = (Color.red + Color.yellow) / 2;
            lr.startWidth = 2 * scale;
            lr.endWidth = 2 * scale;
            lr.SetPosition(0, p0);
            lr.SetPosition(1, p1);
            lr.sortingOrder = 1;
        }
        
        //Draw area crossing paths
        foreach (var pair in AreaCrossingNavigationEdges)
        {
            var center1 = _terrainStructure.BiomeGraph.GetNodeData(pair.x).Center;
            var center2 = _terrainStructure.BiomeGraph.GetNodeData(pair.y).Center;

            var p0 = new Vector3(center1.x, 0, center1.y);
            var p1 = new Vector3(center2.x, 0, center2.y);
            GameObject myLine = new GameObject("Crossable Path Line");
            myLine.transform.position = p0;
            myLine.transform.parent = navigationEdges.transform;
            LineRenderer lr = myLine.AddComponent<LineRenderer>();
            lr.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
            lr.startColor = Color.yellow;
            lr.endColor = Color.yellow;
            lr.startWidth = 2 * scale;
            lr.endWidth = 2 * scale;
            lr.SetPosition(0, p0);
            lr.SetPosition(1, p1);
            lr.sortingOrder = 1;
        }

        //Draw Start
        var start = _terrainStructure.StartBiomeNode;
        var startPos = new Vector2(start.Key.x, start.Key.y);
        var startGo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        startGo.name = "Start";
        startGo.GetComponent<Collider>().enabled = false;
        startGo.transform.parent = result.transform;
        startGo.transform.position = new Vector3(startPos.x, 0, startPos.y);
        startGo.transform.localScale = Vector3.one * 20 * scale;
        var startRenderer = startGo.GetComponent<Renderer>();
        var startTempMaterial = new Material(startRenderer.sharedMaterial) { color = Color.red };
        startRenderer.sharedMaterial = startTempMaterial;

        //Draw End
        var biome = _terrainStructure.EndBiomeNode;
        var pos = new Vector2(biome.Key.x, biome.Key.y);
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = "End";
        go.GetComponent<Collider>().enabled = false;
        go.transform.parent = result.transform;
        go.transform.position = new Vector3(pos.x, 0, pos.y);
        go.transform.localScale = Vector3.one * 20 * scale;
        var renderer = go.GetComponent<Renderer>();
        var tempMaterial = new Material(renderer.sharedMaterial) { color = Color.blue };
        renderer.sharedMaterial = tempMaterial;

        return result;
    }
}
