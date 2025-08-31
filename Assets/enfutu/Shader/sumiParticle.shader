Shader "enfutu/sumiParticle"
{
    Properties
    {
        _Color ("Color", Color) = (0,0,0,0)
        _MainTex ("Texture", 2D) = "white" {}
        _Map ("Map", 2D) = "white"{}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 color : COLOR;
                float4 uv : TEXCOORD0;  //xy:uv, zw:blank
                float4 center : TEXCOORD1; //xyz:center, w:id
                // single pass instanced rendering
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
                int id : TEXCOORD1;

                // single pass instanced rendering
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex, _Map;
            float4 _MainTex_ST, _Color;

            v2f vert (appdata v)
            {
                v2f o;

                // single pass instanced rendering
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                int id = v.center.w;

                float _x = id % 4096;
                float _y = floor(id / 4096);
                float2 st = (float2(_x, _y) + .5) / 4096;
                

                float3 fixedCenter = tex2Dlod(_Map, float4(st, 0, 0));

                float3 wv = mul(unity_ObjectToWorld, v.vertex).xyz;

                float3 pos = wv - v.center;
                pos += fixedCenter;

                o.id = id;

                v.vertex =  mul(unity_WorldToObject, float4(pos, 1));
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = st;//TRANSFORM_TEX(v.uv.xy, _MainTex);
                o.color = v.color;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // single pass instanced rendering
                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                //float2 st = floor(i.color.rg * 4096) / 4096;
                // sample the texture
                fixed4 col = tex2D(_Map, i.uv);

                return col;
            }
            ENDCG
        }
    }
}
