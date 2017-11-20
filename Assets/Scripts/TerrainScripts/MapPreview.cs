using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Random = UnityEngine.Random;

public class MapPreview : MonoBehaviour
{

    public enum DrawModeEnum
    {
        BiomeGraph,
        Terrain
    }

    public DrawModeEnum DrawMode = DrawModeEnum.BiomeGraph;
    public BiomeDistribution BiomeDistribution;
    public List<BiomeSettings> AvailableBiomes;
    public int Seed = 0;
    public Material WaterMaterial;
    

    /* Debug variables */
    private TerrainStructure _terrainStructure;

    void Start()
    {
        gameObject.SetActive(false);
    }

    /* Redraws preview in the scene editor */
    public void GeneratePreview()
    {
        if (Application.isPlaying)
            return;

        ClearDisplay();
        Random.InitState(Seed);
        _terrainStructure = new TerrainStructure(AvailableBiomes, BiomeDistribution);
        switch (DrawMode)
        {
            case DrawModeEnum.BiomeGraph:
                DrawGraph();
                break;
            case DrawModeEnum.Terrain:
                DrawMesh();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    void DrawMesh()
    {
        var heightMap = HeightMapManager.GenerateHeightMap(_terrainStructure, BiomeDistribution);
        heightMap = HeightMapManager.SmoothBiomeEdges(heightMap, 1, _terrainStructure.GetBiomeEdges(), 1);
        var terrainData = new TerrainData();
        
        terrainData.baseMapResolution = BiomeDistribution.MapResolution;
        terrainData.heightmapResolution = Mathf.ClosestPowerOfTwo(BiomeDistribution.MapResolution) + 1;
        terrainData.alphamapResolution = BiomeDistribution.MapResolution;
        terrainData.SetDetailResolution(BiomeDistribution.MapResolution, 16);
        terrainData.size = new Vector3(BiomeDistribution.MapResolution, 10, BiomeDistribution.MapResolution);

        var terrain = Terrain.CreateTerrainGameObject(terrainData);
        terrain.name = "Terrain";
        terrain.transform.parent = transform;
        terrain.transform.position = Vector3.zero;
        terrain.GetComponent<Terrain>().terrainData.SetHeights(0,0,heightMap);

        var water = GameObject.CreatePrimitive(PrimitiveType.Plane);
        water.GetComponent<Renderer>().material = WaterMaterial;
        water.transform.localScale = new Vector3(terrainData.size.x/10f, 1, terrainData.size.z/10f);
        water.transform.parent = terrain.transform;
        water.transform.localPosition = new Vector3(terrainData.size.x/2, (BiomeDistribution.SeaHeight + 0.01f )* terrainData.size.y, terrainData.size.z/2);
    }

    void DrawGraph()
    {
        var newGraphInstance = _terrainStructure.DrawBiomeGraph(BiomeDistribution.MapResolution / 500f);
        newGraphInstance.name = "Graph";
        newGraphInstance.transform.parent = transform;
    }

    void ClearDisplay()
    {
        var toDelete = new List<GameObject>();
        foreach (var o in FindObjectsOfType(typeof(GameObject)))
        {
            var go = (GameObject) o;
            if (go.name == "Graph" || go.name == "Terrain")
                toDelete.Add(go);
        }

        foreach (var go in toDelete)
        {
            StartCoroutine(DestroyInEditor(go));
        }
    }

    IEnumerator DestroyInEditor(GameObject obj)
    {
        yield return new WaitForEndOfFrame();
        DestroyImmediate(obj, true);
    }
}