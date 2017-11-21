using System;
using UnityEngine;

[CreateAssetMenu()]
public class BiomeSettings : ScriptableObject
{
    public BiomeHeight BiomeHeight;
    public BiomeConditions BiomeConditions;
    public bool NotNavigable = false;

    public BiomeSettings(BiomeConditions biomeConditions, BiomeHeight biomeHeight, bool notNavigable)
    {
        BiomeConditions = biomeConditions;
        BiomeHeight = biomeHeight;
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
public class BiomeDistribution
{
    [Range(16, 1024)] public int HeightMapResolution = 128;
    [Range(16, 1024)] public float MapSize = 128;
    [Range(1, 1024)] public float MapHeight = 10;
    [Range(10, 1000)] public int BiomeSamples = 50;
    [Range(0, 1f)] public float MaxHeight = 0.707f;
    [Range(0, 1f)] public float SeaHeight = 0.15f;
    [Range(0, 20)] public int LloydRelaxation = 5;
    [Range(1, 8)] public int Octaves = 3;
}

public class Biome
{
    public readonly Vector2 Center;
    public readonly BiomeSettings BiomeSettings;

    public Biome(Vector2 center, BiomeSettings biomeSettings)
    {
        Center = center;
        BiomeSettings = biomeSettings;
    }
}
