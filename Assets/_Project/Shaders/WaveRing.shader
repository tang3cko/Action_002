Shader "Action002/WaveRing"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1, 1, 1, 0.8)
        _RingRadius ("Ring Radius (normalized)", Float) = 0.5
        _RingThickness ("Ring Thickness (normalized)", Float) = 0.05
        _ArcCenter ("Arc Center Angle (rad)", Float) = 0.0
        _ArcSpread ("Arc Half Spread (rad)", Float) = 3.14159
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
            Blend One One // Additive

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
                // UV center (0.5, 0.5) to normalized distance (0 to 1 across half-size)
                float2 centered = input.uv - 0.5;
                float dist = length(centered) * 2.0; // 0..1 maps to 0..meshHalfSize

                // Ring test: distance from ring center line
                float ringDist = abs(dist - _RingRadius);
                float halfThickness = _RingThickness * 0.5;

                if (ringDist > halfThickness)
                    discard;

                // Arc test (skip for full circle where ArcSpread >= PI)
                if (_ArcSpread < PI - 0.001)
                {
                    float angle = atan2(centered.y, centered.x);
                    float delta = angle - _ArcCenter;
                    // Normalize to [-PI, PI]
                    delta = delta - 2.0 * PI * floor((delta + PI) / (2.0 * PI));
                    if (abs(delta) > _ArcSpread)
                        discard;
                }

                // Smooth edge falloff
                float edgeFade = 1.0 - saturate(ringDist / halfThickness);
                edgeFade = edgeFade * edgeFade; // quadratic falloff

                half4 col = _BaseColor;
                col.a *= edgeFade;
                col.rgb *= col.a; // premultiplied alpha for additive

                return col;
            }
            ENDHLSL
        }
    }
    FallBack Off
}
