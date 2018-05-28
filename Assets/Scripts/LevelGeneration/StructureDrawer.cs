using System;
using System.Collections;
using System.Collections.Generic;
using csDelaunay;
using UnityEngine;

public static class StructureDrawer {

    public static GameObject DrawVoronoiDiagram(Voronoi voronoi, string name)
    {
        GameObject voronoiDiagram = new GameObject(name);
        foreach (var lineSegment in voronoi.VoronoiDiagram())
        {
            Vector2 start = lineSegment.p0.ToUnityVector2();
            Vector2 end = lineSegment.p1.ToUnityVector2();

            GameObject line = DrawLine(new Vector3(start.x, -10, start.y), new Vector3(end.x, -10, end.y), 8, Color.white);
            line.transform.parent = voronoiDiagram.transform;
        }

        return voronoiDiagram;
    }

    public static GameObject DrawAreaSegments(TerrainStructure terrainStructure, string name)
    {
        GameObject result = new GameObject(name);
        var graph = terrainStructure.AreaSegmentGraph;

        // Draw all lines
        GameObject edges = new GameObject("Edges");
        edges.transform.parent = result.transform;
        foreach (var edge in graph.GetAllEdges())
        {
            Vector2 start = terrainStructure.GetAreaSegmentCenter(edge.x);
            Vector2 end = terrainStructure.GetAreaSegmentCenter(edge.y);
            Color color;

            switch (graph.GetEdgeValue(edge.x, edge.y))
            {
                case 0:
                    color = Color.black;
                    break;
                case 1:
                    color = Color.green;
                    break;
                default:
                    color = Color.gray;
                    break;
            }

            GameObject line = DrawLine(new Vector3(start.x, 0, start.y), new Vector3(end.x, 0, end.y), 5, color);
            line.transform.parent = edges.transform;
        }

        // Draw all centers
        GameObject nodes = new GameObject("Nodes");
        nodes.transform.parent = result.transform;
        foreach (var id in graph.GetAllNodeIDs())
        {
            AreaSegment data = graph.GetNodeData(id);
            Vector2 center = terrainStructure.GetAreaSegmentCenter(id);
            Color color;
            switch (data.Type)
            {
                case AreaSegment.EAreaSegmentType.Empty:
                    color = Color.white;
                    break;
                case AreaSegment.EAreaSegmentType.Border:
                    color = Color.black;
                    break;
                case AreaSegment.EAreaSegmentType.Start:
                    color = Color.blue;
                    break;
                case AreaSegment.EAreaSegmentType.Boss:
                    color = Color.red;
                    break;
                case AreaSegment.EAreaSegmentType.Special:
                    color = Color.yellow;
                    break;
                case AreaSegment.EAreaSegmentType.MainPath:
                    color = Color.green;
                    break;
                case AreaSegment.EAreaSegmentType.SidePath:
                    color = Color.cyan;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            GameObject node = DrawSphere(new Vector3(center.x, 0, center.y), 20, color);
            node.name = "Node " + id + " - " + data.Type;
            node.transform.parent = nodes.transform;
        }

        return result;
    }

    public static GameObject DrawSphere(Vector3 center, float radius, Color color)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = "Sphere";
        sphere.transform.position = center;
        sphere.transform.localScale = new Vector3(radius, radius, radius);

        Renderer renderer = sphere.GetComponent<Renderer>();
        renderer.sharedMaterial = new Material(renderer.sharedMaterial) { color = color };

        return sphere;
    }

    public static GameObject DrawLine(Vector3 start, Vector3 end, float width, Color color)
    {
        GameObject line = new GameObject("Line");
        line.transform.position = start;

        LineRenderer lr = line.AddComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));

        lr.SetPosition(0, start);
        lr.startColor = color;
        lr.startWidth = width;

        lr.SetPosition(1, end);
        lr.endColor = color;
        lr.endWidth = width;

        return line;
    }
}
