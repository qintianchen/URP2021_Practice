Shader "Custom/PostProcessing/AtmosphereScatteringSkybox"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Background" "RenderType"="Background" "PreviewType"="Skybox"
        }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "AtmosphereScatteringUtils.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 positionNDC: TEXCOORD1;
                float4 nearPlaneVector: TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            // 从外部传入的值
            sampler2D _TransmittanceLut;
            float4x4 _NearPlaneVectors;
            float _FOV;
            float _Aspect;

            float3 ScreenQuadUVtoViewDirWS(float2 screenQuadUV, float fov, float aspect)
            {
                float near = _ProjectionParams.y;

                float height = 2 * near * tan(fov / 2);
                float width = aspect * height;

                float3 toCenter = float3(0, 0, near); // 在 Unity（OpenGL传统） 下，相机坐标系的 z 指向后方
                float3 toUp = float3(0, height / 2, 0);
                float3 toRight = float3(width / 2, 0, 0);

                float3 viewDirCS = toCenter + toUp * (screenQuadUV.y - 0.5) * 2 + toRight * (screenQuadUV.x - 0.5) * 2;
                float3 viewDirWS = mul(unity_CameraToWorld, viewDirCS);

                return viewDirWS;
            }

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.positionNDC = output.positionCS / output.positionCS.w;
                return output;
            }

            float4 Test(float3 viewDirWS)
            {
                float3 position = float3(0, 0, 0);
                float sphereRadius = 1.414213562373;
                float spherePosition = float3(0, -1, 0);

                float distance;
                float tempFloat;
                GetIntersectPointsWithSphere(position, viewDirWS, spherePosition, sphereRadius, distance, tempFloat);

                return distance;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 screenQuadUV = input.positionNDC.xy * 0.5 + 0.5; // [0, 1]
                float3 viewDirWS = ScreenQuadUVtoViewDirWS(screenQuadUV, _FOV, _Aspect);
                viewDirWS = normalize(viewDirWS);

                float3 cameraPositionWS = _WorldSpaceCameraPos;
                float3 viewPositionWS = float3(0, cameraPositionWS.y, 0);

                AtmosphereParams atmosphereParams = _AtmosphereParamses[0];
                Light mainLight = GetMainLight();
                float3 mainLightDirection = normalize(mainLight.direction);

                float tempFloat;
                float tempFloat2;
                int countIntersectWithPlanet = GetIntersectPointsWithSphere(viewPositionWS, viewDirWS, float3(0, -atmosphereParams.planetRadius, 0), atmosphereParams.planetRadius, tempFloat, tempFloat2);
                if(countIntersectWithPlanet > 0)
                {
                    return 0;
                }

                float distance1;
                GetIntersectPointsWithSphere(viewPositionWS, viewDirWS, float3(0, -atmosphereParams.planetRadius, 0), atmosphereParams.planetRadius + atmosphereParams.atmosphereHeight, distance1, tempFloat);

                float sampleCount = 32;
                float stepLength = distance1 / sampleCount;

                float viewDotUp = dot(viewDirWS, float3(0, 1, 0));
                float viewDotLight = dot(viewDirWS, mainLightDirection);

                float3 finalColor = 0;
                for (int i = 0; i < sampleCount; i++)
                {
                    float curDistance = stepLength * (i + 0.5);
                    float3 curPositionWS = viewPositionWS + viewDirWS * curDistance;
                    float4 shadowCoord = TransformWorldToShadowCoord(curPositionWS);
                    float shadow = MainLightRealtimeShadow(shadowCoord);
                    if(shadow > 0.99)
                    {
                        float height = length(float3(0, atmosphereParams.planetRadius, 0) + curDistance * viewDirWS) - atmosphereParams.planetRadius; // 海拔高度
                        float3 centerToCurPoint = float3(0, atmosphereParams.planetRadius, 0) + viewDirWS * curDistance; // 从地心指向当前考察点的向量
                        float3 centerToCurPoint_normalized = normalize(centerToCurPoint);
                        float cos_theta1 = dot(mainLightDirection, centerToCurPoint_normalized);
                        float3 t1 = GetTransmittanceFromLut(atmosphereParams, height, cos_theta1, _TransmittanceLut);
                        
                        float3 t20 = GetTransmittanceFromLut(atmosphereParams, viewPositionWS.y, viewDotUp, _TransmittanceLut);
                        float3 t21 = GetTransmittanceFromLut(atmosphereParams, height, dot(viewDirWS, centerToCurPoint_normalized), _TransmittanceLut);
                        float3 t2 = t20 / t21;

                        float3 s = GetRayleighScattering(atmosphereParams, viewDotLight, height) + GetMieScattering(atmosphereParams, viewDotLight, height);

                        finalColor += mainLight.color * 32 * t1 * t2 * (s * stepLength);
                    }
                }

                if(viewDotLight > 0.9999f)
                {
                    finalColor += 0.5 * viewDotLight;
                }
                
                finalColor = pow(finalColor, 2.2);
                return half4(finalColor, 1);
            }
            ENDHLSL
        }
    }
}