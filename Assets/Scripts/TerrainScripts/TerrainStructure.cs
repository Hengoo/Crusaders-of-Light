using System;
using System.Collections.Generic;
using System.Linq;
using csDelaunay;
using UnityEngine;
using Random = UnityEngine.Random;


public class TerrainStructure
{
    public Voronoi VoronoiDiagram;

    private readonly Graph<Biome> _biomeGraph = new Graph<Biome>();

    //Mapping of Voronoi library sites and graph IDs
    private readonly Dictionary<Vector2f, int> _biomeIDs = new Dictionary<Vector2f, int>();

    private readonly BiomeSettings _water;

    public TerrainStructure(List<BiomeSettings> availableBiomes, BiomeDistribution biomeDistribution)
    {
        _water = new BiomeSettings(new BiomeConditions(1, 1),
            new BiomeHeight(0.5f, 0.5f, biomeDistribution.SeaHeight, 0, 20), true);
        var centers = new List<Vector2f>();
        for (int i = 0; i < biomeDistribution.BiomeSamples; i++)
        {
            var x = Random.Range(0f, biomeDistribution.MapResolution);
            var y = Random.Range(0f, biomeDistribution.MapResolution);
            centers.Add(new Vector2f(x, y));
        }
        VoronoiDiagram = new Voronoi(centers,
            new Rectf(0, 0, biomeDistribution.MapResolution, biomeDistribution.MapResolution));
        VoronoiDiagram.LloydRelaxation(biomeDistribution.LloydRelaxation);

        /* Assign each site to a biome */
        foreach (var site in VoronoiDiagram.SiteCoords())
        {
            bool isOnBorder = false;
            var center = new Vector2(site.x, site.y);
            var segments = VoronoiDiagram.VoronoiBoundarayForSite(site);

            foreach (var segment in segments)
            {
                if (segment.p0.x <= VoronoiDiagram.PlotBounds.left || segment.p0.x >= VoronoiDiagram.PlotBounds.right
                    || segment.p0.y <= VoronoiDiagram.PlotBounds.top || segment.p0.y >= VoronoiDiagram.PlotBounds.bottom
                    || segment.p1.x <= VoronoiDiagram.PlotBounds.left || segment.p1.x >= VoronoiDiagram.PlotBounds.right
                    || segment.p1.y <= VoronoiDiagram.PlotBounds.top ||
                    segment.p1.y >= VoronoiDiagram.PlotBounds.bottom)
                {
                    isOnBorder = true;
                    break;
                }
            }

            /* Assign biome to site - water if on border */
            var biome = isOnBorder
                ? new Biome(center, _water)
                : new Biome(center, availableBiomes[Random.Range(0, availableBiomes.Count)]);
            

            _biomeIDs.Add(site, _biomeGraph.AddNode(biome));
        }

        /* Create navigation graph - for each biome, add reachable neighbors */
        foreach (var id in _biomeIDs)
        {
            var biome = _biomeGraph.GetNodeData(id.Value);
            if (biome.BiomeSettings.NotNavigable) continue;

            foreach (var neighbor in VoronoiDiagram.NeighborSitesForSite(new Vector2f(biome.Center.x, biome.Center.y)))
            {
                var neighborBiome = _biomeGraph.GetNodeData(_biomeIDs[neighbor]);
                if (!neighborBiome.BiomeSettings.NotNavigable)
                {
                    _biomeGraph.AddEdge(_biomeIDs[neighbor], id.Value, 1);
                }
            }
        }
    }

    public IEnumerable<Edge> GetBiomeEdges()
    {
        var result = new List<Edge>();
        foreach (var site in _biomeIDs.Keys)
        {
            
        }
        foreach (var edge in VoronoiDiagram.Edges)
        {
            result.Add(edge);
        }
        return result;
    }

    public BiomeHeight SampleBiomeHeight(Vector2 position)
    {
        Biome closestBiome = null;
        var closestSqrDistance = float.MaxValue;
        var pos = new Vector2f(position.x, position.y);

        foreach (var biome in _biomeIDs)
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

        return closestBiome == null ? _water.BiomeHeight : closestBiome.BiomeSettings.BiomeHeight;
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

        foreach (var biome in _biomeIDs)
        {
            var pos = new Vector2(biome.Key.x, biome.Key.y);
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "Biome id: " + biome.Value;
            go.GetComponent<Collider>().enabled = false;
            go.transform.parent = biomes.transform;
            go.transform.position = new Vector3(pos.x, 0, pos.y);
            go.transform.localScale = Vector3.one * 20 * scale;
        }

        
        foreach (var edge in VoronoiDiagram.VoronoiDiagram())
        {
            var start = new Vector3(edge.p0.x, 0, edge.p0.y);
            var end = new Vector3(edge.p1.x, 0, edge.p1.y);
            GameObject myLine = new GameObject("Line");
            myLine.transform.position = start;
            myLine.transform.parent = voronoi.transform;
            LineRenderer lr = myLine.AddComponent<LineRenderer>();
            lr.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
            lr.startColor = Color.white;
            lr.endColor = Color.white;
            lr.startWidth = 2 * scale;
            lr.endWidth = 2 * scale;
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);
        }

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

}