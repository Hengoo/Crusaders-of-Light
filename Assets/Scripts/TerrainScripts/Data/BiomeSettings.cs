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
    
    public float Temperature;
    public float Humidity;
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

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        NoiseSettings.ValidateValues();
        base.OnValidate();
    }
#endif
}
