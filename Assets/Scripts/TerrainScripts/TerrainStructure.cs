using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;


public class TerrainStructure
{
    private readonly Graph<Biome> _biomes = new Graph<Biome>();
    private readonly List<int> _biomeIDs = new List<int>();

    public TerrainStructure(List<BiomeSettings> availableBiomes, BiomeDistribution biomeDistribution)
    {
        //Add all points to the graph
        foreach (var biome in DistributeBiomes(availableBiomes, biomeDistribution))
        {
            _biomeIDs.Add(_biomes.AddNode(biome));
        }

        // Calculate connectivity between biome
        List<Triangle> triangles = DelaunayTriangulation(biomeDistribution.MapResolution);

        // Add biome connectivity to graph
        foreach (var triangle in triangles)
        {
            _biomes.AddEdge(triangle.P0, triangle.P1, 1);
            _biomes.AddEdge(triangle.P0, triangle.P2, 1);
            _biomes.AddEdge(triangle.P1, triangle.P2, 1);
        }
    }

    /* 
     * Distribute points using Grid Jitter technique
     * - Number of points is cellsX * cellsY
     * - Grid origin is assumed to be at world 0,0,0 and increases in +X and +Z
     */
    private List<Biome> DistributeBiomes(List<BiomeSettings> availableBiomes, BiomeDistribution biomeDistribution)
    {
        var result = new Biome[biomeDistribution.XCells * biomeDistribution.YCells];
        var cellSize = new Vector2((float)biomeDistribution.MapResolution / biomeDistribution.XCells, (float)biomeDistribution.MapResolution / biomeDistribution.YCells);
        for (int y = 0; y < biomeDistribution.YCells; y++)
        {
            for (int x = 0; x < biomeDistribution.XCells; x++)
            {
                var posX = Random.Range(x * cellSize.x + biomeDistribution.CellOffset, (x + 1) * cellSize.x - biomeDistribution.CellOffset);
                var posY = Random.Range(y * cellSize.y + biomeDistribution.CellOffset, (y + 1) * cellSize.y - biomeDistribution.CellOffset);
                var biomeIndex = Random.Range(0, availableBiomes.Count);
                result[x + y * biomeDistribution.XCells] = new Biome(new Vector2(posX, posY), availableBiomes[biomeIndex]);
            }
        }

        return new List<Biome>(result);
    }

    /* 
     * Calculate Delaunay to determinate biome conectivity using Bowyer-Watson
     */
    private List<Triangle> DelaunayTriangulation(int resolution)
    {
        // Encompassing biomes for Bowyer-Watson
        var tempConditions = new BiomeConditions(1, 1);
        var tempNoise = new BiomeNoise(1, 1, 1, 1);
        var left = _biomes.AddNode(new Biome(new Vector2(-resolution * 0.5f, 0), new BiomeSettings(tempConditions, tempNoise, 0)));
        var right = _biomes.AddNode(new Biome(new Vector2(resolution * 1.5f, 0), new BiomeSettings(tempConditions, tempNoise, 0)));
        var top = _biomes.AddNode(new Biome(new Vector2(resolution * 0.5f, resolution * 2f), new BiomeSettings(tempConditions, tempNoise, 0)));
        var superTriangle = new Triangle(left, right, top);

        // Add super triangle
        HashSet<Triangle> result = new HashSet<Triangle> { superTriangle };
        HashSet<Triangle> badTriangles = new HashSet<Triangle>();
        List<TriangleEdge> polygon = new List<TriangleEdge>();

        // Bowyer-Watson - iterate through all points
        List<Triangle> tempResult;
        foreach (var biomeID in _biomeIDs)
        {
            // Skip super triangle vertices
            if (biomeID == left || biomeID == right || biomeID == top)
                continue;
            
            Vector2 point = _biomes.GetNodeData(biomeID).Center;
            badTriangles.Clear();
            polygon.Clear();
            tempResult = new List<Triangle>(result);

            // Check every triangle
            foreach (var triangle in tempResult)
            {
                // Add bad triangles
                if (IsInCircumcircle(point, triangle))
                {
                    badTriangles.Add(triangle);
                }
            }

            // Calculate polygon hole
            foreach (var triangle in badTriangles)
            {
                foreach (var edge in triangle.GetEdges())
                {
                    var sharedEdge = false;
                    foreach (var other in badTriangles)
                    {
                        if (!other.Equals(triangle) && other.GetEdges().Contains(edge))
                            sharedEdge = true;
                    }
                    if(!sharedEdge)
                        polygon.Add(edge);
                }
            }

            // Remove bad triangles
            foreach (var triangle in badTriangles)
            {
                result.Remove(triangle);
            }

            // Add new triangles connecting to the new point
            foreach (var edge in polygon)
            {
                var triangle1 = new Triangle(edge.From, edge.To, biomeID);
                result.Add(triangle1);
            }
        }

        // Remove super triangle
        _biomes.RemoveNode(left);
        _biomes.RemoveNode(right);
        _biomes.RemoveNode(top);
        tempResult = new List<Triangle>(result);
        foreach (var triangle in tempResult)
            if (triangle.Contains(left) || triangle.Contains(right) || triangle.Contains(top))
                result.Remove(triangle);

        return result.ToList();
    }

    /*
     * http://mathworld.wolfram.com/Circumcircle.html
     */
    private bool IsInCircumcircle(Vector2 q, Triangle triangle)
    {
        var p0 = _biomes.GetNodeData(triangle.P0).Center;
        var p1 = _biomes.GetNodeData(triangle.P1).Center;
        var p2 = _biomes.GetNodeData(triangle.P2).Center;

        var MatA = new Matrix(3, 3);
        MatA[0, 0] = p0.x; MatA[0, 1] = p0.y; MatA[0, 2] = 1;
        MatA[1, 0] = p1.x; MatA[1, 1] = p1.y; MatA[1, 2] = 1;
        MatA[2, 0] = p2.x; MatA[2, 1] = p2.y; MatA[2, 2] = 1;
        var a = MatA.Det();

        var MatBx = new Matrix(3, 3);
        MatBx[0, 0] = p0.x * p0.x + p0.y * p0.y; MatBx[0, 1] = p0.y; MatBx[0, 2] = 1;
        MatBx[1, 0] = p1.x * p1.x + p1.y * p1.y; MatBx[1, 1] = p1.y; MatBx[1, 2] = 1;
        MatBx[2, 0] = p2.x * p2.x + p2.y * p2.y; MatBx[2, 1] = p2.y; MatBx[2, 2] = 1;
        var bx = -MatBx.Det();

        var MatBy = new Matrix(3, 3);
        MatBy[0, 0] = p0.x * p0.x + p0.y * p0.y; MatBy[0, 1] = p0.x; MatBy[0, 2] = 1;
        MatBy[1, 0] = p1.x * p1.x + p1.y * p1.y; MatBy[1, 1] = p1.x; MatBy[1, 2] = 1;
        MatBy[2, 0] = p2.x * p2.x + p2.y * p2.y; MatBy[2, 1] = p2.x; MatBy[2, 2] = 1;
        var by = MatBy.Det();

        var MatC = new Matrix(3, 3);
        MatC[0, 0] = p0.x * p0.x + p0.y * p0.y; MatC[0, 1] = p0.x; MatC[0, 2] = p0.y;
        MatC[1, 0] = p1.x * p1.x + p1.y * p1.y; MatC[1, 1] = p1.x; MatC[1, 2] = p1.y;
        MatC[2, 0] = p2.x * p2.x + p2.y * p2.y; MatC[2, 1] = p2.x; MatC[2, 2] = p2.y;
        var c = MatC.Det();

        //var equation = a * (Math.Pow(bx / (2*a), 2) + Math.Pow(by / (2 * a), 2)) - (bx * bx) / (4 * a) - (by * by) / (4 * a) + c;

        var center = new Vector2(-(float)(bx/(2*a)), -(float)(by/(2*a)));
        var radius = Math.Sqrt(bx * bx + by * by + 4 * a * c) / (2 * Mathf.Abs((float) a));

        return (q - center).sqrMagnitude <= radius * radius;
    }

    /* Sample Biome Data from a given position */
    public BiomeConditions SampleBiomeData(Vector2 position)
    {
        var triangle = FindTriangle(position);
        if (triangle.P0 == -1)
            return new BiomeConditions(0, 0);

        
        var node0 = _biomes.GetNodeData(triangle.P0);
        var node1 = _biomes.GetNodeData(triangle.P1);
        var node2 = _biomes.GetNodeData(triangle.P2);

        return BiomeSettings.BarInterpConditions(position, node0, node1, node2);
    }

    public BiomeNoise SampleBiomeNoise(Vector2 position)
    {
        var triangle = FindTriangle(position);
        if (triangle.P0 == -1)
            return new BiomeNoise(0, 0, 0, 0);

        var node0 = _biomes.GetNodeData(triangle.P0);
        var node1 = _biomes.GetNodeData(triangle.P1);
        var node2 = _biomes.GetNodeData(triangle.P2);

        return BiomeSettings.BarInterpNoise(position, node0, node1, node2);   
    }

    /* Find triangle that contains point */
    private Triangle FindTriangle(Vector2 point)
    {
        var sortedDistances = new SortedList<float, int>();

        /* Find first closest point */
        foreach (var biome in _biomeIDs)
        {
            float dist = (_biomes.GetNodeData(biome).Center - point).SqrMagnitude();
            if (sortedDistances.ContainsKey(dist))
                continue;
            sortedDistances.Add(dist, biome);
        }

        var p0 = sortedDistances.Values[0];
        var triangles = new List<Triangle>();
        var neighbours = _biomes.GetNeighbours(p0);

        /* Find the triangles of p0 */
        for (var i = 0; i < neighbours.Length; i++)
        {
            triangles.Add(new Triangle(p0, neighbours[i], neighbours[(i + i) % neighbours.Length]));
        }

        Triangle match = new Triangle(-1, -1, -1);
        foreach (var triangle in triangles)
        {
            if (PointInTriangle(point, triangle))
            {
                match = triangle;
                break;
            }
        }

        return match;
    }

    private bool PointInTriangle(Vector2 p, Triangle triangle)
    {
        var a = _biomes.GetNodeData(triangle.P0).Center;
        var b = _biomes.GetNodeData(triangle.P1).Center;
        var c = _biomes.GetNodeData(triangle.P2).Center;

        var bar = p.Barycentric(a, b, c);
        
        return (bar.x >= 0) && (bar.y >= 0) && (bar.x + bar.y < 1);
    }

    public GameObject DrawBiomeGraph()
    {
        var graphObj = new GameObject("GraphInstance");
        foreach (var biome in _biomeIDs)
        {
            var pos = _biomes.GetNodeData(biome).Center;
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "Biome id: " + biome;
            go.GetComponent<Collider>().enabled = false;
            go.transform.parent = graphObj.transform;
            go.transform.position = new Vector3(pos.x, 0, pos.y);
            go.transform.localScale = Vector3.one * 20;
        }
        
        foreach (var edge in _biomes.GetAllEdges())
        {
            var start = new Vector3(_biomes.GetNodeData(edge.x).Center.x, 0 , _biomes.GetNodeData(edge.x).Center.y);
            var end = new Vector3(_biomes.GetNodeData(edge.y).Center.x, 0, _biomes.GetNodeData(edge.y).Center.y);
            GameObject myLine = new GameObject(edge.x + " " + edge.y);
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
            List<int> sorted = new List<int>{p0,p1,p2};
            sorted.Sort();
            P0 = sorted[0];
            P1 = sorted[1];
            P2 = sorted[2];
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

        public override int GetHashCode()
        {
            return unchecked(P0 + (31 * P1) + (31 * 31 * P2)); ;
        }

        public override string ToString()
        {
            return P0 + " " + P1 + " " + P2;
        }

        public TriangleEdge[] GetEdges()
        {
            var result = new TriangleEdge[3];
            result[0] = new TriangleEdge(P0, P1);
            result[1] = new TriangleEdge(P1, P2);
            result[2] = new TriangleEdge(P2, P0);
            return result;
        }
    }

    private struct TriangleEdge
    {
        public readonly int From, To;

        public TriangleEdge(int from, int to) : this()
        {
            From = from;
            To = to;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var otherEdge = (TriangleEdge)obj;
            return From == otherEdge.From && To == otherEdge.To
                   || From == otherEdge.To && To == otherEdge.From;
        }

        public override int GetHashCode()
        {
            return From + To * 31;
        }
    }
}