Shader "WalldoffStudios/PlatformShader"
{
    Properties
    {
        _ColorA ("Color A", Color) = (1, 0, 0, 1)
        _ColorB ("Color B", Color) = (0, 0, 1, 1)
        _GradientRadius ("Gradient Radius", Float) = 1.0
        _GradientSoftness ("Gradient Softness", Float) = 0.1
    }
    SubShader
    {
        PackageRequirements
        {
            "com.unity.render-pipelines.universal": "10.2.1"
        }
        Pass
        {
            Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline" }
            HLSLPROGRAM
            // #include "PlatformURP.cginc"
            #pragma vertex vert
            #pragma fragment frag
            //#pragma multi_compile_fwdbase
            #pragma multi_compile_instancing
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT 

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            // Shader properties
            float4 _ColorA;
            float4 _ColorB;
            float _GradientRadius;
            float _GradientSoftness;

            struct Attributes
            {
                float4 positionOS  : POSITION;
                float3 normal : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normal    : TEXCOORD0;
                float3 worldPos  : TEXCOORD1;
                float4 shadowCoords : TEXCOORD2;
            };

            Varyings vert(Attributes v)
            {
                Varyings o;

                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                VertexPositionInputs positions = GetVertexPositionInputs(v.positionOS.xyz);
                o.normal = normalize(mul((float3x3)unity_ObjectToWorld, v.normal));
                float3 worldPos = mul(unity_ObjectToWorld, v.positionOS).xyz;
                o.worldPos.xyz = worldPos;

                o.shadowCoords = GetShadowCoord(positions);

                return o;
            }

            inline half3 GammaToLinearSpace (half3 sRGB)
            {
                return sRGB * (sRGB * (sRGB * 0.305306011h + 0.682171111h) + 0.012522878h);
            }

            half4 frag(Varyings i) : SV_Target
            {
                // Access per-instance gradient center
                float3 gradientCenter = float3(0,0,0);
                float gradientRadius = _GradientRadius;
                float gradientSoftness = _GradientSoftness;

                // Compute distance from gradient center
                float dist = distance(i.worldPos.xyz, gradientCenter);
                float t = smoothstep(gradientRadius - gradientSoftness, gradientRadius + gradientSoftness, dist);

                // Interpolate between Color A and Color B
                float4 color = lerp(_ColorA, _ColorB, t);

                // Shadows
                half shadowAmount = saturate(MainLightRealtimeShadow(i.shadowCoords) * 10);
                //half shadowAmount = MainLightRealtimeShadow(i.shadowCoords);
                
                color.rgb = lerp(float3(0.0,0.0,0.0) * shadowAmount, color.rgb, shadowAmount);

                // Handle color space
                #ifndef UNITY_COLORSPACE_GAMMA
                    color.rgb = GammaToLinearSpace(color.rgb);
                #endif

                return color;
            }
            ENDHLSL
        }
        Pass 
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            CGPROGRAM
            //#include "ToonShaders/ShadowCasting.cginc"
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include <UnityShaderUtilities.cginc>

            struct appdata {
                float4 vertex : POSITION;
            };

            struct Varyings {
                float4 pos : SV_POSITION;
            };

            Varyings vert(appdata v) {
                Varyings o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            float4 frag() : SV_Target {
                return 1.0;
            }
            ENDCG
        }
    }

    SubShader
    {
        
        Pass
        {
            Tags { "LightMode"="ForwardBase" "RenderType"="Opaque" }
            CGPROGRAM
            //#include "PlatformBIP.cginc"
                #pragma vertex vert
                #pragma fragment frag
                #pragma multi_compile_fwdbase
                #pragma multi_compile_instancing
                #pragma instancing_options
                #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
                #pragma multi_compile_fragment _ _SHADOWS_SOFT
                #include "UnityInstancing.cginc"
                #include "Lighting.cginc"
                #include "AutoLight.cginc"
                #include "UnityCG.cginc"

                // Shader properties
                float4 _ColorA;
                float4 _ColorB;
                float _GradientRadius;
                float _GradientSoftness;

                struct Attributes
                {
                    float4 vertex : POSITION;
                    float3 normal : NORMAL;
                    float2 uv     : TEXCOORD0;
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                struct Varyings
                {
                    float4 pos       : SV_POSITION;
                    float3 normal    : TEXCOORD0;
                    float4 worldPos  : TEXCOORD1;
                    SHADOW_COORDS(2)
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                Varyings vert(Attributes v)
                {
                    Varyings o;

                    UNITY_SETUP_INSTANCE_ID(v);
                    UNITY_TRANSFER_INSTANCE_ID(v, o);

                    o.pos = UnityObjectToClipPos(v.vertex);
                    o.normal = UnityObjectToWorldNormal(v.normal);
                    o.worldPos = mul(unity_ObjectToWorld, v.vertex);

                    TRANSFER_SHADOW(o);

                    return o;
                }

                half4 frag(Varyings i) : SV_Target
                {
                    UNITY_SETUP_INSTANCE_ID(i);

                    // Access per-instance gradient center
                    float3 gradientCenter = float3(0,0,0);
                    float gradientRadius = _GradientRadius;
                    float gradientSoftness = _GradientSoftness;

                    // Compute distance from gradient center
                    float dist = distance(i.worldPos.xyz, gradientCenter);
                    float t = smoothstep(gradientRadius - gradientSoftness, gradientRadius + gradientSoftness, dist);

                    // Interpolate between Color A and Color B
                    float4 color = lerp(_ColorA, _ColorB, t);

                    // Shadows
                    half shadowAmount = saturate(SHADOW_ATTENUATION(i) * 10);
                    color.rgb = lerp(float3(0.0,0.0,0.0) * shadowAmount, color.rgb, shadowAmount);

                    // Handle color space
                    #ifndef UNITY_COLORSPACE_GAMMA
                        color.rgb = GammaToLinearSpace(color.rgb);
                    #endif

                    return color;
                }

            ENDCG
        }
        Pass 
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            CGPROGRAM
            //#include "ToonShaders/ShadowCasting.cginc"
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include <UnityShaderUtilities.cginc>

            struct appdata {
                float4 vertex : POSITION;
            };

            struct Varyings {
                float4 pos : SV_POSITION;
            };

            Varyings vert(appdata v) {
                Varyings o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            float4 frag() : SV_Target {
                return 1.0;
            }
            ENDCG
        }
    }
}
