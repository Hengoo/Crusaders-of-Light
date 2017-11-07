using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    public DrawModeEnum DrawMode = DrawModeEnum.BiomeGraph;
    public int Width = 1000;
    public int Height = 1000;

    private TerrainStructure _terrainStructure;
	// Use this for initialization
	void Start ()
	{
	    List<BiomeData> biomeData = new List<BiomeData>();

	    var x = Random.Range(-Width / 2f, Width / 2f);
	    var y = Random.Range(-Height / 2f, Height / 2f);
	    biomeData.Add(new BiomeData(new Vector2(x, y), 1, Random.Range(0, 1f), Random.Range(0, 1f)));
        _terrainStructure = new TerrainStructure(biomeData, Width, Height);
	    _terrainStructure.DrawGraph();

	    x = Random.Range(-Width / 2f, Width / 2f);
	    y = Random.Range(-Height / 2f, Height / 2f);
	    biomeData.Add(new BiomeData(new Vector2(x, y), 1, Random.Range(0, 1f), Random.Range(0, 1f)));
	    _terrainStructure = new TerrainStructure(biomeData, Width, Height);
	    _terrainStructure.DrawGraph();

	    x = Random.Range(-Width / 2f, Width / 2f);
	    y = Random.Range(-Height / 2f, Height / 2f);
	    biomeData.Add(new BiomeData(new Vector2(x, y), 1, Random.Range(0, 1f), Random.Range(0, 1f)));
	    _terrainStructure = new TerrainStructure(biomeData, Width, Height);
	    _terrainStructure.DrawGraph();

    }
	
	// Update is called once per frame
    void DrawMesh()
    {
        Vector3[,] vertices = new Vector3[Width, Height];
        Color[,] vertexColors = new Color[Width, Height];
        int[] triangles = new int[(Width - 1)  * (Height - 1) * 2];


        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                var position = new Vector3(x - Width, 0, y - Height);
                vertices[x, y] = position;
                _terrainStructure.SampleBiomeData(position.x, position.z);
                vertexColors[x,y] = new Color();
            }
        }

        // Generate Triangles
        for (int i = 0; i < triangles.Length; i++)
        {
            
        }
    }

    public enum DrawModeEnum
    {
        BiomeGraph
    }
}
