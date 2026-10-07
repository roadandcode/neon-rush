// The track floor. One quad, no texture: the grid is computed per pixel from world position,
// so it stays sharp at any distance, and scrolling it is a single float.
Shader "NeonRush/NeonGrid"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.02, 0.03, 0.06, 1)
        [HDR] _LineColor ("Line Color", Color) = (0.1, 0.9, 2.2, 1)
        _CellSize ("Cell Size (m)", Float) = 2.5
        _LineWidth ("Line Width (m)", Float) = 0.07
        _Scroll ("Scroll (m)", Float) = 0
        _FadeStart ("Fade Start (m)", Float) = 35
        _FadeEnd ("Fade End (m)", Float) = 110
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
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half4 _LineColor;
                float _CellSize;
                float _LineWidth;
                float _Scroll;
                float _FadeStart;
                float _FadeEnd;
            CBUFFER_END

            Varyings Vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs positions = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = positions.positionCS;
                output.positionWS = positions.positionWS;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                // In cell units. X is shifted half a cell so lines fall on lane edges, not lane centres.
                float2 cell = float2(input.positionWS.x / _CellSize + 0.5, (input.positionWS.z + _Scroll) / _CellSize);

                // Distance to the nearest grid line, in metres, with a one-pixel soft edge.
                float2 toLine = abs(frac(cell - 0.5) - 0.5) * _CellSize;
                float2 pixel = fwidth(cell) * _CellSize;
                float2 lines = 1.0 - smoothstep(_LineWidth * 0.5, _LineWidth * 0.5 + pixel, toLine);
                float grid = max(lines.x, lines.y);

                // Fade into the background colour before either end of the quad, so there is no horizon line
                // ahead of the runner and none behind it when the title-screen camera looks back down the track.
                float fade = 1.0 - smoothstep(_FadeStart, _FadeEnd, abs(input.positionWS.z));

                half3 color = lerp(_BaseColor.rgb, _LineColor.rgb, grid * fade);
                return half4(color, 1.0);
            }
            ENDHLSL
        }
    }
}
