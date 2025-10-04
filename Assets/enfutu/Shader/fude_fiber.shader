Shader "enfutu/fude_fiber"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue" = "Geometry"}
        LOD 100

        Cull Off
        
        Stencil
        {
            Ref 2
            Comp always
            Pass replace
        }

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
            float3 wv : TEXCOORD1;
            float value : TEXCOORD2;

            // single pass instanced rendering
            UNITY_VERTEX_INPUT_INSTANCE_ID
            UNITY_VERTEX_OUTPUT_STEREO
        };

        sampler2D _MainTex;
        float4 _MainTex_ST;

        float4 _Start, _Hit, _End, _EndBase;
        float _HitDist;        

        //float4 _Center;
        //float _InnerRange;

        v2f vert (appdata v)
        {
            v2f o;

            // single pass instanced rendering
            UNITY_SETUP_INSTANCE_ID(v);
            UNITY_TRANSFER_INSTANCE_ID(v, o);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

            int myNum = floor(v.uv.y * 10);     //0Å`10Ç‹Ç≈

            float3 wv = mul(unity_ObjectToWorld, v.vertex).xyz;
            float3 _wv = wv;
            //2éüÉxÉWÉFã»ê¸
            float3 p0 = lerp(_Start, _Hit, myNum * .1);
            float3 p1 = lerp(_Hit, _End, myNum * .1);
            float3 p2 = lerp(p0, p1, myNum * .1);
            wv += p2 - _Start;
            //íºê¸
            _wv += lerp(_Start, _EndBase, myNum * .1) - _Start;

            float deformPow = length(wv - _wv);
            deformPow = saturate(deformPow * 5);
            o.value = deformPow;
            
            float ortho = unity_OrthoParams.w;
            if(ortho == 1)
            {
                v.vertex.xyz *= max(0, (1 - v.uv.y)) * deformPow;
            }

            //à⁄ìÆó í≤Ç◊ÇƒëæÇ≥ÇïœÇ¶ÇΩÇÃÇ≈Ç‡Ç§àÍâÒã»Ç∞Ç»Ç®Ç∑ 
            wv = mul(unity_ObjectToWorld, v.vertex).xyz;
            wv += p2 - _Start;
            o.wv = wv;

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
                //col.g += _Sumi * .01;
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
            //#pragma fragment fragShadow
            //#pragma multi_compile_shadowcaster

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
