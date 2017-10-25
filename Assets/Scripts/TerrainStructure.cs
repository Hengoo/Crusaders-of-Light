using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TerrainStructure
{
    private readonly Graph<BiomeData> _biomes = new Graph<BiomeData>();
    private readonly List<int> _biomeIDs = new List<int>();

    public TerrainStructure(List<BiomeData> biomes, int height, int width)
    {
        //Add all points to the graph
        foreach (var biome in biomes)
        {
            _biomeIDs.Add(_biomes.AddNode(biome));
        }

        // Calculate connectivity between biome
        List<Triangle> triangles = DelaunayTriangulation(height, width);

        // Add biome connectivity to graph
        foreach (var triangle in triangles)
        {
            _biomes.AddEdge(triangle.P0, triangle.P1, 1);
            _biomes.AddEdge(triangle.P0, triangle.P2, 1);
            _biomes.AddEdge(triangle.P1, triangle.P2, 1);
        }
    }

    private List<Triangle> DelaunayTriangulation(int height, int width)
    {
        // Encompassing biomes for Bowyer-Watson
        var left = _biomes.AddNode(new BiomeData(new Vector2(-width, -height / 2f), 0, 0, 0));
        var right = _biomes.AddNode(new BiomeData(new Vector2(width, -height / 2f), 0, 0, 0));
        var top = _biomes.AddNode(new BiomeData(new Vector2(0, height * 1.5f), 0, 0, 0));

        // Add super triangle
        List<Triangle> result = new List<Triangle> {new Triangle(left, top, right)};
        HashSet<int> toRetriangulate = new HashSet<int>();

        // Bowyer-Watson - iterate through all points
        foreach (var biomeID in _biomeIDs)
        {
            // Skip super triangle vertices
            if (biomeID == left || biomeID == right || biomeID == top)
                continue;

            Vector2 point = _biomes.GetNodeData(biomeID).Center;
            toRetriangulate.Clear();
            foreach (var triangle in result)
            {
                // Check which triangles need to be removed
                if (IsInCircumcircle(point, triangle))
                {
                    result.Remove(triangle);
                    toRetriangulate.Add(triangle.P0);
                    toRetriangulate.Add(triangle.P1);
                    toRetriangulate.Add(triangle.P2);
                }
            }
            // Add new triangles connecting to the new point
            var points = toRetriangulate.ToArray();
            for (int i = 0; i < points.Length; i++)
            {
                result.Add(new Triangle(biomeID, points[i], points[(i + 1) % points.Length]));
            }
        }

        // Remove super triangle
        _biomes.RemoveNode(left);
        _biomes.RemoveNode(right);
        _biomes.RemoveNode(top);
        foreach (var triangle in result)
            if (triangle.Contains(left) || triangle.Contains(right) || triangle.Contains(top))
                result.Remove(triangle);

        return result;
    }

    // See http://mathworld.wolfram.com/TriangleInterior.html
    private bool IsInCircumcircle(Vector2 v, Triangle triangle)
    {
        var v0 = _biomes.GetNodeData(triangle.P0).Center;
        var v1 = _biomes.GetNodeData(triangle.P1).Center - v0;
        var v2 = _biomes.GetNodeData(triangle.P2).Center - v0;

        var a = (Det(v, v2) - Det(v0, v2)) / Det(v1, v2);
        var b = - (Det(v, v1) - Det(v0, v1)) / Det(v1, v2);

        return a > 0 && b > 0 && a + b >0;
    }

    private float Det(Vector2 left, Vector2 right)
    {
        return left.x * right.y - left.y * right.x;
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

        public bool Contains(int id)
        {
            return id == P0 || id == P1 || id == P2;
        }
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
