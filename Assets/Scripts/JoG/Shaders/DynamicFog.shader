Shader "JoG/Environment/DynamicFog"
{
    Properties
    {
        [Header(Fog Settings)]
        _FogColor ("Fog Color", Color) = (0.5, 0.6, 0.7, 1)
        _FogDensity ("Fog Density", Range(0, 1)) = 0.1
        _FogSpeed ("Fog Speed", Vector) = (0.1, 0.05, 0, 0)
        _FogNoiseScale ("Fog Noise Scale", Range(0.01, 10)) = 1.0

        [Header(Depth Fog)]
        _DepthFogHeight ("Depth Fog Height", Range(0, 10)) = 2.0
        _DepthFogOffset ("Depth Fog Offset", Range(-5, 5)) = 0.0
        _DepthFogIntensity ("Depth Fog Intensity", Range(0, 1)) = 0.5

        [Header(Animation)]
        _AnimationSpeed ("Animation Speed", Range(0, 5)) = 1.0
        _Turbulence ("Turbulence", Range(0, 1)) = 0.3
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent+100"
        }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float3 localPos : TEXCOORD2;
                float fogFactor : TEXCOORD3;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _FogColor;
                float _FogDensity;
                float4 _FogSpeed;
                float _FogNoiseScale;
                float _DepthFogHeight;
                float _DepthFogOffset;
                float _DepthFogIntensity;
                float _AnimationSpeed;
                float _Turbulence;
            CBUFFER_END

            float Hash(float2 p)
            {
                float3 p3 = frac(float3(p.xyx) * 0.1031);
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.x + p3.y) * p3.z);
            }

            float Noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f);

                return lerp(
                    lerp(Hash(i), Hash(i + float2(1, 0)), u.x),
                    lerp(Hash(i + float2(0, 1)), Hash(i + float2(1, 1)), u.x),
                    u.y
                );
            }

            float FBM(float2 p)
            {
                float value = 0;
                float amplitude = 0.5;
                float frequency = 1;

                for (int i = 0; i < 5; i++)
                {
                    value += amplitude * Noise(p * frequency);
                    amplitude *= 0.5;
                    frequency *= 2;
                }
                return value;
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.position = TransformObjectToHClip(v.vertex.xyz);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.localPos = v.vertex.xyz;
                o.uv = v.uv;

                float3 camPos = _WorldSpaceCameraPos;
                float3 viewDir = normalize(o.worldPos - camPos);
                float fogDistance = length(o.worldPos - camPos);
                o.fogFactor = saturate(fogDistance * 0.01 * _FogDensity);

                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float time = _Time.y * _AnimationSpeed;

                float2 noiseUV = i.worldPos.xz * _FogNoiseScale * 0.1;
                noiseUV += float2(time * _FogSpeed.x, time * _FogSpeed.y);

                float noise1 = FBM(noiseUV);
                noiseUV += float2(time * 0.3, time * 0.2) * _Turbulence;
                float noise2 = FBM(noiseUV * 1.5);
                noiseUV += float2(time * -0.2, time * 0.15) * _Turbulence * 0.5;
                float noise3 = FBM(noiseUV * 2.0);

                float combinedNoise = noise1 * 0.5 + noise2 * 0.3 + noise3 * 0.2;

                float heightFactor = saturate((i.worldPos.y - _DepthFogOffset) / _DepthFogHeight);
                float depthFog = combinedNoise * heightFactor * _DepthFogIntensity;

                float distanceFog = i.fogFactor;
                float finalFog = saturate(combinedNoise * 0.7 + depthFog * 0.3 + distanceFog * 0.2);

                finalFog = smoothstep(0, 0.8, finalFog) * _FogDensity;

                float4 col = _FogColor;
                col.a = finalFog;

                return col;
            }
            ENDHLSL
        }
    }
}