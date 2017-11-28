using System.Collections;
using System.Linq;
using csDelaunay;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class WorldStructureGraph
{

    Graph<WorldArea> _AreaGraph = new Graph<WorldArea>();

    public void GenerateAreaGraph(Graph<Biome> original, int numAreas)
    {
        var workingGraph = new Graph<Biome>(original);

        //Get graph with only navigable biomes
        List<int> unusableBiomes = new List<int>();
        List<int> usableBiomes = new List<int>();

        for (int i = 0; i < workingGraph.Count(); i++)
        {
            if (workingGraph.GetNodeData(i).BiomeSettings.NotNavigable)
            {
                unusableBiomes.Add(i);
            }
            else
            {
                usableBiomes.Add(i);
            }
        }
        foreach (int i in unusableBiomes)
        {
            workingGraph.RemoveNode(i);
        }

        List<int> areaCenters = new List<int>();
        for (int i = 0; i < numAreas; i++)
        {
            int newCenter = usableBiomes[Random.Range(0, usableBiomes.Count)];
            areaCenters.Add(newCenter);
            usableBiomes.Remove(newCenter);
            _AreaGraph.AddNode(new WorldArea());
            _AreaGraph.GetNodeData(i).addContainedBiome(workingGraph.GetNodeData(newCenter), newCenter);
        }

        for(int i = 0; i < areaCenters.Count; i++)
        {
            usableBiomes.Add(areaCenters[i]);
        }

        List<Vector2Int> biomeAreaMatching = new List<Vector2Int>();
        Dictionary<int,Biome> biomes = new Dictionary<int, Biome>();
        foreach (int i in usableBiomes)
        {
            biomeAreaMatching.Add(new Vector2Int(i, -1));
        }
        for (int i = 0; i < areaCenters.Count; i++)
        {
            int index = biomeAreaMatching.IndexOf(new Vector2Int(areaCenters[i], -1));
            biomeAreaMatching[index] = new Vector2Int(areaCenters[i], i);
        }
        int count = workingGraph.Count();
        while (count > 0)
        {

            for (int i = 0; i < areaCenters.Count; i++)
            {

                foreach (Vector2Int biome in biomeAreaMatching)
                {
                    if (biome.y == -1)
                    {
                        foreach (int neighbor in workingGraph.GetNeighbours(biome.x))
                        {
                            if (biomeAreaMatching.Find(x => x.x == neighbor).y == i)
                            {
                                biomeAreaMatching[biomeAreaMatching.IndexOf(biome)] = new Vector2Int(biome.x, -2);
                                break;
                            }
                        }
                    }
                }
                foreach(Vector2Int biome in biomeAreaMatching)
                {
                    Biome help;
                    if (biome.y == i && !biomes.TryGetValue(biome.x, out help)) {
                        biomes.Add(biome.x,workingGraph.GetNodeData(biome.x));
                        workingGraph.RemoveNode(biome.x);

                    }
                }
                foreach (Vector2Int biome in biomeAreaMatching)
                {
                    if (biome.y == -2)
                    {
                        biomeAreaMatching[biomeAreaMatching.IndexOf(biome)] = new Vector2Int(biome.x, i);
                    }
                }
                
            }

            count = workingGraph.Count();
        }

        foreach (Vector2Int biomeToAdd in biomeAreaMatching) {
            _AreaGraph.GetNodeData(biomeToAdd.y).addContainedBiome(biomes[biomeToAdd.x], biomeToAdd.x);
        }
      

    }

    public GameObject DrawAreaGraph(Voronoi voronoiDiagram, Graph<Biome> biomeGraph, float scale)
    {

        var result = new GameObject();
    

        var areas = new GameObject("Areas");
        areas.transform.parent = result.transform;

        var edgesIn = new GameObject("Inner Edges");
        edgesIn.transform.parent = result.transform;

        var borders = new GameObject("Borders");
        borders.transform.parent = result.transform;

        for (int i = 0; i < _AreaGraph.Count(); i++)
        {
            var pos = _AreaGraph.GetNodeData(i).GetCenter();
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "Area id: " + i;
            go.GetComponent<Collider>().enabled = false;
            go.transform.parent = areas.transform;
            go.transform.position = new Vector3(pos.x, 0, pos.y);
            go.transform.localScale = Vector3.one * 20 * scale;
            for (int j = 0; j < _AreaGraph.GetNodeData(i).ContainedBiomes.Count; j++)
            {
                var pos_biome = _AreaGraph.GetNodeData(i).ContainedBiomes[j].Center;
                var go_biome = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go_biome.name = "Biome id: " + _AreaGraph.GetNodeData(i).ContainedBiomesIDs[j];
                go_biome.GetComponent<Collider>().enabled = false;
                go_biome.transform.parent = go.transform;
                go_biome.transform.position = new Vector3(pos_biome.x, 0, pos_biome.y);
                go_biome.transform.localScale = Vector3.one * 0.5f;
            }
            for (int j = 0; j < _AreaGraph.GetNodeData(i).ContainedBiomes.Count - 1; j++)
            {
                var start = new Vector3(_AreaGraph.GetNodeData(i).ContainedBiomes[j].Center.x, 0, _AreaGraph.GetNodeData(i).ContainedBiomes[j].Center.y);
                var end = new Vector3(_AreaGraph.GetNodeData(i).ContainedBiomes[j + 1].Center.x, 0, _AreaGraph.GetNodeData(i).ContainedBiomes[j + 1].Center.y);
                GameObject myLine = new GameObject("Line");
                myLine.transform.position = start;
                myLine.transform.parent = edgesIn.transform;
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

        List<Vector3> pointTests = new List<Vector3>();
        for (int i = 0; i < biomeGraph.Count(); i++)
        {
            if (biomeGraph.GetNodeData(i).BiomeSettings.NotNavigable)
            {
                pointTests.Add(new Vector3(biomeGraph.GetNodeData(i).Center.x, biomeGraph.GetNodeData(i).Center.y, -1));
            }
        }
        for (int j = 0; j < _AreaGraph.Count(); j++)
        {
            foreach (Biome b in _AreaGraph.GetNodeData(j).ContainedBiomes)
            {
                pointTests.Add(new Vector3(b.Center.x, b.Center.y, j));
            }
        }

        foreach (var line in voronoiDiagram.VoronoiDiagram())
        {
            var start = new Vector3(line.p0.x, 0, line.p0.y);
            var end = new Vector3(line.p1.x, 0, line.p1.y);

            var middle = (start + end) / 2;
            var border = new Vector2f(start.x, start.z) - new Vector2f(end.x, end.z);

            var middlePoint = new Vector2f(middle.x, middle.z);
            
            var pointsA = pointTests.OrderBy(x => Mathf.Abs((new Vector2f(x.x, x.y) - middlePoint).x*border.x+(new Vector2f(x.x, x.y) - middlePoint).y*border.y)).Aggregate((agg,next)=> ((new Vector2f(agg.x,agg.y)-middlePoint).magnitude >= (new Vector2f(next.x, next.y) - middlePoint).magnitude ? next : agg));
            pointTests.Remove(pointsA);
            var pointsB = pointTests.OrderBy(x => Mathf.Abs((new Vector2f(x.x, x.y) - middlePoint).x * border.x + (new Vector2f(x.x, x.y) - middlePoint).y * border.y)).Aggregate((agg, next) => ((new Vector2f(agg.x, agg.y) - middlePoint).magnitude >= (new Vector2f(next.x, next.y) - middlePoint).magnitude ? next : agg));
            pointTests.Add(pointsA);


            var borderNormal = pointsA - pointsB;

            if (!((int)pointsA.z == (int)pointsB.z)) {
                GameObject myLine = new GameObject("Line");
                myLine.transform.position = start;
                myLine.transform.parent = borders.transform;
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

        return result;

    }

}

public class AreaGraphBuildingBiome
{
    public int originalBiomeId;
    public int belongingArea;
    public AreaGraphBuildingBiome(int biome) {
        originalBiomeId = biome;
        belongingArea = -1;
    }
}


public class WorldArea
{

    public readonly List<Biome> ContainedBiomes;
    public readonly List<int> ContainedBiomesIDs;

    public WorldArea()
    {
        ContainedBiomes = new List<Biome>();
        ContainedBiomesIDs = new List<int>();
    }

    public void addContainedBiome(Biome biome, int id)
    {
        ContainedBiomes.Add(biome);
        ContainedBiomesIDs.Add(id);
    }

    public Vector2 GetCenter()
    {

        Vector2 output = new Vector2(0, 0);
        foreach (Biome b in ContainedBiomes)
        {
            output += b.Center;
        }
        return output / ContainedBiomes.Count;

    }

}
