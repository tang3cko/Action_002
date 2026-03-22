Shader "Action002/TitleBackground"
{
    Properties
    {
        _DarkColor ("Dark Color", Color) = (0.082, 0.082, 0.090, 1)
        _LightColor ("Light Color", Color) = (0.941, 0.933, 0.902, 1)
        _Speed ("Speed", Float) = 0.04
        _Scale ("Scale", Float) = 3.0
        _Distortion ("Distortion", Float) = 1.0
        _BoundaryWidth ("Boundary Width", Float) = 0.15
        _TendrilStrength ("Tendril Strength", Float) = 0.6
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Background-1"
        }

        Pass
        {
            Name "TitleBackground"
            Tags { "LightMode" = "UniversalForward" }

            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

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

            CBUFFER_START(UnityPerMaterial)
                float4 _DarkColor;
                float4 _LightColor;
                float _Speed;
                float _Scale;
                float _Distortion;
                float _BoundaryWidth;
                float _TendrilStrength;
            CBUFFER_END

            float hash(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f);

                return lerp(
                    lerp(hash(i), hash(i + float2(1.0, 0.0)), u.x),
                    lerp(hash(i + float2(0.0, 1.0)), hash(i + float2(1.0, 1.0)), u.x),
                    u.y);
            }

            float fbm(float2 p)
            {
                float v = 0.0;
                float a = 0.5;
                float2 shift = float2(100.0, 100.0);
                float cosR = cos(0.5);
                float sinR = sin(0.5);
                float2x2 rot = float2x2(cosR, sinR, -sinR, cosR);

                for (int idx = 0; idx < 4; idx++)
                {
                    v += a * noise(p);
                    p = mul(rot, p) * 2.0 + shift;
                    a *= 0.5;
                }

                return v;
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv;
                float t = _Time.y * _Speed;
                float2 p = uv * _Scale;

                // Domain warping layer 1
                float2 q;
                q.x = fbm(p + t * 0.3);
                q.y = fbm(p + float2(1.0, 1.0) + t * 0.2);

                // Domain warping layer 2
                float2 r;
                r.x = fbm(p + 1.2 * q + float2(1.7, 9.2) + 0.15 * t);
                r.y = fbm(p + 1.2 * q + float2(8.3, 2.8) + 0.126 * t);

                // Domain warping layer 3
                float2 s;
                s.x = fbm(p + 1.5 * r + float2(3.1, 5.7) + 0.1 * t);
                s.y = fbm(p + 1.5 * r + float2(6.2, 1.3) + 0.08 * t);

                float f = fbm(p + r * 1.5 + s * 0.8);

                // Yin-yang split boundary with warping
                float boundary = uv.x + (q.x - 0.5) * 0.4 + (r.y - 0.5) * 0.3;

                // Swirl pattern intensity
                float swirl = f * f * 4.0;
                float flow = length(q) * 0.8;
                float detail = dot(r, r) * 0.5;

                // Dark side: base color with subtle lighter swirls
                float3 darkBase = _DarkColor.rgb;
                float3 darkSwirl = lerp(darkBase, darkBase * 1.8, swirl * 0.3);
                darkSwirl = lerp(darkSwirl, float3(0.12, 0.12, 0.15), flow * 0.2);
                darkSwirl = lerp(darkSwirl, darkBase * 0.6, detail * 0.15);

                // Light side: base color with subtle darker swirls
                float3 lightBase = _LightColor.rgb;
                float3 lightSwirl = lerp(lightBase, lightBase * 0.75, swirl * 0.25);
                lightSwirl = lerp(lightSwirl, float3(0.85, 0.84, 0.82), flow * 0.3);
                lightSwirl = lerp(lightSwirl, lightBase * 0.9, detail * 0.15);

                // Boundary zone with ink bleeding
                float bw = _BoundaryWidth + (f - 0.5) * 0.1;
                float boundaryMix = smoothstep(-bw, bw, boundary - 0.5);

                // Ink tendrils at the boundary
                float tendrils = fbm(p * 4.0 + r * 3.0 + t * 0.5);
                float darkTendrils = smoothstep(0.35, 0.65, tendrils)
                    * (1.0 - abs(boundary - 0.5) * 3.0);
                darkTendrils = max(0.0, darkTendrils);

                // Mix dark and light sides
                float3 col = lerp(darkSwirl, lightSwirl, boundaryMix);

                // Add ink bleeding tendrils
                col = lerp(col, darkBase * 0.8,
                    darkTendrils * _TendrilStrength * (1.0 - boundaryMix));
                col = lerp(col, lightBase * 1.1,
                    darkTendrils * (_TendrilStrength * 0.5) * boundaryMix);

                // Simplified vortex highlight near boundary
                float vortexMask = exp(-pow((boundary - 0.5) * 4.0, 2.0));
                col = lerp(col, lerp(darkBase, lightBase, f), vortexMask * 0.1);

                return float4(col, 1.0);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
