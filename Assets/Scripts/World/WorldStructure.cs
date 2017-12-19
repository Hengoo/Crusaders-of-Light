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
    public List<Vector2Int> AreaCrossingEdges { get; private set; }
    public List<Vector2[]> AreaPolygon { get; private set; }
    public int NumberOfAreas { get; private set; }
    private readonly TerrainStructure _terrainStructure;


    public WorldStructure(TerrainStructure terrainStructure, int numAreas, WorldGenerationMethod method)
    {
        _terrainStructure = terrainStructure;
        NavigationGraph = new Graph<Biome>(_terrainStructure.MinimumSpanningTree);
        AreaCrossingEdges = new List<Vector2Int>(numAreas - 1);
        AreaPolygon = new List<Vector2[]>(numAreas);
        NumberOfAreas = numAreas;
        switch (method)
        {
            case WorldGenerationMethod.MinimumSpanningTree:
                GenerateMSP(terrainStructure.MinimumSpanningTree.EdgeCount() / 3);
                break;
            case WorldGenerationMethod.FullyConnectedGraph:
                GenerateFCG();
                break;
            default:
                throw new ArgumentOutOfRangeException("method", method, null);
        }
    }

    private void GenerateMSP(int extraEdges)
    {
        //Find largest path
        var greatestPath = GetLargestPathInMST(new Graph<Biome>(_terrainStructure.MinimumSpanningTree));

        //Divide path in NumberOfAreas Areas
        var areaStartingNodes = new List<int>();
        for (var i = 0; i < NumberOfAreas - 1; i++)
        {
            AreaCrossingEdges.Add(new Vector2Int(greatestPath[greatestPath.Count / NumberOfAreas * (i + 1)], greatestPath[greatestPath.Count / NumberOfAreas * (i + 1) + 1]));
            NavigationGraph.RemoveEdge(AreaCrossingEdges[i].x, AreaCrossingEdges[i].y);
            areaStartingNodes.Add(AreaCrossingEdges[i].x);
        }
        areaStartingNodes.Add(AreaCrossingEdges.Last().y);

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
                if (set.Contains(edge.x) && set.Contains(edge.y) && !(xNeighbors.Intersect(yNeighbors).Any()))
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

            var polygon = new List<Vector2>();
            var reorderer = new EdgeReorderer(areaEdges, typeof(Vertex));
            for (var j = 0; j < reorderer.Edges.Count; j++)
            {
                var edge = reorderer.Edges[j];
                if (!edge.Visible()) continue;

                polygon.Add(edge.ClippedEnds[reorderer.EdgeOrientations[j]].ToUnityVector2());
            }

            AreaPolygon.Add(polygon.ToArray());
        }
    }

    private void GenerateFCG()
    {
        //TODO: implement if needed
    }

    private Vector2[] GenerateAreaPolygon(int area, List<Edge> edges)
    {
        //TODO implement
        return null;
    }

    private List<int> GetLargestPathInMST(Graph<Biome> mst)
    {
        return GetLargestPathRecursion(_terrainStructure.StartBiomeNode.Value, -1, mst);
    }

    private List<int> GetLargestPathRecursion(int currentNode, int parent, Graph<Biome> graph)
    {
        var path = new List<int> {currentNode};
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

        var minimumSpanningTree = new GameObject("Minimum Spanning Tree");
        minimumSpanningTree.transform.parent = result.transform;

        var navigationEdges = new GameObject("Navigation Edges");
        navigationEdges.transform.parent = result.transform;

        var borders = new GameObject("Area Borders");
        borders.transform.parent = result.transform;

        //Draw navigation edges
        foreach (var edge in NavigationGraph.GetAllEdges())
        {
            var biome1 = NavigationGraph.GetNodeData(edge.x);
            var biome2 = NavigationGraph.GetNodeData(edge.y);

            var start = new Vector3(biome1.Center.x, 0, biome1.Center.y);
            var end = new Vector3(biome2.Center.x, 0, biome2.Center.y);
            GameObject myLine = new GameObject("Line");
            myLine.transform.position = start;
            myLine.transform.parent = navigationEdges.transform;
            LineRenderer lr = myLine.AddComponent<LineRenderer>();
            lr.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
            lr.startColor = Color.white;
            lr.endColor = Color.white;
            lr.startWidth = 2 * scale;
            lr.endWidth = 2 * scale;
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);
        }

        //Draw minimum spanning tree
        foreach (var edge in _terrainStructure.MinimumSpanningTree.GetAllEdges())
        {
            var biome1 = _terrainStructure.MinimumSpanningTree.GetNodeData(edge.x);
            var biome2 = _terrainStructure.MinimumSpanningTree.GetNodeData(edge.y);

            var start = new Vector3(biome1.Center.x, 0, biome1.Center.y);
            var end = new Vector3(biome2.Center.x, 0, biome2.Center.y);
            GameObject myLine = new GameObject("Line");
            myLine.transform.position = start;
            myLine.transform.parent = minimumSpanningTree.transform;
            LineRenderer lr = myLine.AddComponent<LineRenderer>();
            lr.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
            lr.startColor = Color.white;
            lr.endColor = Color.white;
            lr.startWidth = 2 * scale;
            lr.endWidth = 2 * scale;
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);

        }

        //Draw area polygon
        foreach (var polygon in AreaPolygon)
        {
            for (int i = 0; i < polygon.Length; i++)
            {
                var start = new Vector3(polygon[i].x, 0, polygon[i].y);
                var end = new Vector3(polygon[(i + 1) % polygon.Length].x, 0, polygon[(i + 1) % polygon.Length].y);
                GameObject myLine = new GameObject("Border");
                myLine.transform.position = start;
                myLine.transform.parent = borders.transform;
                LineRenderer lr = myLine.AddComponent<LineRenderer>();
                lr.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
                lr.startColor = Color.white;
                lr.endColor = Color.white;
                lr.startWidth = 2 * scale;
                lr.endWidth = 2 * scale;
                lr.SetPosition(0, start);
                lr.SetPosition(1, end);
            }
        }

        return result;
    }
}

public enum WorldGenerationMethod
{
    MinimumSpanningTree,
    FullyConnectedGraph
}
