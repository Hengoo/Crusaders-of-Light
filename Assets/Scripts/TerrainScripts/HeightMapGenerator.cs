using UnityEngine;

public class HeightMapGenerator : MonoBehaviour {

    public static HeightMap GenerateHeightMap(int width, int height, Vector2 sampleCenter)
    {

        float minValue = float.MaxValue;
        float maxValue = float.MinValue;

        float[,] values = Noise.GenerateNoiseMap(width, height, sampleCenter);
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                values[i, j] *= heightCurve_threadSafe.Evaluate(values[i, j]) * settings.HeightMultiplier;

                maxValue = Mathf.Max(values[i, j], maxValue);
                minValue = Mathf.Min(values[i, j], minValue);
            }
        }

        return new HeightMap(values, minValue, maxValue);
    }

}

public struct HeightMap
{
    public readonly float[,] values;
    public readonly float minValue, maxValue;

    public HeightMap(float[,] values, float minValue, float maxValue)
    {
        this.values = values;
        this.minValue = minValue;
        this.maxValue = maxValue;
    }
}
