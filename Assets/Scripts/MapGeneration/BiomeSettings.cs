using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Biome", menuName = "Terrain/Biome")]
public class BiomeSettings : ScriptableObject
{
    public string UniqueName;
    public BiomeHeight Height;
    public BiomeConditions Conditions;
    public GameObject[] FillPrefabs;
    [Range(1f, 20f)] public float PrefabMinDistance = 10f;
    public bool NotNavigable = false;
    public SplatPrototypeSerializable Splat;
    public List<BiomeSettings> DontBlendWith = new List<BiomeSettings>();
    public TagBiome[] BiomeTags;
    public SpawnerSetMasterList SpawnerList;
    public BiomeSettings(BiomeConditions conditions, BiomeHeight height, bool notNavigable)
    {
        Conditions = conditions;
        Height = height;
        NotNavigable = notNavigable;
    }
}

[Serializable]
public struct BiomeConditions
{
    [Range(0, 1f)] public float Humidity;
    [Range(0, 1f)] public float Temperature;
    public BiomeConditions(float humidity, float temperature)
    {
        Humidity = humidity;
        Temperature = temperature;
    }
}

[Serializable]
public class BiomeHeight
{
    [Range(0, 1)] public float Persistence = 0.3f;
    [Range(0, 1)] public float LocalMax = 0.6f;
    [Range(0, 1)] public float LocalMin = 0.5f;
    [Range(0, 20)] public float Lacunarity = 2f;
    [Range(1f, 100f)] public float Scale = 10f;

    public BiomeHeight(float lacunarity, float persistence, float localMax, float localMin, float scale)
    {
        Lacunarity = lacunarity;
        LocalMax = localMax;
        Scale = scale;
        LocalMin = localMin > localMax ? localMax : localMin;
        Persistence = Mathf.Clamp01(persistence);
    }
}


[Serializable]
public class BiomeGlobalConfiguration
{
    [Range(16, 1024)] public int HeightMapResolution = 512;
    [Range(16, 1024)] public float MapSize = 1024;
    [Range(1, 1024)] public float MapHeight = 80;
    [Range(10, 1000)] public int BiomeSamples = 45;
    [Range(0, 1f)] public float MaxHeight = 1;
    [Range(0, 1f)] public float SeaHeight = 0.15f;
    [Range(0, 50f)] public float BorderNoise = 8f;
    [Range(0, 20)] public int LloydRelaxation = 5;
    [Range(1, 8)] public int Octaves = 3;
    public BiomeSettings BorderBiome;
    public BiomeSettings CliffBiome;
    public Material WaterMaterial;
    public Material TerrainMaterial;
    public GameObject CoastBlocker;
    public GameObject CoastBlockerPole;
    [Range(0.01f, 20)] public float CoastBlockerLength = 1;
    [Range(0f, 50f)] public float CoastInlandOffset = 5f;
    public SplatPrototypeSerializable RoadSplatPrototype;
    [Range(0, 5)] public int OverallSmoothing = 2;
    public bool SmoothEdges = true;
    [Range(0, 20)] public int EdgeWidth = 3;
    [Range(0, 20)] public int SquareSize = 2;
}

public class Biome
{
    public readonly Vector2 Center;
    public readonly BiomeSettings BiomeSettings;
    public readonly Vector2[] BiomePolygon;
    public readonly bool IsBorderBiome;
    [HideInInspector]public int BiomeLevel = -1;

    public Biome(Vector2 center, BiomeSettings biomeSettings, bool isBorderBiome, Vector2[] biomePolygon)
    {
        Center = center;
        BiomeSettings = biomeSettings;
        IsBorderBiome = isBorderBiome;
        BiomePolygon = biomePolygon;
    }
}
