Shader "Custom/PostProcess/VolumetricLight"
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

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            
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
                float2 uv : TEXCOORD0;
                float4 nearPlaneVector: TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            sampler2D _CameraDepthTexture;

            float _VolumetricLightIntensity;
            float4x4 _NearPlaneVectors; // 从相机指向近平面四个角的向量，世界空间，左上，右上，左下，右下
            
            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                if (output.uv.x < 0.5 && output.uv.y > 0.5)
                {
                    output.nearPlaneVector = _NearPlaneVectors[0];
                }
                else if (output.uv.x > 0.5 && output.uv.y > 0.5)
                {
                    output.nearPlaneVector = _NearPlaneVectors[1];
                }
                else if (output.uv.x < 0.5 && output.uv.y < 0.5)
                {
                    output.nearPlaneVector = _NearPlaneVectors[2];
                }
                else
                {
                    output.nearPlaneVector = _NearPlaneVectors[3];
                }
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float4 nearPlaneVector = input.nearPlaneVector;
                
                float depth = tex2D(_CameraDepthTexture, input.uv);
                float depthVS = LinearEyeDepth(depth, _ZBufferParams);
                float3 cameraPositionWS = _WorldSpaceCameraPos;

                float3 srcPositionWS = cameraPositionWS + nearPlaneVector;
                float3 destPositionWS = cameraPositionWS + nearPlaneVector * depthVS / _ProjectionParams.y;

                float sampleCount = 256;
                float3 step = (destPositionWS - srcPositionWS) / sampleCount;
                float stepLength = length(step);
                half3 finalColor = tex2D(_MainTex, input.uv);
                Light mainLight = GetMainLight();
                float3 mainLightColor = mainLight.color;
                float intensity = _VolumetricLightIntensity / 100.0;
                for (int i = 0; i < sampleCount; i++)
                {
                    float3 curPositionWS = srcPositionWS + step * i;
                    float4 shadowCoord = TransformWorldToShadowCoord(curPositionWS);
                    float shadow = MainLightRealtimeShadow(shadowCoord);
                    if(shadow >= 0.5)
                    {
                        finalColor += mainLightColor * stepLength * intensity;
                    }
                }
                                
                return half4(finalColor, 1);
            }
            ENDHLSL
        }
    }
}