using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SplatPrototypeSerializable
{
    public Texture2D texture;
    public Texture2D normalMap;
    [Range(0f, 1f)] public float metallic;
    [Range(0f, 1f)] public float smoothness;
    public Vector2 tileOffset;
    public Vector2 tileSize;
}
