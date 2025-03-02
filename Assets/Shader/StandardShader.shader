// 汎用的なMRTKスタイルシェーダー
// 元のMRTKシェーダーから修正

Shader "Custom/MRTKStyleStandard"
{
    Properties
    {
        // メインマップ
        _Color("Color", Color) = (1.0, 1.0, 1.0, 1.0)
        _MainTex("Albedo", 2D) = "white" {}
        [Enum(AlbedoAlphaMode)] _AlbedoAlphaMode("Albedo Alpha Mode", Float) = 0 // "Transparency"
        [Toggle] _AlbedoAssignedAtRuntime("Albedo Assigned at Runtime", Float) = 0.0
        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
        _Metallic("Metallic", Range(0.0, 1.0)) = 0.0
        _Smoothness("Smoothness", Range(0.0, 1.0)) = 0.5
        [Toggle(_CHANNEL_MAP)] _EnableChannelMap("Enable Channel Map", Float) = 0.0
        [NoScaleOffset] _ChannelMap("Channel Map", 2D) = "white" {}
        [Toggle(_NORMAL_MAP)] _EnableNormalMap("Enable Normal Map", Float) = 0.0
        [NoScaleOffset] _NormalMap("Normal Map", 2D) = "bump" {}
        _NormalMapScale("Scale", Float) = 1.0
        [Toggle(_EMISSION)] _EnableEmission("Enable Emission", Float) = 0.0
        [HDR]_EmissiveColor("Emissive Color", Color) = (0.0, 0.0, 0.0, 1.0)
        // ... 既存のコード ...

        // クリッピング関連のプロパティを維持
        _BlendedClippingWidth("Blended Clipping With", Range(0.0, 10.0)) = 1.0
        [Toggle(_CLIPPING_BORDER)] _ClippingBorder("Clipping Border", Float) = 0.0
        _ClippingBorderWidth("Clipping Border Width", Range(0.0, 1.0)) = 0.025
        _ClippingBorderColor("Clipping Border Color", Color) = (1.0, 0.2, 0.0, 1.0)
        
        // ... 既存のコード ...

        // ClippingPlane.csとの互換性のために必要なプロパティ
        _ClipPlane("Clip Plane", Vector) = (0, 1, 0, 0)
        _ClipPlaneSide("Clip Plane Side", Float) = 1.0
    }

    // ... 既存のコード ...

    SubShader
    {
        Pass
        {
            // ... 既存のコード ...

            CGPROGRAM

            // ... 既存のコード ...

            // クリッピングプレーンの機能を維持
            #pragma multi_compile _ _CLIPPING_PLANE _CLIPPING_SPHERE _CLIPPING_BOX

            // ... 既存のコード ...

            // MixedRealityShaderUtils.cgincの代わりに必要な関数を直接定義
            #ifndef MIXEDREALITYSHADERUTILS_INCLUDED
            #define MIXEDREALITYSHADERUTILS_INCLUDED

            // クリッピングプレーン用の関数
            inline float PointVsPlane(float3 worldPosition, float4 plane)
            {
                float3 planePosition = plane.xyz * plane.w;
                return dot(worldPosition - planePosition, plane.xyz);
            }

            // ... 他の必要な関数 ...

            #endif

            // ... 既存のコード ...

            // クリッピングプリミティブ関連のコードを維持
#if defined(_CLIPPING_PRIMITIVE)
                float primitiveDistance = 1.0;
#if defined(_CLIPPING_PLANE)
                fixed clipPlaneSide = UNITY_ACCESS_INSTANCED_PROP(Props, _ClipPlaneSide);
                float4 clipPlane = UNITY_ACCESS_INSTANCED_PROP(Props, _ClipPlane);
                primitiveDistance = min(primitiveDistance, PointVsPlane(i.worldPosition.xyz, clipPlane) * clipPlaneSide);
#endif
                // ... 既存のコード ...
#endif

            // ... 既存のコード ...

            ENDCG
        }

        // ... 既存のコード ...
    }

    Fallback "Standard"
    CustomEditor "StandardShaderGUI" // 標準のシェーダーGUIに変更
}