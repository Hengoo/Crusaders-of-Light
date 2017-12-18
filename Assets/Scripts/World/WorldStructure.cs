using System;
using System.Collections;
using System.Collections.Generic;
using csDelaunay;
using UnityEngine;

public class WorldStructure
{
    private TerrainStructure _terrainStructure;
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
        //Find biggest path


        //Divide path in numAreas Areas
        //TODO: implement

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
}

public enum WorldGenerationMethod
{
    MinimumSpanningTree,
    FullyConnectedGraph
}
