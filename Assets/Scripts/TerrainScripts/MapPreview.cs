﻿using System;
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
    public BiomeConfiguration BiomeConfiguration;
    public List<BiomeSettings> AvailableBiomes;
    public int Seed = 0;
    

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
        _terrainStructure = new TerrainStructure(AvailableBiomes, BiomeConfiguration);
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
        var heightMap = HeightMapManager.GenerateHeightMap(_terrainStructure, BiomeConfiguration);
        if (BiomeConfiguration.SmoothEdges)
        {
            //Smooth biome borders
            heightMap = HeightMapManager.SmoothHeightMapWithLines(heightMap, BiomeConfiguration.MapSize / BiomeConfiguration.HeightMapResolution, _terrainStructure.GetBiomeSmoothBorders(), BiomeConfiguration.EdgeWidth, BiomeConfiguration.SquareSize);

            //Rough biome borders
            //heightMap = HeightMapManager.SmoothHeightMapWithLines(heightMap, BiomeConfiguration.MapSize / BiomeConfiguration.HeightMapResolution, _terrainStructure.GetBiomeBorders(), 3, 2);

            //Overall smoothing
            if (BiomeConfiguration.OverallSmoothing > 0)
            {
                heightMap = HeightMapManager.SmoothHeightMap(heightMap, BiomeConfiguration.OverallSmoothing);
                heightMap = HeightMapManager.SmoothHeightMap(heightMap, BiomeConfiguration.OverallSmoothing);
            }
        }
        var terrainData = new TerrainData();
        
        terrainData.baseMapResolution = BiomeConfiguration.HeightMapResolution;
        terrainData.heightmapResolution = Mathf.ClosestPowerOfTwo(BiomeConfiguration.HeightMapResolution) + 1;
        terrainData.alphamapResolution = BiomeConfiguration.HeightMapResolution;
        terrainData.SetDetailResolution(BiomeConfiguration.HeightMapResolution, 32);
        terrainData.size = new Vector3(BiomeConfiguration.MapSize, BiomeConfiguration.MapHeight, BiomeConfiguration.MapSize);

        var terrain = Terrain.CreateTerrainGameObject(terrainData);
        terrain.name = "Terrain";
        terrain.transform.parent = transform;
        terrain.transform.position = Vector3.zero;
        terrain.GetComponent<Terrain>().terrainData.SetHeights(0,0,heightMap);

        var water = GameObject.CreatePrimitive(PrimitiveType.Plane);
        water.GetComponent<Renderer>().material = BiomeConfiguration.WaterMaterial;
        water.transform.localScale = new Vector3(terrainData.size.x/10f, 1, terrainData.size.z/10f);
        water.transform.parent = terrain.transform;
        water.transform.localPosition = new Vector3(terrainData.size.x/2, (BiomeConfiguration.SeaHeight + 0.01f )* terrainData.size.y, terrainData.size.z/2);
    }

    void DrawGraph()
    {
        var newGraphInstance = _terrainStructure.DrawBiomeGraph(BiomeConfiguration.HeightMapResolution / 500f);
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