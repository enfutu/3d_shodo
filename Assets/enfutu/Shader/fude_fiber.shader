Shader "enfutu/fude_fiber"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Cull Off

        CGINCLUDE
        #pragma vertex vert
        #pragma fragment frag
        #include "UnityCG.cginc"

        struct appdata
        {
            float4 vertex : POSITION;
            float2 uv : TEXCOORD0;

            // single pass instanced rendering
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        struct v2f
        {
            float2 uv : TEXCOORD0;
            float4 vertex : SV_POSITION;

            // single pass instanced rendering
            UNITY_VERTEX_INPUT_INSTANCE_ID
            UNITY_VERTEX_OUTPUT_STEREO
        };

        sampler2D _MainTex;
        float4 _MainTex_ST;

        float4 _Start, _Hit, _End;

        v2f vert (appdata v)
        {
            v2f o;

            // single pass instanced rendering
            UNITY_SETUP_INSTANCE_ID(v);
            UNITY_TRANSFER_INSTANCE_ID(v, o);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

            int myNum = floor(v.uv.y * 10);     //0Å`10Ç‹Ç≈
            float3 wv = mul(unity_ObjectToWorld, v.vertex).xyz;

            float3 vec = _Start - _End;
            wv += vec * myNum * .1;


            v.vertex = mul(unity_WorldToObject, float4(wv, 1));
                    
            o.vertex = UnityObjectToClipPos(v.vertex);
            o.uv = TRANSFORM_TEX(v.uv, _MainTex);
            return o;
        }
        ENDCG

        Pass
        {
            CGPROGRAM

            fixed4 frag (v2f i) : SV_Target
            {
                // single pass instanced rendering
                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                // sample the texture
                fixed4 col = 0;//tex2D(_MainTex, i.uv);
                col.r = i.uv.y;
                return col;
            }
            ENDCG
        }

        Pass
        {
            Tags{ "LightMode"="ShadowCaster" }
            
            CGPROGRAM
            #pragma target 3.0
            
            #pragma vertex vert
            #pragma fragment fragShadow
            #pragma multi_compile_shadowcaster

            fixed4 frag (v2f i) : SV_Target
            {
                // single pass instanced rendering
                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                return 0;
            }
            ENDCG   
        }
    }
}
