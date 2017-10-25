using System.Collections.Generic;
using UnityEngine;

public class TerrainStructure : MonoBehaviour
{
    private readonly Graph<BiomeData> _biomes = new Graph<BiomeData>();
    private readonly List<int> _biomeIDs = new List<int>();

    public TerrainStructure(List<BiomeData> biomes)
    {
        //Add all points to the graph
        foreach (var biome in biomes)
        {
            _biomeIDs.Add(_biomes.AddNode(biome));
        }

        List<Triangle> triangles = DelaunayTriangulation();

        foreach (var triangle in triangles)
        {
            _biomes.AddEdge(triangle.P0, triangle.P1, 1);
            _biomes.AddEdge(triangle.P0, triangle.P2, 1);
            _biomes.AddEdge(triangle.P1, triangle.P2, 1);
        }
    }

    public BiomeSample SampleBiomeData(Vector2 position)
    {
        //TODO implement sampling
        return new BiomeSample(0, 0);
    }    

    private struct Triangle
    {
        public readonly int P0, P1, P2;

        // These are point IDs from the graph
        public Triangle(int p0, int p1, int p2)
        {
            P0 = p0;
            P1 = p1;
            P2 = p2;
        }
    }

    private List<Triangle> DelaunayTriangulation()
    {
        List<Triangle> result = new List<Triangle>();

        //TODO calculate delaunay triangulation biome centers
        foreach (var biomeID in _biomeIDs)
        {
            Vector2 center = _biomes.GetNodeData(biomeID).Center;
        }

        return result;
    }
}

public struct BiomeSample
{
    public readonly float Humidity, Temperature;
    public BiomeSample(float humidity, float temperature)
    {
        Humidity = humidity;
        Temperature = temperature;
    }
}

public class BiomeData
{
    public readonly Vector2 Center;
    public readonly float Influence;
    public readonly float Humidity;
    public readonly float Temperature;

    public BiomeData(Vector2 center, float influence, float humidity, float temperature)
    {
        Center = center;
        Influence = influence;
        Humidity = humidity;
        Temperature = temperature;
    }
}
