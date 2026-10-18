// See-through latex balloons for the pool (Goal 1, Stage 1C).
// Clearer face-on and more solid toward the silhouette (fresnel), with a soft highlight,
// as the balloons read in the ref. Transparent, no depth write; casts a light shadow.
Shader "KPop/Balloon"
{
    Properties
    {
        _BaseColor ("Colour", Color) = (0.78, 0.72, 0.91, 1)
        _AlphaCenter ("Opacity Face-On", Range(0, 1)) = 0.35
        _AlphaEdge ("Opacity at Edge", Range(0, 1)) = 0.85
        _FresnelPower ("Edge Falloff", Range(0.5, 8)) = 2.5
        _Highlight ("Highlight Strength", Range(0, 2)) = 0.6
        _Glossiness ("Highlight Tightness", Range(8, 256)) = 64
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" }

        // Shared by every pass so the SRP Batcher sees one UnityPerMaterial layout.
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        CBUFFER_START(UnityPerMaterial)
            float4 _BaseColor;
            float _AlphaCenter, _AlphaEdge, _FresnelPower, _Highlight, _Glossiness;
        CBUFFER_END
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float fogFactor : TEXCOORD2;
            };

            Varyings vert(Attributes v)
            {
                Varyings o;
                VertexPositionInputs p = GetVertexPositionInputs(v.positionOS.xyz);
                o.positionCS = p.positionCS;
                o.positionWS = p.positionWS;
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                o.fogFactor = ComputeFogFactor(p.positionCS.z);
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                float3 n = normalize(i.normalWS);
                float3 v = normalize(GetWorldSpaceViewDir(i.positionWS));
                float fres = pow(1 - saturate(dot(n, v)), _FresnelPower);

                Light light = GetMainLight();
                float ndl = saturate(dot(n, light.direction) * 0.5 + 0.5);
                float3 col = _BaseColor.rgb * (light.color * ndl * 0.6 + SampleSH(n));
                float spec = pow(saturate(dot(n, normalize(light.direction + v))), _Glossiness) * _Highlight;
                col += light.color * spec;

                float alpha = saturate(lerp(_AlphaCenter, _AlphaEdge, fres) + spec);
                return half4(MixFog(col, i.fogFactor), alpha);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On ZTest LEqual ColorMask 0

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            float3 _LightDirection, _LightPosition;

            float4 vert(float4 positionOS : POSITION, float3 normalOS : NORMAL) : SV_POSITION
            {
                float3 p = TransformObjectToWorld(positionOS.xyz);
                float3 n = TransformObjectToWorldNormal(normalOS);
                #if defined(_CASTING_PUNCTUAL_LIGHT_SHADOW)
                    float3 dir = normalize(_LightPosition - p);
                #else
                    float3 dir = _LightDirection;
                #endif
                float4 cs = TransformWorldToHClip(ApplyShadowBias(p, n, dir));
                #if UNITY_REVERSED_Z
                    cs.z = min(cs.z, UNITY_NEAR_CLIP_VALUE);
                #else
                    cs.z = max(cs.z, UNITY_NEAR_CLIP_VALUE);
                #endif
                return cs;
            }
            half4 frag() : SV_Target { return 0; }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
