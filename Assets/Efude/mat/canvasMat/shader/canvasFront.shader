Shader "Unlit/canvasFront"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Washi ("和紙テクスチャ", 2D) = "white" {}
        [Toggle] _Off ("_Off(debug)", int) = 0     
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "LightMode" = "ForwardBase" }

        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "UnityLightingCommon.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID //SPSI
            };

            struct v2f_vert
            {
                float2 uv : TEXCOORD0;
                float3 worldPosZero : TEXCOORD1;
                fixed4 diff : COLOR0; // 拡散ライティングカラー
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO //SPSI
            };

            struct v2f_frag
            {
                float2 uv : TEXCOORD0;
                float3 worldPosZero : TEXCOORD1;
                fixed4 diff : COLOR0; // 拡散ライティングカラー
                UNITY_VPOS_TYPE vpos : VPOS;
            };

            sampler2D _MainTex, _Washi;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            int _Off;

            v2f_vert vert (appdata v)
            {
                v2f_vert o;
                UNITY_SETUP_INSTANCE_ID(v); //SPSI
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o); //SPSI
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                //撮影機能に関わる部分
                //オブジェクト原点。ローカル座標系をワールド座標系に変換
                fixed3 worldPosZero = mul(unity_ObjectToWorld, fixed4(0,0,0,1));
                o.worldPosZero = worldPosZero;

                //ライティングに関わる部分
                half3 worldNormal = UnityObjectToWorldNormal(v.normal);
                half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
                o.diff = nl * _LightColor0;
                o.diff.rgb += ShadeSH9(half4(worldNormal,1));

                return o;
            }

            fixed4 frag (v2f_frag i) : SV_Target
            {
                half2 st = i.uv;
                //反転
                st.y = 1 - st.y;           

                half4 col = tex2D(_MainTex, st);
                col *= tex2D(_Washi, st) + .2;
                col *= lerp(1, i.diff, step(.5, _Off));

                //col.rgb = lerp(col.rgb * .8, col.rgb, col.a);

                return col;
            }
            ENDCG
        }
    }
}
