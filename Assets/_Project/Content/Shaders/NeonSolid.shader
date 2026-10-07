// Unlit "glowing outline" look for the runner, hazards and pickups. A dark fill with bright
// edges reads clearly at speed and needs no lights, no textures and no extra geometry.
//
// Box edges are found from object-space position, so they work on any scaled unit cube and
// keep a constant width in metres. Rounded meshes use the rim term instead.
Shader "NeonRush/NeonSolid"
{
    Properties
    {
        _FillColor ("Fill Color", Color) = (0.03, 0.04, 0.08, 1)
        [HDR] _GlowColor ("Glow Color", Color) = (0.2, 1.6, 2.4, 1)
        [Toggle] _BoxEdges ("Box Edges (unit cube meshes)", Float) = 1
        _EdgeWidth ("Edge Width (m)", Float) = 0.06
        _RimPower ("Rim Power", Range(0.5, 8)) = 2.5
        _RimStrength ("Rim Strength", Range(0, 2)) = 0.35
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }

        Pass
        {
            Name "Forward"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionOS : TEXCOORD0;
                float3 normalOS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float3 viewDirWS : TEXCOORD3;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _FillColor;
                half4 _GlowColor;
                float _BoxEdges;
                float _EdgeWidth;
                float _RimPower;
                float _RimStrength;
            CBUFFER_END

            Varyings Vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs positions = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = positions.positionCS;
                output.positionOS = input.positionOS.xyz;
                output.normalOS = input.normalOS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.viewDirWS = GetWorldSpaceViewDir(positions.positionWS);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half3 normalWS = normalize(input.normalWS);
                half3 viewDirWS = normalize(input.viewDirWS);
                half rim = pow(1.0 - saturate(dot(normalWS, viewDirWS)), _RimPower) * _RimStrength;

                // Distance in metres from this pixel to the nearest edge of the face it is on.
                float3 scale = float3(
                    length(UNITY_MATRIX_M._m00_m10_m20),
                    length(UNITY_MATRIX_M._m01_m11_m21),
                    length(UNITY_MATRIX_M._m02_m12_m22));
                float3 fromEdge = (0.5 - abs(input.positionOS)) * scale;
                fromEdge += abs(input.normalOS) * 1000.0; // the axis the face points along is not an edge
                float nearest = min(fromEdge.x, min(fromEdge.y, fromEdge.z));
                float edge = 1.0 - smoothstep(_EdgeWidth, _EdgeWidth + fwidth(nearest), nearest);

                half glow = saturate(max(edge * _BoxEdges, rim));
                half3 color = lerp(_FillColor.rgb, _GlowColor.rgb, glow);
                return half4(color, 1.0);
            }
            ENDHLSL
        }
    }
}
