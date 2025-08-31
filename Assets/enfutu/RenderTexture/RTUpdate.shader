Shader "enfutu/RTUpdate"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "black" {}
        _WposMap ("WposMap", 2D) = "black" {}
        _Mark0 ("MarkTexture0", 2D) = "black" {}
        _Mark1 ("MarkTexture0", 2D) = "black" {}
        _Src ("Src", 2D) = "black" {}
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

            sampler2D _MainTex, _WposMap, _Mark0, _Mark1, _Src;
            float4 _MainTex_ST;

            float rand(float2 st)
            {
                return frac(sin(dot(st, float2(12.9898, 78.233))) * 43758.5453);
            }

            v2f vert (appdata v)
            {
                v2f o;

                // single pass instanced rendering
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // single pass instanced rendering
                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                
                float2 st = i.uv;

                float2 fst = floor(st * 4096) / 4096;
                float3 map = tex2D(_WposMap, float2(1 - fst.x, fst.y));

                fixed mark = saturate(tex2D(_Mark0, st).r + tex2D(_Mark1, st).r);

                //srcÇÕç∂âEÇ…Ç∏ÇÁÇ∑
                float offset = 0.0024414062;

                fixed src0 = tex2D(_Src, fst).a;
                fixed src1 = tex2D(_Src, floor((st + float2(offset, 0)) * 4096 ) / 4096).a * .98;
                fixed src2 = tex2D(_Src, floor((st - float2(offset, 0)) * 4096 ) / 4096).a * .98;

                fixed src = (src0 + src1 + src2) * .33333;

                fixed4 col = 0;
                col.a = saturate(src + mark);
                col.rgb = map * step(.001, col.a);

                return col;
            }
            ENDCG
        }
    }
}
