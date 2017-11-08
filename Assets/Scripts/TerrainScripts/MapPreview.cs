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
    [Range(100, 10000)] public int MapWidth = 1000;
    [Range(100, 10000)] public int MapHeight = 1000;
    [Range(0, 100)] public int XCells = 10;
    [Range(0, 100)] public int YCells = 10;
    [Range(0, 1000f)] public float CellOffset = 10;
    public int Seed = 0;
    public bool DebugMode = false;

    public GameObject Graph;


    /* Debug variables */
    private TerrainStructure _terrainStructure;

    void Start()
    {
        gameObject.SetActive(false);
    }

    void OnValidate()
    {
        // Prevent offset from being larger than the cell size
        CellOffset = Mathf.Min((float) (MapWidth - 1) / XCells / 2f, (float) (MapHeight - 1) / YCells / 2f, CellOffset);
        
        //Update Display Settings
        DrawInEditor();
    }

    /* Redraws preview in the scene editor */
    void DrawInEditor()
    {
        if (!DebugMode || Application.isPlaying)
            return;

        Random.InitState(Seed);
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
        for(int i = 0; i < Graph.transform.childCount; i++)
            StartCoroutine(DestroyInEditor(Graph.transform.GetChild(i).gameObject));

        List<BiomeData> biomeData = new List<BiomeData>();
        foreach (var point in DistributePoints(XCells, YCells, MapWidth, MapHeight, CellOffset))
        {
            biomeData.Add(new BiomeData(point, 1, 1, 1));
        }
        _terrainStructure = new TerrainStructure(biomeData, MapWidth, MapWidth);
        var newGraphInstance = _terrainStructure.DrawGraph();
        newGraphInstance.transform.parent = Graph.transform;
    }

    IEnumerator DestroyInEditor(GameObject obj)
    {
        yield return new WaitForEndOfFrame();
        DestroyImmediate(obj, true);
    }

    /* 
     * Distribute points using Grid Jitter technique
     * - Number of points is cellsX * cellsY
     * - Grid origin is assumed to be at world 0,0,0 and increases in +X and +Z
     */
    Vector2[,] DistributePoints(int cellsX, int cellsY, float width, float height, float offset)
    {
        var result = new Vector2[cellsX, cellsY];
        var cellSize = new Vector2(width / cellsX, height / cellsY);
        for (int y = 0; y < cellsY; y++)
        {
            for (int x = 0; x < cellsX; x++)
            {
                var posX = Random.Range(x * cellSize.x + offset, (x + 1) * cellSize.x - offset);
                var posY = Random.Range(y * cellSize.y + offset, (y + 1) * cellSize.y - offset);
                result[x, y] = new Vector2(posX, posY);
            }
        }

        return result;
    }
}