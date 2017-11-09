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
        Mesh
    }

    public DrawModeEnum DrawMode = DrawModeEnum.BiomeGraph;
    public BiomeDistribution BiomeDistribution;
    public List<BiomeSettings> AvailableBiomes;
    public int Seed = 0;

    public GameObject Graph;
    public MeshRenderer Mesh;

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
                (float)(BiomeDistribution.MapWidth - 1) / BiomeDistribution.XCells / 2f,
                (float)(BiomeDistribution.MapHeight - 1) / BiomeDistribution.YCells / 2f,
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
            case DrawModeEnum.Mesh:
                DrawMesh();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    void DrawMesh()
    {
    }

    void DrawGraph()
    {
        var newGraphInstance = _terrainStructure.DrawBiomeGraph();
        newGraphInstance.transform.parent = Graph.transform;
    }

    void ClearDisplay()
    {
        for (int i = 0; i < Graph.transform.childCount; i++)
            StartCoroutine(DestroyInEditor(Graph.transform.GetChild(i).gameObject));
    }

    IEnumerator DestroyInEditor(GameObject obj)
    {
        yield return new WaitForEndOfFrame();
        DestroyImmediate(obj, true);
    }
}