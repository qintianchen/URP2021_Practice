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

                float3 viewPositionWS = float3(0, 0, 0);

                AtmosphereParams atmosphereParams = _AtmosphereParamses[0];
                Light mainLight = GetMainLight();

                float distance1;
                float tempFloat;
                GetIntersectPointsWithSphere(viewPositionWS, viewDirWS, float3(0, -atmosphereParams.planetRadius, 0), atmosphereParams.planetRadius, distance1, tempFloat);

                float sampleCount = 32;
                float stepLength = distance1 / sampleCount;

                float viewDotUp = dot(viewDirWS, float3(0, 1, 0));

                float totalAttenuationFromScattering = 0;
                for (int i = 0; i < sampleCount; i++)
                {
                    float curDistance = stepLength * (i + 0.5);

                    float height = TriangleCosineLaw(curDistance, atmosphereParams.planetRadius, -viewDotUp) - atmosphereParams.planetRadius; // 海拔高度
                    float cos_theta = dot(mainLight.direction, float3(0, 1, 0));
                    float t1 = GetTransmittanceFromLut(atmosphereParams, height, cos_theta, _TransmittanceLut);

                    float cos_theta2 = dot(viewDirWS, float3(0, 1, 0));
                    float t20 = GetTransmittanceFromLut(atmosphereParams, 0, cos_theta2, _TransmittanceLut);
                    float t21 = GetTransmittanceFromLut(atmosphereParams, height, cos_theta2, _TransmittanceLut);
                    float t2 = t20 / t21;

                    float cos_theta3 = dot(viewDirWS, mainLight.direction);
                    float s = GetRayleighScattering(atmosphereParams, cos_theta3, height) + GetMieScattering(atmosphereParams, cos_theta3, height);

                    totalAttenuationFromScattering += s; // * t1 * t2;
                }

                // return half4(viewDirWS, 1);
                return Test(viewDirWS);
                return half4(mainLight.color * 2 * totalAttenuationFromScattering, 1);
            }
            ENDHLSL
        }
    }
}