using System;
using System.Collections;
using System.Collections.Generic;
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
    

    /* Debug variables */
    private TerrainStructure _terrainStructure;

    void Start()
    {
        gameObject.SetActive(false);
    }

    void OnValidate()
    {
        // Prevent offset from being larger than the cell size
        BiomeDistribution.CellOffset =
            Mathf.Min(
                (float)(BiomeDistribution.MapResolution - 1) / BiomeDistribution.XCells / 2f,
                (float)(BiomeDistribution.MapResolution - 1) / BiomeDistribution.YCells / 2f,
                BiomeDistribution.CellOffset
            );
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
        var heightMap = HeightMapGenerator.GenerateHeightMap(_terrainStructure, BiomeDistribution);
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