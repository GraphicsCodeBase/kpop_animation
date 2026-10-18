// Procedural mosaic tiles for the pool (Goal 1, Stage 1C).
// Tiles are laid out in world space, so they line up across the floor, walls and lane lines
// with no UVs. Each tile picks a colour from a weighted palette of four; grout separates them.
// Far away, where a tile is smaller than a pixel, it fades to the palette average to avoid shimmer.
// Lighting is soft and matte (wrapped diffuse, softened shadows) to suit the stylised anime look.
Shader "KPop/Mosaic"
{
    Properties
    {
        _TileSize ("Tile Size (m)", Float) = 0.025
        _GridOffset ("Grid Offset (m, world XYZ)", Vector) = (0, 0, 0, 0)
        _GroutWidth ("Grout Width (fraction of tile)", Range(0, 0.4)) = 0.12
        _GroutColor ("Grout Colour", Color) = (0.86, 0.93, 0.92, 1)
        _Color0 ("Colour 0", Color) = (0.80, 0.90, 0.89, 1)
        _Color1 ("Colour 1", Color) = (0.75, 0.87, 0.86, 1)
        _Color2 ("Colour 2", Color) = (0.49, 0.69, 0.70, 1)
        _Color3 ("Colour 3", Color) = (0.37, 0.62, 0.65, 1)
        _Weights ("Weights 0-3", Vector) = (40, 32, 18, 10)
        [Toggle] _DotLattice ("Dot Lattice (dark tiles only on every other tile)", Float) = 0
        _DotChance ("Dot Chance (per lattice site)", Range(0, 1)) = 0.75
        _Seed ("Seed", Float) = 0
        _Wrap ("Diffuse Wrap", Range(0, 1)) = 0.5
        _ShadowStrength ("Shadow Strength", Range(0, 1)) = 0.6
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }

        // Shared by every pass: the SRP Batcher needs one identical UnityPerMaterial layout across passes,
        // otherwise materials get each other's properties.
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        CBUFFER_START(UnityPerMaterial)
            float _TileSize, _GroutWidth, _Seed, _Wrap, _ShadowStrength, _DotLattice, _DotChance;
            float4 _GroutColor, _Color0, _Color1, _Color2, _Color3, _Weights, _GridOffset;
        CBUFFER_END
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
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

            float Hash(float3 p)
            {
                p = frac(p * 0.1031 + _Seed * 0.0173);
                p += dot(p, p.zyx + 31.32);
                return frac((p.x + p.y) * p.z);
            }

            float3 Palette(float h, float4 w)
            {
                float4 c = float4(w.x, w.x + w.y, w.x + w.y + w.z, w.x + w.y + w.z + w.w) / max(dot(w, 1), 1e-4);
                return h < c.x ? _Color0.rgb : h < c.y ? _Color1.rgb : h < c.z ? _Color2.rgb : _Color3.rgb;
            }

            half4 frag(Varyings i) : SV_Target
            {
                float3 n = normalize(i.normalWS);
                float3 a = abs(n);
                // Project onto the plane the surface faces (all pool surfaces are axis-aligned).
                bool up = a.y >= a.x && a.y >= a.z;
                float3 g3 = i.positionWS + _GridOffset.xyz;
                float2 uv = up ? g3.xz : a.x >= a.z ? g3.zy : g3.xy;
                float axis = up ? 1 : a.x >= a.z ? 2 : 3;
                uv /= _TileSize;

                float2 cell = floor(uv);
                float2 f = frac(uv);
                float h = Hash(float3(cell, axis));
                // Dot lattice (pool floor in the ref): dark tiles (colours 2-3) sit only on every other tile in
                // both directions, so they never touch; the rest is the light field (colours 0-1).
                bool site = all(frac(cell * 0.5) < 0.25);
                float3 fieldTile = h < _Weights.x / max(_Weights.x + _Weights.y, 1e-4) ? _Color0.rgb : _Color1.rgb;
                float3 dotTile = frac(h * 7.13) < _Weights.z / max(_Weights.z + _Weights.w, 1e-4) ? _Color2.rgb : _Color3.rgb;
                float3 tile = _DotLattice > 0.5 ? (site && frac(h * 3.71) < _DotChance ? dotTile : fieldTile) : Palette(h, _Weights);

                // Grout, antialiased over one pixel.
                float2 px = max(fwidth(uv), 1e-5);
                float2 g = smoothstep(_GroutWidth * 0.5 - px, _GroutWidth * 0.5 + px, min(f, 1 - f));
                float3 albedo = lerp(_GroutColor.rgb, tile, g.x * g.y);

                // Fade to the average colour once tiles drop below ~1 pixel (where they would shimmer).
                float4 w = _Weights / max(dot(_Weights, 1), 1e-4);
                float3 avgTile = _Color0.rgb * w.x + _Color1.rgb * w.y + _Color2.rgb * w.z + _Color3.rgb * w.w;
                if (_DotLattice > 0.5)
                {
                    float3 avgLight = (_Color0.rgb * w.x + _Color1.rgb * w.y) / max(w.x + w.y, 1e-4);
                    float3 avgDark = (_Color2.rgb * w.z + _Color3.rgb * w.w) / max(w.z + w.w, 1e-4);
                    avgTile = lerp(avgLight, avgDark, 0.25 * _DotChance);
                }
                float groutArea = 1 - (1 - _GroutWidth) * (1 - _GroutWidth);
                float3 avg = lerp(avgTile, _GroutColor.rgb, groutArea);
                albedo = lerp(albedo, avg, smoothstep(0.8, 1.3, max(px.x, px.y)));

                Light light = GetMainLight(TransformWorldToShadowCoord(i.positionWS));
                float shadow = lerp(1, light.shadowAttenuation, _ShadowStrength);
                float ndl = saturate((dot(n, light.direction) + _Wrap) / (1 + _Wrap));
                float3 ambient = SampleSH(n);
                #if defined(_SCREEN_SPACE_OCCLUSION)
                    ambient *= GetScreenSpaceAmbientOcclusion(GetNormalizedScreenSpaceUV(i.positionCS)).indirectAmbientOcclusion;
                #endif

                float3 col = albedo * (light.color * ndl * shadow + ambient);
                return half4(MixFog(col, i.fogFactor), 1);
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

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On ColorMask R

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            float4 vert(float4 positionOS : POSITION) : SV_POSITION { return TransformObjectToHClip(positionOS.xyz); }
            half frag() : SV_Target { return 0; }
            ENDHLSL
        }

        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormals" }
            ZWrite On

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Packing.hlsl"

            struct V { float4 positionCS : SV_POSITION; float3 normalWS : TEXCOORD0; };
            V vert(float4 positionOS : POSITION, float3 normalOS : NORMAL)
            {
                V o;
                o.positionCS = TransformObjectToHClip(positionOS.xyz);
                o.normalWS = TransformObjectToWorldNormal(normalOS);
                return o;
            }
            half4 frag(V i) : SV_Target
            {
                float3 n = normalize(i.normalWS);
                #if defined(_GBUFFER_NORMALS_OCT)
                    float2 oct = saturate(PackNormalOctQuadEncode(n) * 0.5 + 0.5);
                    return half4(PackFloat2To888(oct), 0);
                #else
                    return half4(n, 0);
                #endif
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
