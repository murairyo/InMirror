Shader "Custom/PortalMirror"
{
    Properties
    {
        _MainTex ("Render Texture", 2D) = "white" {} // レンダーテクスチャ
        _Metallic ("Metallic", Range(0, 1)) = 0.0 // メタリック値
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            Stencil
            {
                Ref 1           // ステンシル値を1に設定
                Comp Always     // 常にステンシルテストを通過
                Pass Replace    // ステンシルバッファに1を書き込む
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Metallic; // メタリック値の宣言

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv); // レンダーテクスチャから色を取得
                float metallic = _Metallic; // メタリック値を取得
                col.rgb *= metallic; // 簡単なメタリック効果の例
                return col;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}