Shader "Custom/CoffeePuddle"
{
    Properties
    {
        _CoffeeColor ("Coffee Color", Color) = (0.12, 0.035, 0.01, 1)
        _MainTex ("Puddle Mask", 2D) = "white" {}
        _Reveal ("Reveal", Range(0,1)) = 0
        _FlowSoftness ("Flow Softness", Range(0.001, 0.5)) = 0.08
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent"
            "RenderPipeline"="UniversalPipeline"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Back

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)

                float4 _CoffeeColor;
                float _Reveal;
                float _FlowSoftness;

            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;

                output.positionHCS =
                    TransformObjectToHClip(input.positionOS.xyz);

                output.uv = input.uv;

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float mask = SAMPLE_TEXTURE2D(
                    _MainTex,
                    sampler_MainTex,
                    input.uv
                ).r;

                // The reveal always sweeps along U. CoffeeFlowController rotates
                // the quad so U points in the mug's spill direction.
                float revealMask = 1.0 - smoothstep(
                    _Reveal - _FlowSoftness,
                    _Reveal + _FlowSoftness,
                    input.uv.x
                );

                return half4(
                    _CoffeeColor.rgb,
                    mask * revealMask * _CoffeeColor.a
                );
            }

            ENDHLSL
        }
    }
}
