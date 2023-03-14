Shader "AtmosphereScattering/Skybox"
{
    Properties
    {
    }
    SubShader
    {
        Tags
        {
        }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 screenUV : TEXCOORD2;
            };

            Texture2D _SkyViewLut;
            SamplerState sampler_SkyViewLut;
            
            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                float3 positionNDC = (output.positionCS / output.positionCS.w).xyz;
                output.screenUV = positionNDC.xy * 0.5 + 0.5;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float3 color = _SkyViewLut.Sample(sampler_SkyViewLut, input.screenUV);
                return half4(color, 1);
            }
            ENDHLSL
        }
    }
}