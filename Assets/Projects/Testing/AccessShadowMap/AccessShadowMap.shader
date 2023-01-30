Shader "Custom/Testing/AccessShadowMap"
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
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS             // 重要
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE     // 重要

            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS: TEXCOORD1;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            sampler2D _CameraDepthTexture;

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float depth = tex2D(_CameraDepthTexture, input.uv);
                float depthVS = LinearEyeDepth(depth, _ZBufferParams);

                float4 NDC = float4(input.uv * 2 - 1, depth * 2 - 1, 1);
                float4 positionCS = NDC * depthVS;
                float4 positionWS = mul(UNITY_MATRIX_I_VP, positionCS);

                float4 shadowCoord = TransformWorldToShadowCoord(positionWS);   // 重要
                half shadow = MainLightRealtimeShadow(shadowCoord);             // 重要

                return shadow;
            }
            ENDHLSL
        }
    }
}