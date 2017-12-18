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
        var greatestPath = GetLargestPathInMST();

        //Divide path in numAreas Areas
        var areaStartingNodes = new List<int>();
        var crossingEdges = new List<Vector2Int>(numAreas - 1);
        var separatedAreas = new Graph<Biome>(_terrainStructure.MinimumSpanningTree);
        for (var i = 0; i < numAreas - 1; i++)
        {
            crossingEdges[i] = new Vector2Int(greatestPath[greatestPath.Count / numAreas * i], greatestPath[greatestPath.Count / numAreas * i + 1]);
            separatedAreas.RemoveEdge(crossingEdges[i].x, crossingEdges[i].y);
            areaStartingNodes.Add(crossingEdges[i].x);
        }
        areaStartingNodes.Add(crossingEdges.Last().y);

        //Group nodes in each area
        var areaNodes = new List<int>[numAreas];
        for (var i = 0; i < areaNodes.Length; i++)
        {
            areaNodes[i] = new List<int> {areaStartingNodes[i]};
            while()
        }

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

    private List<int> GetLargestPathInMST()
    {
        return GetLargestPathRecursion(_terrainStructure.StartBiomeNode.Value, new List<int>());
    }

    private List<int> GetLargestPathRecursion(int currentNode, List<int> path)
    {
        path.Add(currentNode);
        var greatestPathSize = 0;
        var greatestPath = new List<int>();
        foreach (var neighbor in _terrainStructure.MinimumSpanningTree.GetNeighbours(currentNode))
        {
            //Skip neighbors already in the path
            if (path.Contains(neighbor))
                continue;

            var neighborPath = GetLargestPathRecursion(neighbor, path);
            if (neighborPath.Count > greatestPathSize)
            {
                greatestPathSize = neighborPath.Count;
                greatestPath = neighborPath;
            }
        }

        return path.Concat(greatestPath).ToList();
    }

    private List<int> GetConnectedNodes(List<int> list)
    {
        
    }
}

public enum WorldGenerationMethod
{
    MinimumSpanningTree,
    FullyConnectedGraph
}
