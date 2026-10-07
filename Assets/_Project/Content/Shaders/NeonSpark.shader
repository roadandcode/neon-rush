// Particles for impacts and pickups. Additive and unlit, with the dot drawn from the quad's own
// UVs, so there is no texture and overlapping sparks add up to a hotter core that the bloom
// pass then spreads.
Shader "NeonRush/NeonSpark"
{
    Properties
    {
        [HDR] _Tint ("Tint", Color) = (2, 2, 2, 1)
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" "IgnoreProjector" = "True" }

        Pass
        {
            Name "Forward"
            Tags { "LightMode" = "UniversalForward" }

            Blend One One
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                half4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                half4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _Tint;
            CBUFFER_END

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.color = input.color;
                output.uv = input.uv;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                // Bright in the middle of the quad, nothing at its edge. Squared, so the core is small and hot.
                half falloff = saturate(1.0 - length(input.uv - 0.5) * 2.0);
                falloff *= falloff;

                // The particle's alpha is its fade-out; with additive blending that is just a scale.
                half3 color = _Tint.rgb * input.color.rgb * (falloff * input.color.a);
                return half4(color, 1.0);
            }
            ENDHLSL
        }
    }
}
