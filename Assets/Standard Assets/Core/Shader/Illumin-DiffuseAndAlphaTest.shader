Shader "Universal Render Pipeline/DSelf-Illumin/Diffuse_AlphaTest"
{
    Properties
    {
        _Color ("Main Color", Color) = (1,1,1,1)
        _MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
        _Illum ("Illumin (A)", 2D) = "white" {}
        _Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
        
        // URP 标准属性，用于配合编辑器 (可选，但推荐)
        [HideInInspector] _Surface("__surface", Float) = 0.0
        [HideInInspector] _Blend("__blend", Float) = 0.0
        [HideInInspector] _AlphaClip("__clip", Float) = 1.0
        [HideInInspector] _Cull("__cull", Float) = 0.0 // 0 = Off
    }

    SubShader
    {
        // 渲染管线标记
        Tags 
        { 
            "RenderType" = "TransparentCutout" 
            "Queue" = "AlphaTest" 
            "RenderPipeline" = "UniversalPipeline" 
            "IgnoreProjector" = "True"
        }
        
        LOD 200
        
        // 全局双面渲染
        Cull Off

        // ------------------------------------------------------------------
        // Pass 1: 主光照 Pass (UniversalForward)
        // ------------------------------------------------------------------
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            // 接收阴影、雾效等关键字
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
                float2 uvIllum : TEXCOORD3;
                float fogFactor : TEXCOORD4;
            };

            // 材质属性定义 (CBUFFER 用于 SRP Batcher 优化)
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Illum_ST;
                half4 _Color;
                half _Cutoff;
            CBUFFER_END

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            TEXTURE2D(_Illum);   SAMPLER(sampler_Illum);

            Varyings vert(Attributes input)
            {
                Varyings output;

                // 顶点位置转换：对象空间 -> 世界空间 -> 裁剪空间
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionWS = vertexInput.positionWS;
                output.positionCS = vertexInput.positionCS;

                // 法线转换：对象空间 -> 世界空间
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, float4(1,1,1,1)); // 简化切线
                output.normalWS = normalInput.normalWS;

                // UV 处理
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.uvIllum = TRANSFORM_TEX(input.uv, _Illum);

                // 雾效
                output.fogFactor = ComputeFogFactor(output.positionCS.z);

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // 1. 采样主纹理和颜色
                half4 mainTexColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                half4 baseColor = mainTexColor * _Color;

                // 2. Alpha Test (透明度剔除)
                clip(baseColor.a - _Cutoff);

                // 3. 准备光照数据
                InputData inputData = (InputData)0;
                inputData.positionWS = input.positionWS;
                inputData.normalWS = NormalizeNormalPerPixel(input.normalWS);
                inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
                inputData.shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                inputData.fogCoord = input.fogFactor;
                inputData.bakedGI = SampleSH(input.normalWS); // 简化的 GI 采样

                // 4. 计算光照 (SimpleLit / Lambert 模拟)
                // 使用 UniversalFragmentBlinnPhong 但将 Specular 和 Smoothness 设为 0 来模拟 Lambert
                SurfaceData surfaceData = (SurfaceData)0;
                surfaceData.albedo = baseColor.rgb;
                surfaceData.alpha = baseColor.a;
                surfaceData.metallic = 0;
                surfaceData.specular = 0;
                surfaceData.smoothness = 0;
                surfaceData.occlusion = 1;
                
                // 5. 计算自发光 (Emission)
                // 原逻辑: o.Emission = c.rgb * tex2D(_Illum, IN.uv_Illum).a;
                half illumAlpha = SAMPLE_TEXTURE2D(_Illum, sampler_Illum, input.uvIllum).a;
                surfaceData.emission = baseColor.rgb * illumAlpha;

                // 6. 组合最终颜色
                half4 color = UniversalFragmentBlinnPhong(inputData, surfaceData);
                
                // 7. 应用雾效 (UniversalFragmentBlinnPhong 内部通常不包含雾效混合，需手动处理或确认)
                color.rgb = MixFog(color.rgb, inputData.fogCoord);
                
                return color;
            }
            ENDHLSL
        }

        // ------------------------------------------------------------------
        // Pass 2: ShadowCaster (投射阴影必须)
        // ------------------------------------------------------------------
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            
            // 包含 URP 阴影库
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _Color;
                half _Cutoff;
            CBUFFER_END

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);

            Varyings ShadowPassVertex(Attributes input)
            {
                Varyings output;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                float3 positionWS = vertexInput.positionWS;
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);

                // 处理阴影偏差
                output.positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, float3(0,0,0)));
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                return output;
            }

            half4 ShadowPassFragment(Varyings input) : SV_TARGET
            {
                half4 mainTexColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                half4 baseColor = mainTexColor * _Color;
                
                // 阴影也必须进行 Alpha Test，否则阴影是方块
                clip(baseColor.a - _Cutoff);
                
                return 0;
            }
            ENDHLSL
        }

        // ------------------------------------------------------------------
        // Pass 3: DepthOnly (深度预渲染，用于 SSAO 等后期)
        // ------------------------------------------------------------------
        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }

            ZWrite On
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

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
                float4 _MainTex_ST;
                half4 _Color;
                half _Cutoff;
            CBUFFER_END

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);

            Varyings DepthOnlyVertex(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                return output;
            }

            half4 DepthOnlyFragment(Varyings input) : SV_TARGET
            {
                half4 mainTexColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                half4 baseColor = mainTexColor * _Color;
                
                clip(baseColor.a - _Cutoff);
                
                return 0;
            }
            ENDHLSL
        }
    }
    
    // 降级回滚 (虽然在 URP 环境下通常不生效，但保留是个好习惯)
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}