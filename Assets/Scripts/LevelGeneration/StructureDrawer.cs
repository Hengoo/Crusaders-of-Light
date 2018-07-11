using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using csDelaunay;
using UnityEngine;

public static class StructureDrawer
{

    public static GameObject DrawAreas(IEnumerable<AreaSettings> areaSettingsCollection, string name = "Areas")
    {
        GameObject areas = new GameObject(name);

        foreach (var area in areaSettingsCollection)
        {
            GameObject areaGO = DrawAreaSettings(area);
            areaGO.transform.parent = areas.transform;
        }

        return areas;
    }

    public static GameObject DrawAreaSettings(AreaSettings areaSettings)
    {
        GameObject result = new GameObject(areaSettings.Name);

        var border = DrawPolygon(areaSettings.BorderPolygon, Color.yellow, "Border");
        border.transform.parent = result.transform;

        foreach (var clearPolygon in areaSettings.ClearPolygons)
        {
            var clearPolygonGO = DrawPolygon(clearPolygon, Color.red, "Clear");
            clearPolygonGO.transform.parent = result.transform;
        }

        foreach (var areaData in areaSettings.AreaDataGraph.GetAllNodeData())
        {
            var segment = DrawPolygon(areaData.Polygon, Color.blue, areaData.Segment.Type + " Segment");
            segment.transform.parent = result.transform;
            segment.SetActive(false);

            var center = DrawSphere(new Vector3(areaData.Center.x, 0, areaData.Center.y), 2, Color.blue, areaData.Segment.Type + " Center");
            center.transform.parent = result.transform;
        }


        return result;
    }

    public static GameObject DrawVoronoiDiagram(Voronoi voronoi, string name = "VoronoiDiagram")
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

    public static GameObject DrawAreaSegments(TerrainStructure terrainStructure, string name = "AreaSegments")
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

            switch ((AreaSegment.EAreaSegmentEdgeType)graph.GetEdgeValue(edge.x, edge.y))
            {
                case AreaSegment.EAreaSegmentEdgeType.NonNavigable:
                    color = Color.black;
                    break;
                case AreaSegment.EAreaSegmentEdgeType.MainPath:
                    color = Color.green;
                    break;
                case AreaSegment.EAreaSegmentEdgeType.SidePath:
                    color = Color.cyan;
                    break;
                case AreaSegment.EAreaSegmentEdgeType.BossInnerPath:
                    color = Color.red;
                    break;
                case AreaSegment.EAreaSegmentEdgeType.SpecialInnerPath:
                    color = Color.yellow;
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

    public static GameObject DrawMultipleLines(IEnumerable<Vector2[]> lines, Color color, string name = "Lines")
    {
        GameObject result = new GameObject(name);

        foreach (var line in lines)
        {
            var p0 = new Vector3(line[0].x, 0, line[0].y);
            var p1 = new Vector3(line[1].x, 0, line[1].y);

            var go = DrawLine(p0, p1, 3, color);
            go.transform.parent = result.transform;
        }

        return result;
    }

    public static GameObject DrawPolygon(IEnumerable<Vector2> polygon, Color color, string name = "Polygon")
    {
        GameObject result = new GameObject(name);
        for (int i = 0; i < polygon.Count(); i++)
        {
            var p0 = polygon.ElementAt(i);
            var p1 = polygon.ElementAt(i == polygon.Count() - 1 ? 0 : i + 1);
            var start = new Vector3(p0.x, 0, p0.y);
            var end = new Vector3(p1.x, 0, p1.y);

            GameObject line = DrawLine(start, end, 3, color);
            line.transform.parent = result.transform;
        }

        return result;
    }

    public static GameObject DrawSphere(Vector3 center, float radius, Color color, string name = "Sphere")
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = name;
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

    public static GameObject DrawLine(Vector2 start, Vector2 end, float width, Color color)
    {
        GameObject line = new GameObject("Line");
        line.transform.position = start;

        LineRenderer lr = line.AddComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));

        lr.SetPosition(0, new Vector3(start.x, 0, start.y));
        lr.startColor = color;
        lr.startWidth = width;

        lr.SetPosition(1, new Vector3(end.x, 0, end.y));
        lr.endColor = color;
        lr.endWidth = width;

        return line;
    }
}
