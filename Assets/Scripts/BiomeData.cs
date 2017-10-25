using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BiomeData : ScriptableObject
{
    public Vector2 Center;
    public float Influence;
    public float Humidity;
    public float Temperature;

    public Material BiomeMaterial;
}
