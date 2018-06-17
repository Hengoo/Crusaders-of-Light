// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)
//unity built in shader modifierd

Shader "Custom/CustomTerrain" {
	Properties{
	[HideInInspector] _Control("Control (RGBA)", 2D) = "red" {}
	[HideInInspector] _Splat3("Layer 3 (A)", 2D) = "white" {}
	[HideInInspector] _Splat2("Layer 2 (B)", 2D) = "white" {}
	[HideInInspector] _Splat1("Layer 1 (G)", 2D) = "white" {}
	[HideInInspector] _Splat0("Layer 0 (R)", 2D) = "white" {}
	[HideInInspector] _Normal3("Normal 3 (A)", 2D) = "bump" {}
	[HideInInspector] _Normal2("Normal 2 (B)", 2D) = "bump" {}
	[HideInInspector] _Normal1("Normal 1 (G)", 2D) = "bump" {}
	[HideInInspector] _Normal0("Normal 0 (R)", 2D) = "bump" {}
	// used in fallback on old cards & base map
	[HideInInspector] _MainTex("BaseMap (RGB)", 2D) = "white" {}
	[HideInInspector] _Color("Main Color", Color) = (1,1,1,1)

	_HeightFactor("HeightFactor",Range(0, 10)) = 1
	_Smoothness("smoothness",Range(0, 1)) = 0
	}

		CGINCLUDE
//#pragma surface surf Lambert vertex:SplatmapVert finalcolor:SplatmapFinalColor finalprepass:SplatmapFinalPrepass finalgbuffer:SplatmapFinalGBuffer noinstancing
#pragma surface surf Standard vertex:SplatmapVert finalcolor:SplatmapFinalColor finalprepass:SplatmapFinalPrepass finalgbuffer:SplatmapFinalGBuffer noinstancing
#pragma multi_compile_fog

//not totally sure why but those lines are needed to use the standart (pbr) approach and add smoothness to material
#include "UnityPBSLighting.cginc"
//#define TERRAIN_SPLAT_ADDPASS
//#define TERRAIN_STANDARD_SHADER
#define TERRAIN_SURFACE_OUTPUT SurfaceOutputStandard
#include "TerrainSplatmapCommon.cginc"


half _HeightFactor;
half _Smoothness;

inline float heightblend(float heightSelf, float height0, float height1, float height2)
{
	//TODO: magic blending here
	//general idea: think of weight as a height, and add the respective (scaled)weight to it.
	//then have to blend between the wieghts that are close to each other (have some scalar that determines the distance for blending)
	//possible problem with this idea:
	
	//topBlend = clamp( topBlend + (heightTop - height) *_HeightFactor, 0 ,1);
	//float result = (heightSelf - heightOther) 

	//this seems to work the best
	return  (heightSelf*3 - height0 - height1 - height2)*_HeightFactor;
}

void SplatmapMixTest(Input IN, out half4 splat_control, out half weight, out fixed4 mixedDiffuse, inout fixed3 mixedNormal)
{
	//this code snippet: https://forum.unity.com/threads/terrain-multi-uv-mixing-with-new-unity-5-standard-shader.308112/
	//heavy modified
    splat_control = tex2D(_Control, IN.tc_Control);
    weight = dot(splat_control, half4(1,1,1,1));
 
    #ifndef UNITY_PASS_DEFERRED
        // Normalize weights before lighting and restore weights in applyWeights function so that the overal
        // lighting result can be correctly weighted.
        // In G-Buffer pass we don't need to do it if Additive blending is enabled.
        // TODO: Normal blending in G-buffer pass...
        splat_control /= (weight + 1e-3f); // avoid NaNs in splat_control
    #endif
 
    #if !defined(SHADER_API_MOBILE) && defined(TERRAIN_SPLAT_ADDPASS)
        clip(weight - 0.0039 /*1/255*/);
    #endif

	//TODO: extract the height value from the alpha value of the texture.
	//-> adjust splat_control with heightblending
	fixed4 albedo0 = tex2D(_Splat0, IN.uv_Splat0);
	fixed4 albedo1 = tex2D(_Splat1, IN.uv_Splat1);
	fixed4 albedo2 = tex2D(_Splat2, IN.uv_Splat2);
	fixed4 albedo3 = tex2D(_Splat3, IN.uv_Splat3);

	//probably have to normalize the height ???
	float height0 = albedo0.a  * splat_control.r;
	float height1 = albedo1.a  * splat_control.g;
	float height2 = albedo2.a  * splat_control.b;
	float height3 = albedo3.a * splat_control.a;
	//float height0 = albedo0.a;
	//float height1 = albedo1.a;
	//float height2 = albedo2.a;
	//float height3 = albedo3.a;

	//magic blending?
	//splat_control.r += heightblend(height0, height1, height2, height3);
	//splat_control.g += heightblend(height1, height0, height2, height3);
	//splat_control.b += heightblend(height2, height1, height0, height3);
	//splat_control.a += heightblend(height3, height1, height2, height0);
	splat_control.r = clamp(splat_control.r + heightblend(height0, height1, height2, height3),0,1);
	splat_control.g = clamp(splat_control.g + heightblend(height1, height0, height2, height3),0,1);
	splat_control.b = clamp(splat_control.b + heightblend(height2, height1, height0, height3),0,1);
	splat_control.a = clamp(splat_control.a + heightblend(height3, height1, height2, height0),0,1);

	float sum = splat_control.r + splat_control.g + splat_control.b + splat_control.a;

	splat_control = splat_control * 1/sum;

    mixedDiffuse = 0.0f;
    //mixedDiffuse += splat_control.r * tex2D(_Splat0, IN.uv_Splat0) * tex2D(_Splat0, IN.uv_Splat0 * -0.25) * 4;
    //mixedDiffuse += splat_control.g * tex2D(_Splat1, IN.uv_Splat1) * tex2D(_Splat1, IN.uv_Splat1 * -0.25) * 4;
    //mixedDiffuse += splat_control.b * tex2D(_Splat2, IN.uv_Splat2) * tex2D(_Splat2, IN.uv_Splat2 * -0.25) * 4;
    //mixedDiffuse += splat_control.a * tex2D(_Splat3, IN.uv_Splat3) * tex2D(_Splat3, IN.uv_Splat3 * -0.25) * 4;

	//we dont use a, but ... doesnt matter here
	mixedDiffuse += splat_control.r * albedo0.rgba;
	mixedDiffuse += splat_control.g * albedo1.rgba;
	mixedDiffuse += splat_control.b * albedo2.rgba;
	mixedDiffuse += splat_control.a * albedo3.rgba;
 
	
    #ifdef _TERRAIN_NORMAL_MAP
        fixed4 nrm = 0.0f;
        nrm += splat_control.r * tex2D(_Normal0, IN.uv_Splat0);
        nrm += splat_control.g * tex2D(_Normal1, IN.uv_Splat1);
        nrm += splat_control.b * tex2D(_Normal2, IN.uv_Splat2);
		nrm += splat_control.a * tex2D(_Normal3, IN.uv_Splat3);
        mixedNormal = UnpackNormal(nrm);
    #endif
}

void surf(Input IN, inout SurfaceOutputStandard o)
	{
		half4 splat_control;
		half weight;
		half4 mixedDiffuse;

		//SplatmapMix(IN, splat_control, weight, mixedDiffuse, o.Normal);
		SplatmapMixTest(IN, splat_control, weight, mixedDiffuse, o.Normal);
		o.Albedo = mixedDiffuse.rgb;
		o.Alpha = weight;
		o.Smoothness = _Smoothness;
	}
	ENDCG
	
		Category{
		Tags{
		"Queue" = "Geometry-99"
		"RenderType" = "Opaque"
	}
		// TODO: Seems like "#pragma target 3.0 _TERRAIN_NORMAL_MAP" can't fallback correctly on less capable devices?
		// Use two sub-shaders to simulate different features for different targets and still fallback correctly.
		SubShader{ // for sm3.0+ targets
		CGPROGRAM
#pragma target 3.0
#pragma multi_compile __ _TERRAIN_NORMAL_MAP
		ENDCG
	}
		SubShader{ // for sm2.0 targets
		CGPROGRAM
		ENDCG
	}
	}

		//Dependency "AddPassShader" = "Custom/Terrain"
		//Dependency "BaseMapShader" = "Diffuse"
		//Dependency "Details0" = "Hidden/TerrainEngine/Details/Vertexlit"
		//Dependency "Details1" = "Hidden/TerrainEngine/Details/WavingDoublePass"
		//Dependency "Details2" = "Hidden/TerrainEngine/Details/BillboardWavingDoublePass"
		//Dependency "Tree0" = "Hidden/TerrainEngine/BillboardTree"

		Fallback "Diffuse"
}
