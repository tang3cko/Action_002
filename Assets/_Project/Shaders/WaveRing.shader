Shader "Action002/WaveRing"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1, 1, 1, 0.8)
        _RingRadius ("Ring Radius (normalized)", Float) = 0.5
        _RingThickness ("Ring Thickness (normalized)", Float) = 0.05
        _ArcCenter ("Arc Center Angle (rad)", Float) = 0.0
        _ArcSpread ("Arc Half Spread (rad)", Float) = 3.14159
        _NormalizedTime ("Normalized Time (0-1)", Float) = 0.0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
        }

        Pass
        {
            Name "WaveRing"
            Tags { "LightMode" = "Universal2D" }

            ZWrite Off
            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha // Alpha Blend

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
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float _RingRadius;
                float _RingThickness;
                float _ArcCenter;
                float _ArcSpread;
                float _NormalizedTime;
            CBUFFER_END

            #define PI 3.14159265

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float t = saturate(_NormalizedTime);

                // Ring thickness thins as wave expands (energy conservation)
                // At t=0: full thickness. At t=1: 30% of original.
                float thicknessMul = 1.0 - t * 0.7;
                float effectiveThickness = _RingThickness * thicknessMul;

                // UV center (0.5, 0.5) to normalized distance (0 to 1 across half-size)
                float2 centered = input.uv - 0.5;
                float dist = length(centered) * 2.0;

                // Ring test with dynamic thickness
                float ringDist = abs(dist - _RingRadius);
                float halfThickness = effectiveThickness * 0.5;

                if (ringDist > halfThickness)
                    discard;

                // Arc test (skip for full circle where ArcSpread >= PI)
                if (_ArcSpread < PI - 0.001)
                {
                    float angle = atan2(centered.y, centered.x);
                    float delta = angle - _ArcCenter;
                    delta = delta - 2.0 * PI * floor((delta + PI) / (2.0 * PI));
                    if (abs(delta) > _ArcSpread)
                        discard;
                }

                // Smooth edge falloff
                float edgeFade = 1.0 - saturate(ringDist / halfThickness);
                edgeFade = edgeFade * edgeFade;

                // Leading-edge brightening: outer half of ring glows brighter early on
                float leadingEdge = saturate((dist - _RingRadius) / max(halfThickness, 0.001) + 0.5);
                float leadingGlow = leadingEdge * (1.0 - t) * 0.4;

                // Dissipation: alpha fades out as wave expands
                float dissipation = 1.0 - smoothstep(0.4, 1.0, t);

                half4 col = _BaseColor;
                col.rgb += col.rgb * leadingGlow;
                col.a *= edgeFade * dissipation;

                return col;
            }
            ENDHLSL
        }
    }
    FallBack Off
}
