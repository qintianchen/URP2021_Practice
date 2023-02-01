Shader "Custom/Testing/ConstInHLSL"
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
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half4 finalColor = tex2D(_MainTex, input.uv);
                const float sampleCount = 256;

                for (int i=0;i<sampleCount;i++)
                {
                    const float a1 = cos(i);
                    const float a2 = sqrt(abs(a1));
                    const float a3 = i * 2;
                    const float a4 = i / 20.0;
                    const float a5 = sin(a3);
                    const float a6 = exp(a4);
                    const float a7 = 0.1;
                    const float a8 = -0.01;

                    const float4 b1 = a2 * a5 * a6 * a7;
                    const float4 b2 = a2 * a5 * a6 * a8;
                    
                    if(a1 < 0)
                    {
                        finalColor += abs(b1) ;
                    }
                    else
                    {
                        finalColor -= abs(b2);
                    }
                }
                
                return half4(finalColor.xyz, 1);
            }
            ENDHLSL
        }
    }
}