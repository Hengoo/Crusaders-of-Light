using System;
using System.Collections.Generic;
using System.Linq;
using csDelaunay;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;


public class TerrainStructure
{
    private readonly Voronoi _voronoiDiagram;
    private readonly Graph<Biome> _biomeGraph = new Graph<Biome>();
    private readonly BiomeConfiguration _biomeConfiguration;
    private readonly WorldStructureTemp _worldStructure;
    private KeyValuePair<Vector2f, int> _startBiome;

    //Mapping of Voronoi library sites and graph IDs
    private readonly Dictionary<Vector2f, int> _siteBiomeDictionary = new Dictionary<Vector2f, int>();

    public TerrainStructure(List<BiomeSettings> availableBiomes, BiomeConfiguration biomeConfiguration)
    {
        _biomeConfiguration = biomeConfiguration;
        var navigableBiomeIDs = new HashSet<int>();
        var centers = new List<Vector2f>();

        for (int i = 0; i < biomeConfiguration.BiomeSamples; i++)
        {
            var x = Random.Range(0f, biomeConfiguration.MapSize);
            var y = Random.Range(0f, biomeConfiguration.MapSize);
            centers.Add(new Vector2f(x, y));
        }
        _voronoiDiagram = new Voronoi(centers,
            new Rectf(0, 0, biomeConfiguration.MapSize, biomeConfiguration.MapSize));
        _voronoiDiagram.LloydRelaxation(biomeConfiguration.LloydRelaxation);

        /* Assign each site to a biome */
        foreach (var site in _voronoiDiagram.SiteCoords())
        {
            bool isOnBorder = false;
            var center = new Vector2(site.x, site.y);
            var segments = _voronoiDiagram.VoronoiBoundarayForSite(site);

            foreach (var segment in segments)
            {
                if (segment.p0.x <= _voronoiDiagram.PlotBounds.left || segment.p0.x >= _voronoiDiagram.PlotBounds.right
                    || segment.p0.y <= _voronoiDiagram.PlotBounds.top || segment.p0.y >= _voronoiDiagram.PlotBounds.bottom
                    || segment.p1.x <= _voronoiDiagram.PlotBounds.left || segment.p1.x >= _voronoiDiagram.PlotBounds.right
                    || segment.p1.y <= _voronoiDiagram.PlotBounds.top ||
                    segment.p1.y >= _voronoiDiagram.PlotBounds.bottom)
                {
                    isOnBorder = true;
                    break;
                }
            }

            /* Assign biome to site - water if on border */
            var biome = isOnBorder
                ? new Biome(center, _biomeConfiguration.BorderBiome)
                : new Biome(center, availableBiomes[Random.Range(0, availableBiomes.Count)]);

            var biomeID = _biomeGraph.AddNode(biome);
            _siteBiomeDictionary.Add(site, biomeID);
            if (!biome.BiomeSettings.NotNavigable)
                navigableBiomeIDs.Add(biomeID);
        }

        /* MSP */
        foreach (var edge in GeneratePaths())
        {
            _biomeGraph.AddEdge(edge.Value, edge.Key, 1);
        }

        return;

        /* Create navigation graph - for each biome, add reachable neighbors */
        foreach (var id in _siteBiomeDictionary)
        {
            var biome = _biomeGraph.GetNodeData(id.Value);
            if (biome.BiomeSettings.NotNavigable) continue;

            foreach (var neighbor in _voronoiDiagram.NeighborSitesForSite(new Vector2f(biome.Center.x, biome.Center.y)))
            {
                var neighborBiome = _biomeGraph.GetNodeData(_siteBiomeDictionary[neighbor]);
                if (!neighborBiome.BiomeSettings.NotNavigable)
                {
                    _biomeGraph.AddEdge(_siteBiomeDictionary[neighbor], id.Value, 1);
                }
            }
        }
    }

    public IEnumerable<LineSegment> GetBiomeSmoothBorders()
    {
        var result = new List<LineSegment>();

        foreach (var edge in _voronoiDiagram.Edges)
        {
            if (!edge.Visible())
                continue;

            var leftBiome = _biomeGraph.GetNodeData(_siteBiomeDictionary[edge.LeftSite.Coord]);
            var rightBiome = _biomeGraph.GetNodeData(_siteBiomeDictionary[edge.RightSite.Coord]);
            if (leftBiome.BiomeSettings.UniqueName == rightBiome.BiomeSettings.UniqueName
                || leftBiome.BiomeSettings.DontBlendWith.Contains(rightBiome.BiomeSettings)
                || rightBiome.BiomeSettings.DontBlendWith.Contains(leftBiome.BiomeSettings))

                continue;

            var p0 = edge.ClippedEnds[LR.LEFT];
            var p1 = edge.ClippedEnds[LR.RIGHT];
            var segment = new LineSegment(p0, p1);
            result.Add(segment);
        }
        //DrawLineSegments(result, 1, new GameObject("Blended Borders").transform);

        return result;
    }

    public IEnumerable<LineSegment> GetBiomeBorders()
    {
        var result = new List<LineSegment>();

        foreach (var edge in _voronoiDiagram.Edges)
        {
            if (!edge.Visible())
                continue;

            var leftBiome = _biomeGraph.GetNodeData(_siteBiomeDictionary[edge.LeftSite.Coord]);
            var rightBiome = _biomeGraph.GetNodeData(_siteBiomeDictionary[edge.RightSite.Coord]);
            if (leftBiome.BiomeSettings.UniqueName == rightBiome.BiomeSettings.UniqueName)
                continue;

            var p0 = edge.ClippedEnds[LR.LEFT];
            var p1 = edge.ClippedEnds[LR.RIGHT];
            var segment = new LineSegment(p0, p1);
            result.Add(segment);
        }
        //DrawLineSegments(result, 1, new GameObject("All Borders").transform);

        return result;
    }

    public BiomeHeight SampleBiomeHeight(Vector2 position)
    {
        Biome closestBiome = null;
        var closestSqrDistance = float.MaxValue;
        var pos = new Vector2f(position.x + Random.Range(-_biomeConfiguration.BorderNoise, _biomeConfiguration.BorderNoise),
            position.y + Random.Range(-_biomeConfiguration.BorderNoise, _biomeConfiguration.BorderNoise));

        foreach (var biome in _siteBiomeDictionary)
        {
            var currentBiome = _biomeGraph.GetNodeData(biome.Value);
            var center = new Vector2f(currentBiome.Center.x, currentBiome.Center.y);
            var sqrDistance = center.DistanceSquare(pos);
            if (sqrDistance < closestSqrDistance)
            {
                closestBiome = _biomeGraph.GetNodeData(biome.Value);
                closestSqrDistance = sqrDistance;
            }
        }

        return closestBiome == null ? _biomeConfiguration.BorderBiome.BiomeHeight : closestBiome.BiomeSettings.BiomeHeight;
    }

    public GameObject DrawBiomeGraph(float scale)
    {
        var result = new GameObject();

        var biomes = new GameObject("Biomes");
        biomes.transform.parent = result.transform;
        var voronoi = new GameObject("Voronoi");
        voronoi.transform.parent = result.transform;
        var delaunay = new GameObject("Modified Delaunay");
        delaunay.transform.parent = result.transform;

        foreach (var biome in _siteBiomeDictionary)
        {
            var pos = new Vector2(biome.Key.x, biome.Key.y);
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "Biome id: " + biome.Value;
            go.GetComponent<Collider>().enabled = false;
            go.transform.parent = biomes.transform;
            go.transform.position = new Vector3(pos.x, 0, pos.y);
            go.transform.localScale = Vector3.one * 20 * scale;
            if (biome.Value == _startBiome.Value)
            {
                var renderer = go.GetComponent<Renderer>();
                var tempMaterial = new Material(renderer.sharedMaterial) {color = Color.red};
                renderer.sharedMaterial = tempMaterial;
            }
        }

        DrawLineSegments(_voronoiDiagram.VoronoiDiagram(), scale, voronoi.transform);

        foreach (var edge in _biomeGraph.GetAllEdges())
        {
            var biome1 = _biomeGraph.GetNodeData(edge.x);
            var biome2 = _biomeGraph.GetNodeData(edge.y);

            var start = new Vector3(biome1.Center.x, 0, biome1.Center.y);
            var end = new Vector3(biome2.Center.x, 0, biome2.Center.y);
            GameObject myLine = new GameObject("Line");
            myLine.transform.position = start;
            myLine.transform.parent = delaunay.transform;
            LineRenderer lr = myLine.AddComponent<LineRenderer>();
            lr.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
            lr.startColor = Color.white;
            lr.endColor = Color.white;
            lr.startWidth = 2 * scale;
            lr.endWidth = 2 * scale;
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);
        }

        return result;
    }

    private void DrawLineSegments(IEnumerable<LineSegment> lines, float scale, Transform parent)
    {
        foreach (var line in lines)
        {
            var start = new Vector3(line.p0.x, 0, line.p0.y);
            var end = new Vector3(line.p1.x, 0, line.p1.y);
            GameObject myLine = new GameObject("Line");
            myLine.transform.position = start;
            myLine.transform.parent = parent;
            LineRenderer lr = myLine.AddComponent<LineRenderer>();
            lr.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
            lr.startColor = Color.white;
            lr.endColor = Color.white;
            lr.startWidth = 2 * scale;
            lr.endWidth = 2 * scale;
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);
        }
    }

    /* Generate paths between existing biomes */
    private List<KeyValuePair<int, int>> GeneratePaths()
    {
        List<KeyValuePair<int, int>> result;
        var navigableBiomes = new Dictionary<Vector2f, int>();
        var randomBiomeList = new List<KeyValuePair<Vector2f, int>>();
        foreach (var pair in _siteBiomeDictionary)
        {
            if (!_biomeGraph.GetNodeData(pair.Value).BiomeSettings.NotNavigable)
            {
                navigableBiomes.Add(pair.Key, pair.Value);
                randomBiomeList.Add(pair);
            }
        }
        randomBiomeList.Shuffle();
        Debug.Log(randomBiomeList.First().Value + " out of " + randomBiomeList.Count + " Numbers ");
        _startBiome = randomBiomeList.First();

        result = PrimMSP(_startBiome, navigableBiomes);

        return result;
    }

    /* Create a Minimum Spanning Tree using Prim's algorithm */
    private static List<KeyValuePair<int, int>> PrimMSP(KeyValuePair<Vector2f, int> startNode, IDictionary<Vector2f, int> nodes)
    {
        var result = new List<KeyValuePair<int, int>>();
        var tree = new List<KeyValuePair<Vector2f, int>>();
        nodes.Remove(startNode.Key);
        tree.Add(startNode);

        //Iterate until all nodes all connected to the tree
        while (nodes.Count > 0)
        {
            var current = new KeyValuePair<Vector2f, int>();
            var closest = new KeyValuePair<Vector2f, int>();
            float closestSqrDistance = float.MaxValue;

            //Find the closest node pair, where one node is in the tree and the other isn't
            foreach (var node in tree)
            {
                foreach (var outNode in nodes)
                {
                    var currentDistance = node.Key.DistanceSquare(outNode.Key);
                    if (currentDistance < closestSqrDistance)
                    {
                        closest = node;
                        current = outNode;
                        closestSqrDistance = currentDistance;
                    }
                }
            }

            nodes.Remove(current.Key);
            tree.Add(current);
            result.Add(new KeyValuePair<int, int>(current.Value, closest.Value));
        }

        return result;
    }
}