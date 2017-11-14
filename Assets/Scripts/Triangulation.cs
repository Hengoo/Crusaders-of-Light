using System.Collections.Generic;
using NUnit.Framework.Constraints;
using TriangleNet;
using UnityEngine;
using TriangleNet.Geometry;
using TriangleNet.Voronoi;
using TriangleNet.Tools;
using Mesh = TriangleNet.Mesh;

public class Triangulation
{
    public static bool Triangulate(List<Vector2> points, List<List<Vector2>> holes, out List<int> outIndices,
        out List<Vector3> outVertices)
    {
        outVertices = new List<Vector3>();
        outIndices = new List<int>();
        Polygon poly = new Polygon();

        //Polygon and segments
        for (int i = 0; i < points.Count; i++)
        {
            poly.Add(new Vertex(points[i].x, points[i].y));
            poly.Add(i == points.Count - 1
                ? new Segment(new Vertex(points[i].x, points[i].y), new Vertex(points[0].x, points[0].y))
                : new Segment(new Vertex(points[i].x, points[i].y), new Vertex(points[i + 1].x, points[i + 1].y)));
        }

        // Holes
        for (int i = 0; i < holes.Count; i++)
        {
            List<Vertex> vertices = new List<Vertex>();
            for (int j = 0; j < holes[i].Count; j++)
            {
                vertices.Add(new Vertex(holes[i][j].x, holes[i][j].y));
            }

            poly.Add(new Contour(vertices), true);
        }

        var mesh = poly.Triangulate();

        foreach (var t in mesh.Triangles)
        {
            for (int j = 2; j >= 0; j--)
            {
                bool found = false;
                for (var k = 0; k < outVertices.Count; k++)
                {
                    if ((outVertices[k].x == t.GetVertex(j).X) && (outVertices[k].z == t.GetVertex(j).Y))
                    {
                        outIndices.Add(k);
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    outVertices.Add(new Vector3((float)t.GetVertex(j).X, 0, (float)t.GetVertex(j).Y));
                    outIndices.Add(outVertices.Count - 1);
                }
            }
        }

        return true;
    }

    public static bool Triangulate(List<Vector2> points, out List<int> outIndices, out List<Vector3> outVertices)
    {
        return Triangulate(points, new List<List<Vector2>>(), out outIndices, out outVertices);
    }
}
