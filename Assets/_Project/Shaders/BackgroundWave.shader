Shader "Action002/BackgroundWave"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.102, 0.102, 0.180, 1)
        _WaveColor ("Wave Color", Color) = (0.15, 0.15, 0.22, 1)
        _WaveIntensity ("Wave Intensity", Range(0, 0.3)) = 0.08
        _Speed ("Speed", Float) = 0.03
        _Scale ("Scale", Float) = 3.0
        _Distortion ("Distortion", Float) = 0.8
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
            Name "BackgroundWave"
            Tags { "LightMode" = "Universal2D" }

            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

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
                half4 _BaseColor;
                half4 _WaveColor;
                half _WaveIntensity;
                half _Speed;
                half _Scale;
                half _Distortion;
            CBUFFER_END

            half ValueNoise(half2 p)
            {
                half2 i = floor(p);
                half2 f = frac(p);
                half2 u = f * f * (3.0h - 2.0h * f);

                half a = frac(sin(dot(i + half2(0.0h, 0.0h), half2(127.1h, 311.7h))) * 43758.5453h);
                half b = frac(sin(dot(i + half2(1.0h, 0.0h), half2(127.1h, 311.7h))) * 43758.5453h);
                half c = frac(sin(dot(i + half2(0.0h, 1.0h), half2(127.1h, 311.7h))) * 43758.5453h);
                half d = frac(sin(dot(i + half2(1.0h, 1.0h), half2(127.1h, 311.7h))) * 43758.5453h);

                return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
            }

            half FBM(half2 p)
            {
                half value = 0.0h;
                half amplitude = 0.5h;

                // 3 octaves (fixed loop for WebGL)
                value += amplitude * ValueNoise(p);
                p *= 2.0h;
                amplitude *= 0.5h;

                value += amplitude * ValueNoise(p);
                p *= 2.0h;
                amplitude *= 0.5h;

                value += amplitude * ValueNoise(p);

                return value;
            }

            half DomainWarp(half2 uv, half time)
            {
                half2 q = half2(
                    FBM(uv + half2(0.0h, 0.0h) + time * 0.5h),
                    FBM(uv + half2(5.2h, 1.3h) + time * 0.3h));

                half2 r = half2(
                    FBM(uv + _Scale * q + half2(1.7h, 9.2h) + time * 0.15h),
                    FBM(uv + _Scale * q + half2(8.3h, 2.8h) + time * 0.126h));

                return FBM(uv + _Distortion * r);
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half2 uv = input.uv * _Scale;
                half time = _Time.y * _Speed;

                half warp = DomainWarp(uv, time);
                half blend = saturate(warp * _WaveIntensity);

                half3 color = lerp(_BaseColor.rgb, _WaveColor.rgb, blend);
                return half4(color, 1.0h);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
