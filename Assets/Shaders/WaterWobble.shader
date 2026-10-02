Shader "Custom/WaterWobble"
{
    Properties
    {
        _BaseColor ("Water Color", Color) = (0.03, 0.25, 0.45, 0.65)
        _WaveHeight ("Wave Height", Range(0, 0.1)) = 0
        _WaveSpeed ("Wave Speed", Range(0, 10)) = 0
        _WaveScale ("Wave Scale", Range(0.1, 20)) = 4
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent"
            "RenderPipeline"="UniversalPipeline"
        }

        Pass
        {
            Tags
            {
                "LightMode"="UniversalForward"
            }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)

                float4 _BaseColor;
                float _WaveHeight;
                float _WaveSpeed;
                float _WaveScale;

            CBUFFER_END


            Varyings vert(Attributes input)
            {
                Varyings output;

                float3 worldPos = TransformObjectToWorld(input.positionOS.xyz);

                float time = _Time.y * _WaveSpeed;

                // Crossed sine waves. _WaveHeight of 0 zeroes this out below, so
                // no branch is needed here.
                float wave =
                    (sin( worldPos.x * _WaveScale + time ) +
                     sin( worldPos.z * _WaveScale * 1.3 - time * 0.8 )) * 0.5;

                // Only displace the top surface, not the sides.
                float topMask =
                    smoothstep(
                        0.1,
                        0.5,
                        input.positionOS.y
                    );

                worldPos.y += wave * _WaveHeight * topMask;

                output.positionHCS =
                    TransformWorldToHClip(worldPos);

                output.normalWS =
                    TransformObjectToWorldNormal(
                        input.normalOS
                    );

                return output;
            }


            half4 frag(Varyings input) : SV_Target
            {
                float3 normal =
                    normalize(input.normalWS);

                float lighting =
                    saturate(
                        dot(
                            normal,
                            normalize(float3(0.3, 1.0, 0.2))
                        )
                    );

                float3 color =
                    _BaseColor.rgb *
                    (0.65 + lighting * 0.35);

                return half4(
                    color,
                    _BaseColor.a
                );
            }

            ENDHLSL
        }
    }
}
