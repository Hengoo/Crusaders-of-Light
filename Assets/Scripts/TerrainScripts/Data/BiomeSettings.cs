using System;
using UnityEngine;

[CreateAssetMenu()]
public class BiomeSettings : ScriptableObject
{
    public BiomeNoise BiomeNoise;
    public BiomeConditions BiomeConditions;
    public float Influence;

    public BiomeSettings(BiomeConditions biomeConditions, BiomeNoise biomeNoise, float influence)
    {
        BiomeConditions = biomeConditions;
        BiomeNoise = biomeNoise;
        Influence = influence;
    }

    public static BiomeConditions BarInterpConditions(Vector2 pos, Biome b0, Biome b1, Biome b2)
    {
        var bar = pos.Barycentric(b0.Center, b1.Center, b2.Center);
        

        var temp = b0.BiomeSettings.BiomeConditions.Temperature * bar.x
                   + b1.BiomeSettings.BiomeConditions.Temperature * bar.y
                   + b2.BiomeSettings.BiomeConditions.Temperature * bar.z;
        var hum = b0.BiomeSettings.BiomeConditions.Humidity * bar.x
                  + b1.BiomeSettings.BiomeConditions.Humidity * bar.y
                  + b2.BiomeSettings.BiomeConditions.Humidity * bar.z;

        return new BiomeConditions(hum, temp);
    }

    public static BiomeNoise BarInterpNoise(Vector2 pos, Biome b0, Biome b1, Biome b2)
    {
        var bar = pos.Barycentric(b0.Center, b1.Center, b2.Center);

        var lacunarity = b0.BiomeSettings.BiomeNoise.Lacunarity * bar.x
                         + b1.BiomeSettings.BiomeNoise.Lacunarity * bar.y
                         + b2.BiomeSettings.BiomeNoise.Lacunarity * bar.z;
        var persistence = b0.BiomeSettings.BiomeNoise.Persistence * bar.x
                          + b1.BiomeSettings.BiomeNoise.Persistence * bar.y
                          + b2.BiomeSettings.BiomeNoise.Persistence * bar.z;
        var localMax = b0.BiomeSettings.BiomeNoise.LocalMax * bar.x
                       + b1.BiomeSettings.BiomeNoise.LocalMax * bar.y
                       + b2.BiomeSettings.BiomeNoise.LocalMax * bar.z;
        var localMin = b0.BiomeSettings.BiomeNoise.LocalMin * bar.x
                       + b1.BiomeSettings.BiomeNoise.LocalMin * bar.y
                       + b2.BiomeSettings.BiomeNoise.LocalMin * bar.z;

        return new BiomeNoise(lacunarity, persistence, localMax, localMin);
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
public class BiomeNoise
{
    [Range(0,1)]public float Persistence;
    [Range(0, 1)] public float LocalMax;
    [Range(0, 1)] public float LocalMin;
    public float Lacunarity;

    public BiomeNoise(float lacunarity, float persistence, float localMax, float localMin)
    {
        Lacunarity = lacunarity;
        LocalMax = localMax;
        LocalMin = localMin > localMax ? localMax : localMin;
        Persistence = Mathf.Clamp01(persistence);
    }
}


[Serializable]
public class BiomeDistribution
{
    [Range(10, 10000)] public int MapResolution = 33;
    [Range(0, 100)] public int XCells = 10;
    [Range(0, 100)] public int YCells = 10;
    [Range(0, 1000f)] public float CellOffset = 10;
    [Range(1, 8)] public int Octaves;
    public float PerlinScale;
    public Vector2 PerlinOffset;
    [Range(0,1f)]public float MaxHeight = 0.707f;
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
