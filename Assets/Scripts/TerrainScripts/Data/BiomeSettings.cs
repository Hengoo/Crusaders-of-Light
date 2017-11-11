using System;
using UnityEngine;

[CreateAssetMenu()]
public class BiomeSettings : ScriptableObject
{
    public BiomeHeight BiomeHeight;
    public BiomeConditions BiomeConditions;
    public float Influence;

    public BiomeSettings(BiomeConditions biomeConditions, BiomeHeight biomeHeight, float influence)
    {
        BiomeConditions = biomeConditions;
        BiomeHeight = biomeHeight;
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

    public static BiomeHeight BarInterpNoise(Vector2 pos, Biome b0, Biome b1, Biome b2)
    {
        var bar = pos.Barycentric(b0.Center, b1.Center, b2.Center);

        var lacunarity = b0.BiomeSettings.BiomeHeight.Lacunarity * bar.x
                         + b1.BiomeSettings.BiomeHeight.Lacunarity * bar.y
                         + b2.BiomeSettings.BiomeHeight.Lacunarity * bar.z;
        var persistence = b0.BiomeSettings.BiomeHeight.Persistence * bar.x
                          + b1.BiomeSettings.BiomeHeight.Persistence * bar.y
                          + b2.BiomeSettings.BiomeHeight.Persistence * bar.z;
        var localMax = b0.BiomeSettings.BiomeHeight.LocalMax * bar.x
                       + b1.BiomeSettings.BiomeHeight.LocalMax * bar.y
                       + b2.BiomeSettings.BiomeHeight.LocalMax * bar.z;
        var localMin = b0.BiomeSettings.BiomeHeight.LocalMin * bar.x
                       + b1.BiomeSettings.BiomeHeight.LocalMin * bar.y
                       + b2.BiomeSettings.BiomeHeight.LocalMin * bar.z;

        return new BiomeHeight(lacunarity, persistence, localMax, localMin);
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
    [Range(0,1)]public float Persistence;
    [Range(0, 1)] public float LocalMax;
    [Range(0, 1)] public float LocalMin;
    public float Lacunarity;

    public BiomeHeight(float lacunarity, float persistence, float localMax, float localMin)
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
    [Range(10, 10000)] public int MapResolution = 128;
    [Range(10, 1000)] public int BiomeSamples = 50;
    [Range(0,1f)]public float MaxHeight = 0.707f;
    [Range(0, 20)] public int LloydRelaxation = 5;
}

public class Biome
{
    public readonly Vector2 Center;
    public readonly BiomeSettings BiomeSettings;
    public bool IsWater;

    public Biome(Vector2 center, BiomeSettings biomeSettings, bool isWater)
    {
        Center = center;
        BiomeSettings = biomeSettings;
        IsWater = isWater;
    }
}
