Shader "Hidden/Custom/Grayscale"
{
    HLSLINCLUDE

        #include "PostProcessing/Shaders/StdLib.hlsl"
		#pragma target 5.0

        TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
        float _Factor;
		float _Black;
		float _DistFactor;
		float _DistFactor2;
		float _a;
		float _b;
		float _c;


		StructuredBuffer<float4> buffer;

        float4 Frag(VaryingsDefault i) : SV_Target
        {
            float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);
            float luminance = dot(color.rgb, float3(0.2126729, 0.7151522, 0.0721750))*0.1 + _Black;
			float blend =0;
			uint count = 0;
			uint test2 = 1;
			buffer.GetDimensions(count,test2);

			float dist = 0;
			float tmp = 0;
			uint sum = 0;

			for(uint j =0; j < count ; j++)
			{
				float4 entry = buffer[j];
				if(entry.b < 0 || entry.a == 0)
				{
					continue;
				}

				float2 len = (i.texcoord - entry.rg) * _DistFactor2;
				len.x *= 1.7777;

				//dist = distance (i.texcoord , buffer[j].rg);
				dist = length(len);

				dist = dist *  buffer[j].b * _DistFactor;
				//dist = 1/(pow(dist, 2)*100);
				dist = pow(_a, -(pow(dist - _b,2)/(pow(_c,2))));
				//dist = log2(1.3-dist);
				tmp = clamp(dist* entry.a * _Factor ,0,1);
				if(tmp > 0.005)
				{
					sum +=1;
					blend+= tmp;
				}
				
				//blend += clamp(pow(1- distance (i.texcoord , buffer[j].rg)*1,2),0,1) * buffer[j].a * _Factor;
			}

			//_Blend = clamp(pow(1- distance (i.texcoord , buffer[abc].rg)*1 + 0.05,2),0,1);


			//0,1,0 is up left
			//1,0,0 is down right
			//since i.texcoord is only UV ... b is useless
			//could use it to either to encode distance to camera or encode the factor

			//_Blend = buffer[0].r;
			//_Blend = buffer[0].a;
			
			
			

			//_Blend = length(i.texcoord);
			//lor = buffer[0].pos.rgb;
			
            
			//color.rgb = color.rgb;
			
			if(sum != 0)
			{
				//blend /= sum;
			}
			blend = clamp(blend,0,0.97);
			blend = log2( blend+1);

			color.rgb = lerp(color.rgb, float3(0,0,0), blend.xxx);
			color.rgb = lerp(color.rgb, luminance, blend.xxx);
			//color.rgb = lerp(color.rgb, -0.05, blend.xxx);
			//color.rgb = lerp(float3(1,1,1), float3(0,0,0), 0);
			//color.rgb = float3(1,blend,blend);
			

            return color;
        }

    ENDHLSL

    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            HLSLPROGRAM

                #pragma vertex VertDefault
                #pragma fragment Frag

            ENDHLSL
        }
    }
}