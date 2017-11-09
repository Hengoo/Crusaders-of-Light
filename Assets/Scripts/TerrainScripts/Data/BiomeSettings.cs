using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class BiomeSettings : UpdatableData
{
    public NoiseSettings NoiseSettings;
    
    public bool UseFalloff;
    public float HeightMultiplier;
    public AnimationCurve HeightCurve;

    public BiomeConditions BiomeConditions;
    public float Influence;

    public float MinHeight
    {
        get
        {
            return HeightMultiplier * HeightCurve.Evaluate(0);
        }
    }

    public float MaxHeight
    {
        get
        {
            return HeightMultiplier * HeightCurve.Evaluate(1);
        }
    }

    public static BiomeConditions BarInterpConditions(Vector3 baryCoord, BiomeSettings n0, BiomeSettings n1, BiomeSettings n2)
    {

        var temp = n0.BiomeConditions.Temperature * baryCoord.x
                   + n1.BiomeConditions.Temperature * baryCoord.y
                   + n2.BiomeConditions.Temperature * baryCoord.z;
        var hum = n0.BiomeConditions.Humidity * baryCoord.x
                  + n1.BiomeConditions.Humidity * baryCoord.y
                  + n2.BiomeConditions.Humidity * baryCoord.z;

        return new BiomeConditions(hum, temp);
    }

    public static NoiseSettings BarInterpNoise(Vector3 baryCoord, BiomeSettings n0, BiomeSettings n1, BiomeSettings n2)
    {
        var noiseSettings = new NoiseSettings
        {
            lacunarity = n0.NoiseSettings.lacunarity * baryCoord.x
                         + n1.NoiseSettings.lacunarity * baryCoord.y
                         + n2.NoiseSettings.lacunarity * baryCoord.z,
            persistance = n0.NoiseSettings.persistance * baryCoord.x
                          + n1.NoiseSettings.persistance * baryCoord.y
                          + n2.NoiseSettings.persistance * baryCoord.z,
            scale = n0.NoiseSettings.scale * baryCoord.x
                         + n1.NoiseSettings.scale * baryCoord.y
                         + n2.NoiseSettings.scale * baryCoord.z,
            octaves = n0.NoiseSettings.octaves


        };

        return noiseSettings;
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        NoiseSettings.ValidateValues();
        base.OnValidate();
    }
#endif
}

public struct BiomeConditions
{
    public readonly float Humidity, Temperature;
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
