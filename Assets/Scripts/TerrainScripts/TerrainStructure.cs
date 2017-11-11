using System;
using System.Collections.Generic;
using System.Linq;
using csDelaunay;
using UnityEngine;
using Random = UnityEngine.Random;


public class TerrainStructure
{
    public Voronoi VoronoiDiagram;

    private readonly Graph<Biome> _biomes = new Graph<Biome>();
    private readonly List<int> _biomeIDs = new List<int>();

    private static BiomeSettings Water = new BiomeSettings(new BiomeConditions(1,1), new BiomeHeight(0,0,0,0), 1);

    public TerrainStructure(List<BiomeSettings> availableBiomes, BiomeDistribution biomeDistribution)
    {
        var centers = new List<Vector2f>();
        for(int i = 0; i < biomeDistribution.BiomeSamples; i++)
        {
            var x = Random.Range(0f, biomeDistribution.MapResolution);
            var y = Random.Range(0f, biomeDistribution.MapResolution);
            centers.Add(new Vector2f(x, y));
        }
        VoronoiDiagram = new Voronoi(centers, new Rectf(0, 0, biomeDistribution.MapResolution, biomeDistribution.MapResolution));
        VoronoiDiagram.LloydRelaxation(biomeDistribution.LloydRelaxation);

        /* Assign each site to a biome */

        Debug.Log(VoronoiDiagram.PlotBounds.bottomRight + " " + VoronoiDiagram.PlotBounds.topLeft);
        foreach (var site in VoronoiDiagram.SiteCoords())
        {
            Biome biome;
            bool isOnBorder = false;
            var center = new Vector2(site.x, site.y);
            var segments = VoronoiDiagram.VoronoiBoundarayForSite(site);

            foreach (var segment in segments)
            {
                if (segment.p0.x <= VoronoiDiagram.PlotBounds.left || segment.p0.x >= VoronoiDiagram.PlotBounds.right
                    || segment.p0.y <= VoronoiDiagram.PlotBounds.top || segment.p0.y >= VoronoiDiagram.PlotBounds.bottom
                    || segment.p1.x <= VoronoiDiagram.PlotBounds.left || segment.p1.x >= VoronoiDiagram.PlotBounds.right
                    || segment.p1.y <= VoronoiDiagram.PlotBounds.top || segment.p1.y >= VoronoiDiagram.PlotBounds.bottom)
                {
                    isOnBorder = true;
                    break;
                }
            }

            /* Assign biome to site - water if on border */
            biome = isOnBorder ? new Biome(center, Water, true) : new Biome(center, availableBiomes[Random.Range(0, availableBiomes.Count)], false);
            Debug.Log(isOnBorder ? "Water" : "NOT WATER");

            _biomeIDs.Add(_biomes.AddNode(biome));
        }
    }

    public float SampleBiomeHeight(Vector2 position)
    {
        Biome closestBiome = null;
        var closestSqrDistance = float.MaxValue;
        var pos = new Vector2f(position.x, position.y);

        foreach (var biome in _biomeIDs)
        {
            var currentBiome = _biomes.GetNodeData(biome);
            var center = new Vector2f(currentBiome.Center.x, currentBiome.Center.y);
            var sqrDistance = center.DistanceSquare(pos);
            if (sqrDistance < closestSqrDistance)
            {
                closestBiome = _biomes.GetNodeData(biome);
                closestSqrDistance = sqrDistance;
            }
        }

        return closestBiome != null && closestBiome.IsWater ? 0 : .5f;
    }

    public GameObject DrawBiomeGraph(float scale)
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
            go.transform.localScale = Vector3.one * 20 * scale;
        }

        foreach (var edge in VoronoiDiagram.VoronoiDiagram())
        {
            var start = new Vector3(edge.p0.x, 0, edge.p0.y);
            var end = new Vector3(edge.p1.x, 0, edge.p1.y);
            GameObject myLine = new GameObject("Line");
            myLine.transform.position = start;
            myLine.transform.parent = graphObj.transform;
            LineRenderer lr = myLine.AddComponent<LineRenderer>();
            lr.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
            lr.startColor = Color.white;
            lr.endColor = Color.white;
            lr.startWidth = 2 * scale;
            lr.endWidth = 2 * scale;
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);
        }

        return graphObj;
    }

}