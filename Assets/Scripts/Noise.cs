﻿using System;
using UnityEngine;

public static class Noise
{

    public enum NormalizeMode { Local, Global }

    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, NoiseSettings settings, Vector2 sampleCenter)
    {
        float[,] noiseMap = new float[mapWidth, mapHeight];

        float maxPossibleHeight = 0;
        float amplitude = 1;
        float frequency = 1;

        System.Random prng = new System.Random(GameController.Instance.Seed);

        float maxLocalNoiseHeight = float.MinValue;
        float minLocalNoiseHeight = float.MaxValue;

        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                amplitude = 1;
                frequency = 1;
                float noiseHeight = 0;

                Vector2[] octaveOffsets = new Vector2[settings.octaves];
                for (int i = 0; i < settings.octaves; i++)
                {
                    float offsetX = prng.Next(-100000, 100000) + settings.offset.x + sampleCenter.x;
                    float offsetY = prng.Next(-100000, 100000) - settings.offset.y - sampleCenter.y;
                    octaveOffsets[i] = new Vector2(offsetX, offsetY);

                    maxPossibleHeight += amplitude;
                    amplitude *= settings.persistance;
                }

                for (int i = 0; i < settings.octaves; i++)
                {
                    float sampleX = (x - halfWidth + octaveOffsets[i].x) / settings.scale * frequency;
                    float sampleY = (y - halfHeight + octaveOffsets[i].y) / settings.scale * frequency;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= settings.persistance;
                    frequency *= settings.lacunarity;
                }
                maxLocalNoiseHeight = Mathf.Max(noiseHeight, maxLocalNoiseHeight);
                minLocalNoiseHeight = Mathf.Min(noiseHeight, minLocalNoiseHeight);
                noiseMap[x, y] = noiseHeight;

                if (settings.normalizeMode == NormalizeMode.Global)
                {
                    float normalizedHeight = (noiseMap[x, y] + 1) / maxPossibleHeight;
                    noiseMap[x, y] = Mathf.Clamp(normalizedHeight, 0, int.MaxValue);
                }
            }
        }

        if (settings.normalizeMode == NormalizeMode.Local)
            for (int y = 0; y < mapHeight; y++)
                for (int x = 0; x < mapWidth; x++)
                    noiseMap[x, y] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[x, y]);

        return noiseMap;
    }

}

[Serializable]
public class NoiseSettings
{
    public Noise.NormalizeMode normalizeMode;
    public float scale = 50;
    public int octaves = 6;
    [Range(0, 1)]
    public float persistance = .6f;
    public float lacunarity = 2;
    
    public Vector2 offset;

    public void ValidateValues()
    {
        scale = Mathf.Max(scale, 0.01f);
        octaves = Mathf.Max(1, octaves);
        lacunarity = Mathf.Max(1, lacunarity);
        persistance = Mathf.Clamp01(persistance);
    }
}
