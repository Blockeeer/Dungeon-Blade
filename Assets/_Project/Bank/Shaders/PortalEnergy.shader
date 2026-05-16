Shader "DungeonBlade/PortalEnergy"
{
    Properties
    {
        _BaseMap        ("Swirl Texture", 2D)        = "white" {}
        _BaseColor      ("Base Color", Color)        = (1, 0.4, 0.1, 1)
        _EmissionColor  ("Emission", Color)          = (1, 0.4, 0.1, 1)
        _UVRotation     ("UV Rotation (rad)", Float) = 0
        _UVRadialScroll ("Radial Scroll", Float)     = 0
        _SpiralTwist    ("Spiral Twist", Float)      = 1.5
        _AlphaCutoff    ("Alpha Cutoff", Range(0,1)) = 0
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }
        LOD 100

        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode" = "UniversalForward" }
            Cull Off
            ZWrite On

            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseColor;
                float4 _EmissionColor;
                float  _UVRotation;
                float  _UVRadialScroll;
                float  _SpiralTwist;
                float  _AlphaCutoff;
            CBUFFER_END

            static const float TAU = 6.28318530718;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv          = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 p   = IN.uv - 0.5;
                float  r   = saturate(length(p) * 2.0);
                float  ang = atan2(p.y, p.x);

                // True spiral: angle gets twisted by radius so iso-color lines wind inward
                float twisted = ang + _UVRotation + r * _SpiralTwist;

                float u = twisted / TAU + 0.5;
                float v = frac(r * 2.0 + _UVRadialScroll);

                half4 tex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, float2(u, v));

                // Radial vignette so the center glows hotter than the edges
                float centerBoost = 1.0 - smoothstep(0.0, 1.0, r);
                half3 col = tex.rgb * _BaseColor.rgb + _EmissionColor.rgb * (0.5 + centerBoost);

                clip(tex.a - _AlphaCutoff);
                return half4(col, 1);
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Unlit"
}
