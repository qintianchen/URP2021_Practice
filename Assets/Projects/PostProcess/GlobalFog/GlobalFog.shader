Shader "Custom/PostProcess/GlobalFog"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque" "LightMode"="UniversalForward"
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
                float2 uv : TEXCOORD0;
                float2 uvDepth : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;

            sampler2D _CameraDepthTexture;

            float3 _FogColor;
            float _FogHeightMin;
            float _FogHeightMax;
            float _FogDepth;

            float4x4 _Inverse_VP;

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.uvDepth = output.uv;
                
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float depth = tex2D(_CameraDepthTexture, input.uvDepth).r;
                float depthVS = LinearEyeDepth(depth, _ZBufferParams);

                float4 NDC = float4(input.uv.x * 2 - 1, input.uv.y * 2 - 1, depth, 1);   
                float4 positionCS = NDC * depthVS;
                float4 positionWS = mul(_Inverse_VP, positionCS);
                positionWS /= positionWS.w;

                float heightWS = positionWS.y;

                float heightRatio = (heightWS - _FogHeightMin) / (_FogHeightMax - _FogHeightMin);
                float depthRatio = depthVS / _FogDepth;

                heightRatio = saturate(heightRatio);
                depthRatio = saturate(depthRatio);

                float3 finalColor = tex2D(_MainTex, input.uv).xyz;
                finalColor = lerp(_FogColor, finalColor, heightRatio);
                finalColor = lerp(finalColor, _FogColor, depthRatio);
                
                return half4(finalColor, 1);
            }
            ENDHLSL
        }
    }
}