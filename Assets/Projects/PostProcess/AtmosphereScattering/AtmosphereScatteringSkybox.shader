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

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
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
                float4 nearPlaneVector: TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            sampler2D _CameraDepthTexture;

            float4x4 _NearPlaneVectors;

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
                // float depth = tex2D(_CameraDepthTexture, input.uv);
                // float depthVS = LinearEyeDepth(depth, _ZBufferParams);

                // float3 cameraPositionWS = _WorldSpaceCameraPos;
                // float3 positionWS = cameraPositionWS + input.nearPlaneVector.xyz * depthVS;
                float3 viewDirWS = normalize(input.nearPlaneVector.xyz);

                AtmosphereParams params;
                params.planetRadius = 6400;
                params.atmosphereHeight = 5600; // ???
                params.rayleighScattering_h0 = float3(5.802, 13.558, 33.1) * 1E-6;
                params.rayleighHeight = 8500;
                params.mieScattering_h0 = (3.996).xxx * 1E-6;
                params.mieHeight = 1200;
                params.mieAnisotropy = 0.6;
                params.mieAbsorption = (4.4).xxx * 1E-6;
                params.ozoneAbsorption = float3(0.65, 1.881, 0.085) * 1E-6;

                float3 viewPositionWS = float3(0, 0, 0);
                
                float distance1;
                float tempFloat;
                GetIntersectPointsWithSphere(viewPositionWS, viewDirWS, float3(0, -params.planetRadius, 0), params.planetRadius + params.atmosphereHeight, distance1, tempFloat);

                float sampleCount = 32;
                float stepLength = distance1 / sampleCount;
                Light mainLight = GetMainLight();
                float3 lightDirWS = mainLight.direction;
                for (int i = 0; i < sampleCount; i++)
                {
                    float3 curPoint = viewPositionWS + stepLength * (i + 0.5);
                    float distanceFromCurPoint;
                    GetIntersectPointsWithSphere(curPoint, lightDirWS, float3(0, -params.planetRadius, 0), params.planetRadius + params.atmosphereHeight, distanceFromCurPoint, tempFloat);
                }

                half4 finalColor = tex2D(_MainTex, input.uv);
                return finalColor;
            }
            ENDHLSL
        }
    }
}