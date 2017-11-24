using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class LogicStructureGraph {

    Graph<WorldArea> _AreaGraph = new Graph<WorldArea>();
    
    public void GenerateAreaGraph(Graph<Biome> original, int maxSize) {
        
        Graph<Biome> workingGraph = original;

        //Get graph with only navigable biomes
        List<int> unusableBiomes = new List<int>();
        List<int> usableBiomes = new List<int>();

        for(int i = 0; i < workingGraph.Count(); i++)
        {
            if (workingGraph.GetNodeData(i).BiomeSettings.NotNavigable) {
                unusableBiomes.Add(i);
            }
            else
            {
                usableBiomes.Add(i);
            }
        }
        foreach (int i in unusableBiomes) {
            workingGraph.RemoveNode(i);
        }

        //Iterate over graph, add removed biomes to new areas
        int workingGraphCount = workingGraph.Count();
        while (workingGraphCount > 0)
        {
            int maxAreaSize = Random.Range(1, maxSize+1);
            int startingBiome = usableBiomes[Random.Range(0, usableBiomes.Count)];

            WorldArea newArea = new WorldArea();

            newArea.addContainedBiome(workingGraph.GetNodeData(startingBiome), startingBiome);
            
            int additionalBiome = startingBiome;

            for (int i = 0; i < maxAreaSize-1; i++)
            {                
                if (workingGraph.GetNeighbours(startingBiome).Length > 0) {
                    additionalBiome = workingGraph.GetNeighbours(startingBiome)[Random.Range(0, workingGraph.GetNeighbours(startingBiome).Length)];

                    workingGraph.RemoveNode(startingBiome);
                    usableBiomes.Remove(startingBiome);

                    newArea.addContainedBiome(workingGraph.GetNodeData(additionalBiome), additionalBiome);

                    startingBiome = additionalBiome;
                }
            }

            workingGraph.RemoveNode(additionalBiome);
            usableBiomes.Remove(additionalBiome);
            workingGraphCount = workingGraph.Count();

            _AreaGraph.AddNode(newArea);

        }

    }

    public GameObject DrawAreaGraph(float scale) {

        var result = new GameObject();

        var areas = new GameObject("Areas");
        areas.transform.parent = result.transform;

        var edgesIn = new GameObject("Inner Edges");
        edgesIn.transform.parent = result.transform;

        /*var edges = new GameObject("Edges");
        edges.transform.parent = result.transform;

        var borders = new GameObject("Borders");
        borders.transform.parent = result.transform;*/

        for (int i = 0; i < _AreaGraph.Count(); i++)
        {
            var pos = _AreaGraph.GetNodeData(i).GetCenter();
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "Area id: " + i;
            go.GetComponent<Collider>().enabled = false;
            go.transform.parent = areas.transform;
            go.transform.position = new Vector3(pos.x, 0, pos.y);
            go.transform.localScale = Vector3.one * 20 * scale;
            for(int j = 0; j < _AreaGraph.GetNodeData(i).ContainedBiomes.Count; j++)
            {
                var pos_biome = _AreaGraph.GetNodeData(i).ContainedBiomes[j].Center;
                var go_biome = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go_biome.name = "Biome id: " + _AreaGraph.GetNodeData(i).ContainedBiomesIDs[j];
                go_biome.GetComponent<Collider>().enabled = false;
                go_biome.transform.parent = go.transform;
                go_biome.transform.position = new Vector3(pos_biome.x, 0, pos_biome.y);
                go_biome.transform.localScale = Vector3.one * 0.5f;
            }
            for (int j = 0; j < _AreaGraph.GetNodeData(i).ContainedBiomes.Count-1; j++)
            {
                var start = new Vector3(_AreaGraph.GetNodeData(i).ContainedBiomes[j].Center.x, 0, _AreaGraph.GetNodeData(i).ContainedBiomes[j].Center.y);
                var end = new Vector3(_AreaGraph.GetNodeData(i).ContainedBiomes[j+1].Center.x, 0, _AreaGraph.GetNodeData(i).ContainedBiomes[j+1].Center.y);
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

        return result;

    }
    	
}



public class WorldArea {

    public readonly List<Biome> ContainedBiomes;
    public readonly List<int> ContainedBiomesIDs;

    public WorldArea() {
        ContainedBiomes = new List<Biome>();
        ContainedBiomesIDs = new List<int>();
    }

    public void addContainedBiome(Biome biome, int id) {
        ContainedBiomes.Add(biome);
        ContainedBiomesIDs.Add(id);
    }

    public Vector2 GetCenter() {

        Vector2 output = new Vector2(0,0);
        foreach(Biome b in ContainedBiomes)
        {
            output += b.Center;
        }
        return output / ContainedBiomes.Count;

    }

}
