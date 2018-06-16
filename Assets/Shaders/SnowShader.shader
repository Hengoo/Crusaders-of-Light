
// Normal Mapping for a Triplanar Shader - Ben Golus 2017
// Unity Surface Shader example shader

// Implements correct triplanar normals in a Surface Shader with out computing or passing additional information from the
// vertex shader. Instead works around some oddities with how Surface Shaders handle the tangent space vectors. Attempting
// to directly access the tangent matrix data results in a shader generation error. This works around the issue by tricking
// the surface shader into not using those vectors until actually in the generated shader code.

//this is modified by Hengo



//IDEA: decode the heiht into the nromal map alpha??? (doesa athiswork for terrain? does it support norm,al texutre with alpha?)

Shader "Custom/SnowShader" {
	Properties{
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		[NoScaleOffset] _BumpMap("Normal Map", 2D) = "bump" {}
		[NoScaleOffset] _HeightMap("HeightMap", 2D) = "white" {}

		_MainTexUp("Albedo (RGB) Up", 2D) = "white" {}
		[NoScaleOffset] _BumpMapUp("Normal Map UP", 2D) = "bump" {}
		[NoScaleOffset] _HeightMapUp("HeightMap UP", 2D) = "white" {}

		_TopFactor("TopFactor", Range(-1, 1.01)) = 0.5
		_TopRange("TopBlendingRange",Range(0.01, 1)) = 0.5
		_HeightFactor("HeightFactor",Range(0, 10)) = 1
		_Glossiness("Smoothness", Range(0, 1)) = 0.5
		[Gamma] _Metallic("Metallic", Range(0, 1)) = 0
		_OcclusionStrength("OcclusionStrength", Range(0.0, 1.0)) = 1.0
	}
		SubShader{
		Tags{ "RenderType" = "Opaque" }
		LOD 200

		CGPROGRAM
			// Physically based Standard lighting model, and enable shadows on all light types
	#pragma surface surf Standard fullforwardshadows

				// Use shader model 3.0 target, to get nicer looking lighting
	#pragma target 3.0

	#include "UnityStandardUtils.cginc"

				// flip UVs horizontally to correct for back side projection
	#define TRIPLANAR_CORRECT_PROJECTED_U

				// offset UVs to prevent obvious mirroring
				// #define TRIPLANAR_UV_OFFSET

				// hack to work around the way Unity passes the tangent to world matrix to surface shaders to prevent compiler errors
	#if defined(INTERNAL_DATA) && (defined(UNITY_PASS_FORWARDBASE) || defined(UNITY_PASS_FORWARDADD) || defined(UNITY_PASS_DEFERRED) || defined(UNITY_PASS_META))
	#define WorldToTangentNormalVector(data,normal) mul(normal, half3x3(data.internalSurfaceTtoW0, data.internalSurfaceTtoW1, data.internalSurfaceTtoW2))
	#else
	#define WorldToTangentNormalVector(data,normal) normal
	#endif

				// Reoriented Normal Mapping
				// http://blog.selfshadow.com/publications/blending-in-detail/
				// Altered to take normals (-1 to 1 ranges) rather than unsigned normal maps (0 to 1 ranges)
				half3 blend_rnm(half3 n1, half3 n2)
			{
				n1.z += 1;
				n2.xy = -n2.xy;

				return n1 * dot(n1, n2) / n1.z - n2;
			}

			sampler2D _MainTex;
			//float4 _MainTex_ST;

			sampler2D _BumpMap;
			sampler2D _HeightMap;


			sampler2D _MainTexUp;
			float4 _MainTexUp_ST;

			sampler2D _BumpMapUp;
			sampler2D _HeightMapUp;


			half _TopFactor;
			half _TopRange;
			half _HeightFactor;
			half _Glossiness;
			half _Metallic;

			half _OcclusionStrength;


			struct Input {
				float3 worldPos;
				float3 worldNormal;
				float2 uv_MainTex;
				INTERNAL_DATA
			};

			void surf(Input IN, inout SurfaceOutputStandard o) {
				// work around bug where IN.worldNormal is always (0,0,0)!
				IN.worldNormal = WorldNormalVector(IN, float3(0,0,1));

				// calculate triplanar blend
				half3 triblend = saturate(pow(IN.worldNormal, 4));
				triblend /= max(dot(triblend, half3(1,1,1)), 0.0001);

				//only apply top texture to top part
				bool top = dot(IN.worldNormal, float3(0, 1, 0)) > _TopFactor;

				// calculate triplanar uvs
				// applying texture scale and offset values ala TRANSFORM_TEX macro
				float2 uvXTop = IN.worldPos.zy * _MainTexUp_ST.xy + _MainTexUp_ST.zy;
				float2 uvYTop = IN.worldPos.xz * _MainTexUp_ST.xy + _MainTexUp_ST.zy;
				float2 uvZTop = IN.worldPos.xy * _MainTexUp_ST.xy + _MainTexUp_ST.zy;

				//Now i can evaluate the height map in order to blend with the height maps. TODO:Could also add some other noise?????
				half topBlend = clamp(clamp((dot(IN.worldNormal, float3(0, 1, 0)) - _TopFactor), -_TopRange, _TopRange) / _TopRange ,0,1);

				if (topBlend != 0 && topBlend != 1)
				{
					//topBlend = tex2D(_HeightMapUp, uvXTop) *(1- topBlend) + tex2D(_HeightMap, uvX) * (topBlend);
					//float height = (tex2D(_HeightMap, uvX) * triblend.x + tex2D(_HeightMap, uvY)* triblend.y + tex2D(_HeightMap, uvZ) * triblend.z) * (1 - topBlend);
					float height = tex2D(_HeightMap, IN.uv_MainTex) * (1 - topBlend);
					float heightTop = (tex2D(_HeightMapUp, uvXTop) * triblend.x + tex2D(_HeightMapUp, uvYTop) * triblend.y + tex2D(_HeightMapUp, uvZTop) * triblend.z) * topBlend;
					topBlend = clamp( topBlend + (heightTop - height) *_HeightFactor, 0 ,1);
				}


				// offset UVs to prevent obvious mirroring
	#if defined(TRIPLANAR_UV_OFFSET)
				uvYTop += 0.33;
				uvZTop += 0.67;
	#endif

				// minor optimization of sign(). prevents return value of 0
				half3 axisSign = IN.worldNormal < 0 ? -1 : 1;

				// flip UVs horizontally to correct for back side projection
	#if defined(TRIPLANAR_CORRECT_PROJECTED_U)
				uvXTop.x *= axisSign.x;
				uvYTop.x *= axisSign.y;
				uvZTop.x *= -axisSign.z;
	#endif
				
				//albedo texture

				fixed4 colX = tex2D(_MainTexUp, uvXTop);
				fixed4 colY = tex2D(_MainTexUp, uvYTop);
				fixed4 colZ = tex2D(_MainTexUp, uvZTop);
				fixed4 col = (colX * triblend.x + colY * triblend.y + colZ * triblend.z)*topBlend + tex2D(_MainTex, IN.uv_MainTex) * (1-topBlend);

				//normal of top
				half3 tnormalX = UnpackNormal(tex2D(_BumpMapUp, uvXTop));
				half3 tnormalY = UnpackNormal(tex2D(_BumpMapUp, uvYTop));
				half3 tnormalZ = UnpackNormal(tex2D(_BumpMapUp, uvZTop));


				// flip normal maps' x axis to account for flipped UVs
	#if defined(TRIPLANAR_CORRECT_PROJECTED_U)
				tnormalX.x *= axisSign.x;
				tnormalY.x *= axisSign.y;
				tnormalZ.x *= -axisSign.z;
	#endif

				half3 absVertNormal = abs(IN.worldNormal);

				// swizzle world normals to match tangent space and apply reoriented normal mapping blend
				tnormalX = blend_rnm(half3(IN.worldNormal.zy, absVertNormal.x), tnormalX);
				tnormalY = blend_rnm(half3(IN.worldNormal.xz, absVertNormal.y), tnormalY);
				tnormalZ = blend_rnm(half3(IN.worldNormal.xy, absVertNormal.z), tnormalZ);

				// apply world space sign to tangent space Z
				tnormalX.z *= axisSign.x;
				tnormalY.z *= axisSign.y;
				tnormalZ.z *= axisSign.z;

				// sizzle tangent normals to match world normal and blend together
				half3 worldNormal = normalize(
					tnormalX.zyx * triblend.x +
					tnormalY.xzy * triblend.y +
					tnormalZ.xyz * triblend.z
				);
				// convert world space normals into tangent normals
				half3 norm = UnpackNormal (tex2D (_BumpMap, IN.uv_MainTex))* (1-topBlend) + WorldToTangentNormalVector(IN, worldNormal) * topBlend;

				// set surface oUput properties
				o.Albedo = col.rgb;
	
				o.Metallic = _Metallic;
				o.Smoothness = _Glossiness;
				o.Occlusion = _OcclusionStrength;

				//renormalize normal again
				o.Normal = normalize(norm);
			}
			ENDCG
		}
			FallBack "Diffuse"
}
