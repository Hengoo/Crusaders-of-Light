using System;
using UnityEngine;

[CreateAssetMenu()]
public class BiomeSettings : MonoBehaviour
{
    public BiomeConditions BiomeConditions;
    public float Influence;

    public BiomeSettings(BiomeConditions biomeConditions, float influence)
    {
        BiomeConditions = biomeConditions;
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
