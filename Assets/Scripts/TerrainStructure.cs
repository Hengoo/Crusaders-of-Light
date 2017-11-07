using System;
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
        List<Triangle> result = new List<Triangle> {new Triangle(left, right, top)};
        List<int> toRetriangulate = new List<int>();

        // Bowyer-Watson - iterate through all points
        List<Triangle> tempResult;
        foreach (var biomeID in _biomeIDs)
        {
            // Skip super triangle vertices
            if (biomeID == left || biomeID == right || biomeID == top)
                continue;

            Debug.Log("PASS " + biomeID);
            Vector2 point = _biomes.GetNodeData(biomeID).Center;
            toRetriangulate.Clear();
            tempResult = new List<Triangle>(result);
            foreach (var triangle in tempResult)
            {
                // Check which triangles need to be removed
                if (IsInCircumcircle(point, triangle))
                {
                    Debug.Log("Removed triangle " + triangle + " " + result.Remove(triangle));
                    toRetriangulate.Add(triangle.P0);
                    toRetriangulate.Add(triangle.P1);
                    toRetriangulate.Add(triangle.P2);
                }
            }

            // Add new triangles connecting to the new point
            var points = toRetriangulate.ToArray();
            for (int i = 0; i < points.Length; i += 3)
            {
                var triangle1 = new Triangle(biomeID, points[i], points[i + 1]);
                var triangle2 = new Triangle(biomeID, points[i], points[i + 2]);
                var triangle3 = new Triangle(biomeID, points[i + 1], points[i + 2]);
                Debug.Log("Added triangle " + triangle1 + " | " + triangle2 + " | " + triangle3);
                result.Add(triangle1);
                result.Add(triangle2);
                result.Add(triangle3);
            }
        }

        // Remove super triangle
        /*_biomes.RemoveNode(left);
        _biomes.RemoveNode(right);
        _biomes.RemoveNode(top);
        tempResult = new List<Triangle>(result);
        foreach (var triangle in tempResult)
            if (triangle.Contains(left) || triangle.Contains(right) || triangle.Contains(top))
                result.Remove(triangle);*/

        return result;
    }

    //http://mathworld.wolfram.com/Circumcircle.html
    private bool IsInCircumcircle(Vector2 q, Triangle triangle)
    {
        var p0 = _biomes.GetNodeData(triangle.P0).Center;
        var p1 = _biomes.GetNodeData(triangle.P1).Center;
        var p2 = _biomes.GetNodeData(triangle.P2).Center;

        var MatA = new Matrix(3, 3);
        MatA[0, 0] = p0.x; MatA[0, 1] = p0.y; MatA[0, 2] = 1;
        MatA[1, 0] = p2.x; MatA[1, 1] = p1.y; MatA[1, 2] = 1;
        MatA[2, 0] = p1.x; MatA[2, 1] = p2.y; MatA[2, 2] = 1;
        var a = MatA.Det();

        var MatBx = new Matrix(3, 3);
        MatBx[0, 0] = Mathf.Pow(p0.x, 2) + Mathf.Pow(p0.y, 2); MatBx[0, 1] = p0.y; MatBx[0, 2] = 1;
        MatBx[1, 0] = Mathf.Pow(p1.x, 2) + Mathf.Pow(p1.y, 2); MatBx[1, 1] = p1.y; MatBx[1, 2] = 1;
        MatBx[2, 0] = Mathf.Pow(p2.x, 2) + Mathf.Pow(p2.y, 2); MatBx[2, 1] = p2.y; MatBx[2, 2] = 1;
        var bx = MatBx.Det();

        var MatBy = new Matrix(3, 3);
        MatBy[0, 0] = Mathf.Pow(p0.x, 2) + Mathf.Pow(p0.y, 2); MatBy[0, 1] = p0.x; MatBy[0, 2] = 1;
        MatBy[1, 0] = Mathf.Pow(p1.x, 2) + Mathf.Pow(p1.y, 2); MatBy[1, 1] = p1.x; MatBy[1, 2] = 1;
        MatBy[2, 0] = Mathf.Pow(p2.x, 2) + Mathf.Pow(p2.y, 2); MatBy[2, 1] = p2.x; MatBy[2, 2] = 1;
        var by = MatBy.Det();

        var MatC = new Matrix(3, 3);
        MatC[0, 0] = Mathf.Pow(p0.x, 2) + Mathf.Pow(p0.y, 2); MatC[0, 1] = p0.x; MatC[0, 2] = p0.y;
        MatC[1, 0] = Mathf.Pow(p1.x, 2) + Mathf.Pow(p1.y, 2); MatC[1, 1] = p1.x; MatC[1, 2] = p1.y;
        MatC[2, 0] = Mathf.Pow(p2.x, 2) + Mathf.Pow(p2.y, 2); MatC[2, 1] = p2.x; MatC[2, 2] = p2.y;
        var c = MatC.Det();

        var result = a * (Mathf.Pow(q.x, 2) + Mathf.Pow(q.y, 2)) + bx * q.x + by * q.y + c;

        return result > 0;
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

    public BiomeSample SampleBiomeData(float x, float y)
    {
        return SampleBiomeData(new Vector2(x,y));
    }

    public GameObject DrawGraph()
    {
        Debug.Log(_biomes.GetAllEdges().Length);
        var graphObj = new GameObject("Graph");
        foreach (var biome in _biomeIDs)
        {
            var pos = _biomes.GetNodeData(biome).Center;
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.transform.parent = graphObj.transform;
            go.transform.position = new Vector3(pos.x, 0, pos.y);
            go.transform.localScale = Vector3.one * 20;
        }

        foreach (var edge in _biomes.GetAllEdges())
        {
            var start = new Vector3(_biomes.GetNodeData(edge.x).Center.x, 0 , _biomes.GetNodeData(edge.x).Center.y);
            var end = new Vector3(_biomes.GetNodeData(edge.y).Center.x, 0, _biomes.GetNodeData(edge.y).Center.y);
            GameObject myLine = new GameObject();
            myLine.transform.position = start;
            myLine.transform.parent = graphObj.transform;
            LineRenderer lr = myLine.AddComponent<LineRenderer>();
            lr.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
            lr.startColor = Color.white;
            lr.endColor = Color.white;
            lr.startWidth = 2;
            lr.endWidth = 2;
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);
        }

        return graphObj;
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

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var otherTri = (Triangle) obj;
            List<int> mine = new List<int> { P0, P1, P2 };
            List<int> other = new List<int> { otherTri.P0, otherTri.P1, otherTri.P2 };
            mine.Sort();
            other.Sort();
            return other[0] == mine[0] && other[1] == mine[1] && other[2] == mine[2];
        }

        public override string ToString()
        {
            return P0 + " " + P1 + " " + P2;
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
