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

    public static GameObject DrawGraph(Graph<Biome> graph, string name)
    {
        GameObject result = new GameObject(name);

        // Draw all lines
        GameObject edges = new GameObject("Edges");
        edges.transform.parent = result.transform;
        foreach (var e in graph.GetAllEdges())
        {
            Vector2 start = graph.GetNodeData(e.x).Center;
            Vector2 end = graph.GetNodeData(e.y).Center;

            GameObject edge = DrawLine(new Vector3(start.x, 0, start.y), new Vector3(end.x, 0, end.y), 5, Color.gray);
            edge.transform.parent = edges.transform;
        }

        // Draw all centers
        GameObject nodes = new GameObject("Biomes");
        nodes.transform.parent = result.transform;
        foreach (var e in graph.GetAllNodeData())
        {
            GameObject node = DrawSphere(new Vector3(e.Center.x, 0, e.Center.y), 20,
                e.IsBorderBiome ? Color.black : Color.white);
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
