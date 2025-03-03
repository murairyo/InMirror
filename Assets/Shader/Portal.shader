// 汎用的なMRTKスタイルシェーダー
// 元のMRTKシェーダーから修正

Shader "Custom/MRTKStyleStandard"
{
    Properties
    {
        // カリングモード設定
        [Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull Mode", Float) = 2 // Back
        
        // メインマップ
        _Color("Color", Color) = (1.0, 1.0, 1.0, 1.0)
        _MainTex("Albedo", 2D) = "white" {}
        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
        _Metallic("Metallic", Range(0.0, 1.0)) = 0.0
        _Glossiness("Smoothness", Range(0.0, 1.0)) = 0.5
        
        [Toggle(_NORMALMAP)] _EnableNormalMap("Enable Normal Map", Float) = 0.0
        [NoScaleOffset] _BumpMap("Normal Map", 2D) = "bump" {}
        _BumpScale("Normal Scale", Float) = 1.0
        
        [Toggle(_EMISSION)] _EnableEmission("Enable Emission", Float) = 0.0
        [HDR]_EmissionColor("Emission Color", Color) = (0.0, 0.0, 0.0, 1.0)

        // レンダリングモード設定
        [Enum(Opaque,0,Cutout,1,Fade,2,Transparent,3)] _RenderingMode("Rendering Mode", Float) = 0
        
        // ブレンド設定（レンダリングモードに基づいて自動設定）
        [HideInInspector] _SrcBlend("Source Blend", Float) = 1
        [HideInInspector] _DstBlend("Destination Blend", Float) = 0
        [HideInInspector] _ZWrite("ZWrite", Float) = 1
        [HideInInspector] _BlendOp("Blend Operation", Float) = 0
        
        // ステンシル関連のプロパティ
        [Space(20)]
        [Header(Stencil Settings)]
        [Space(10)]
        _StencilRef("Stencil Reference", Range(0, 255)) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp("Stencil Comparison", Float) = 8 // Always
        [Enum(UnityEngine.Rendering.StencilOp)] _StencilOp("Stencil Operation", Float) = 0 // Keep
        [Enum(UnityEngine.Rendering.StencilOp)] _StencilFail("Stencil Fail", Float) = 0 // Keep
        [Enum(UnityEngine.Rendering.StencilOp)] _StencilZFail("Stencil ZFail", Float) = 0 // Keep
        _StencilWriteMask("Stencil Write Mask", Range(0, 255)) = 255
        _StencilReadMask("Stencil Read Mask", Range(0, 255)) = 255
    }

    // レンダリングモードに基づいてシェーダー設定を変更するカスタムシェーダーGUI
    CustomEditor "RenderingModeCustomEditor"

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 100
        
        // カリングモード設定
        Cull [_Cull]
        
        // ステンシル設定
        Stencil
        {
            Ref [_StencilRef]
            Comp [_StencilComp]
            Pass [_StencilOp]
            Fail [_StencilFail]
            ZFail [_StencilZFail]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        // レンダリング設定
        Blend [_SrcBlend] [_DstBlend]
        BlendOp [_BlendOp]
        ZWrite [_ZWrite]
        
        Pass
        {
            Name "FORWARD"
            Tags { "LightMode" = "ForwardBase" }
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            
            // 機能の有効化
            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local _EMISSION
            #pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            
            #include "UnityCG.cginc"
            #include "UnityPBSLighting.cginc"
            #include "AutoLight.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
                #if defined(_NORMALMAP)
                float4 tangent : TANGENT;
                #endif
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
                float3 worldNormal : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
                #if defined(_NORMALMAP)
                float3 worldTangent : TEXCOORD3;
                float3 worldBitangent : TEXCOORD4;
                #endif
                UNITY_FOG_COORDS(5)
                UNITY_SHADOW_COORDS(6)
            };
            
            // プロパティの定義
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float _Metallic;
            float _Glossiness;
            float _Cutoff;
            
            #if defined(_NORMALMAP)
            sampler2D _BumpMap;
            float _BumpScale;
            #endif
            
            #if defined(_EMISSION)
            float4 _EmissionColor;
            #endif
            
            // UnpackScaleNormalの再定義
            float3 CustomUnpackScaleNormal(float4 packednormal, float scale)
            {
                #if defined(UNITY_NO_DXT5nm)
                    return packednormal.xyz * 2 - 1;
                #else
                    float3 normal;
                    normal.xy = (packednormal.rg * 2 - 1) * scale;
                    normal.z = sqrt(1 - saturate(dot(normal.xy, normal.xy)));
                    return normal;
                #endif
            }
            
            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                
                #if defined(_NORMALMAP)
                o.worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
                float tangentSign = v.tangent.w * unity_WorldTransformParams.w;
                o.worldBitangent = cross(o.worldNormal, o.worldTangent) * tangentSign;
                #endif
                
                UNITY_TRANSFER_FOG(o, o.pos);
                UNITY_TRANSFER_SHADOW(o, o.uv);
                return o;
            }
            
            fixed4 frag(v2f i) : SV_Target
            {
                // 基本的なテクスチャサンプリング
                fixed4 albedo = tex2D(_MainTex, i.uv) * _Color;
                
                // アルファテスト
                #if defined(_ALPHATEST_ON)
                clip(albedo.a - _Cutoff);
                #endif
                
                // 法線マップ
                float3 worldNormal = i.worldNormal;
                #if defined(_NORMALMAP)
                float3 normalMap = CustomUnpackScaleNormal(tex2D(_BumpMap, i.uv), _BumpScale);
                float3x3 tangentToWorld = float3x3(i.worldTangent, i.worldBitangent, i.worldNormal);
                worldNormal = mul(normalMap, tangentToWorld);
                #endif
                worldNormal = normalize(worldNormal);
                
                // ライティング計算
                float3 worldViewDir = normalize(UnityWorldSpaceViewDir(i.worldPos));
                
                // PBRパラメータ設定
                SurfaceOutputStandard o;
                UNITY_INITIALIZE_OUTPUT(SurfaceOutputStandard, o);
                o.Albedo = albedo.rgb;
                o.Normal = worldNormal;
                o.Metallic = _Metallic;
                o.Smoothness = _Glossiness;
                o.Alpha = albedo.a;
                
                #if defined(_EMISSION)
                o.Emission = _EmissionColor.rgb;
                #else
                o.Emission = 0;
                #endif
                
                // ライティング
                UnityGI gi;
                UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
                gi.indirect.diffuse = 0;
                gi.indirect.specular = 0;
                gi.light.color = _LightColor0.rgb;
                gi.light.dir = normalize(_WorldSpaceLightPos0.xyz);
                
                // シャドウ
                UNITY_LIGHT_ATTENUATION(atten, i, i.worldPos);
                gi.light.color *= atten;
                
                // 最終的なカラー計算
                float4 finalColor;
                
                #if defined(_ALPHABLEND_ON) || defined(_ALPHAPREMULTIPLY_ON)
                    #if defined(_ALPHAPREMULTIPLY_ON)
                        o.Albedo *= o.Alpha;
                    #endif
                    finalColor = float4(LightingStandard(o, worldViewDir, gi).rgb, o.Alpha);
                #else
                    finalColor = float4(LightingStandard(o, worldViewDir, gi).rgb, 1.0);
                #endif
                
                // フォグ
                UNITY_APPLY_FOG(i.fogCoord, finalColor);
                
                return finalColor;
            }
            ENDCG
        }
        
        // シャドウキャスティングパス
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            
            ZWrite On ZTest LEqual
            Cull [_Cull]
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #pragma multi_compile_shadowcaster
            #pragma shader_feature_local _ALPHATEST_ON
            
            #include "UnityCG.cginc"
            
            struct v2f
            {
                V2F_SHADOW_CASTER;
                float2 uv : TEXCOORD1;
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float _Cutoff;
            
            v2f vert(appdata_base v)
            {
                v2f o;
                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                return o;
            }
            
            float4 frag(v2f i) : SV_Target
            {
                #if defined(_ALPHATEST_ON)
                fixed4 texColor = tex2D(_MainTex, i.uv);
                clip(texColor.a * _Color.a - _Cutoff);
                #endif
                
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
    }
    
    Fallback "Standard"
}