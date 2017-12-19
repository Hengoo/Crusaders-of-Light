using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using csDelaunay;
using UnityEngine;

public class WorldStructure
{
    private readonly TerrainStructure _terrainStructure;
    public Graph<Biome> NavigationGraph;

    public WorldStructure(TerrainStructure terrainStructure, int numAreas, WorldGenerationMethod method)
    {
        _terrainStructure = terrainStructure;
        switch (method)
        {
            case WorldGenerationMethod.MinimumSpanningTree:
                GenerateMSP(numAreas, terrainStructure.BiomeGraph.EdgeCount() / 2);
                break;
            case WorldGenerationMethod.FullyConnectedGraph:
                GenerateFCG();
                break;
            default:
                throw new ArgumentOutOfRangeException("method", method, null);
        }
    }

    private void GenerateMSP(int numAreas, int extraEdges)
    {
        //Find largest path
        var greatestPath = GetLargestPathInMST(new Graph<Biome>(_terrainStructure.MinimumSpanningTree));

        return;

        foreach (var VARIABLE in greatestPath)
        {
            Debug.Log(VARIABLE);
        }

        //Divide path in numAreas Areas
        var areaStartingNodes = new List<int>();
        var crossingEdges = new List<Vector2Int>(numAreas - 1);
        var separatedAreas = new Graph<Biome>(_terrainStructure.MinimumSpanningTree);
        for (var i = 0; i < numAreas - 1; i++)
        {
            crossingEdges.Add(new Vector2Int(greatestPath[greatestPath.Count / numAreas * (i + 1)], greatestPath[greatestPath.Count / numAreas * (i + 1) + 1]));
            separatedAreas.RemoveEdge(crossingEdges[i].x, crossingEdges[i].y);
            _terrainStructure.MinimumSpanningTree.RemoveEdge(crossingEdges[i].x, crossingEdges[i].y);
            areaStartingNodes.Add(crossingEdges[i].x);
            Debug.Log("GP: " + greatestPath.Count + ", AREA: " + crossingEdges[i].x);
        }
        areaStartingNodes.Add(crossingEdges.Last().y);
        Debug.Log("GP: " + greatestPath.Count + ", AREA: " + crossingEdges.Last().y);



        ////Group nodes in each area
        //var areaNodes = new HashSet<int>[numAreas];
        //for (var i = 0; i < areaNodes.Length; i++)
        //{
        //    areaNodes[i] = GetConnectedNodes(areaStartingNodes[i], separatedAreas, new HashSet<int>());
        //}

        //Create area graphs
        //TODO: implement
    }

    private void GenerateFCG()
    {
        //TODO: implement
    }

    private Vector2[] GenerateAreaPolygon(List<Edge> edges)
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


        var areas = new GameObject("Areas");
        areas.transform.parent = result.transform;

        var edgesIn = new GameObject("Inner Edges");
        edgesIn.transform.parent = result.transform;

        var borders = new GameObject("Borders");
        borders.transform.parent = result.transform;

        return result;
    }
}

public enum WorldGenerationMethod
{
    MinimumSpanningTree,
    FullyConnectedGraph
}
