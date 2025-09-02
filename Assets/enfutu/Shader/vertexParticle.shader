Shader "enfutu/vertexParticle"
{
    Properties
    {
        _Map ("Map", 2D) = "black" {}
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        cull off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float2 uv2 : TEXCOORD1;

                // single pass instanced rendering
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float2 uv2 : TEXCOORD1;

                // single pass instanced rendering
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex, _Map;
            float4 _MainTex_ST;

            float3 rotateByQuat(float3 v, float4 q)
            {
                // q = (xyz, w), 単位クォータニオン
                float3 t = 2.0f * cross(q.xyz, v);
                return v + q.w * t + cross(q.xyz, t);
            }

            float4 quatFromTo(float3 from, float3 to)
            {
                float3 f = normalize(from);
                float3 t = normalize(to);
                float d = dot(f, t);

                // ほぼ逆向き（180度回転）対策：任意の直交軸で回す
                if (d < -0.999999f)
                {
                    float3 axis = normalize(abs(f.x) > 0.5 ? cross(f, float3(0,1,0))
                                               : cross(f, float3(1,0,0)));
                    return float4(axis, 0); // w=0 で 180度
                }

                float3 c = cross(f, t);
                float w = 1.0f + d;
                float4 q = float4(c, w);
                return normalize(q);
            }

            v2f vert (appdata v)
            {
                v2f o;

                // single pass instanced rendering
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                float4 map = tex2Dlod(_Map, float4(v.uv, 0, 0));
                float3 center = map.xyz;

                if(length(center) < .0001) { center = 10000; }

                float3 nextcenter = tex2Dlod(_Map, float4(float2(v.uv.x + 0.00024414062, v.uv.y), 0, 0));
                float3 vec = normalize(center - nextcenter);

                float3 n0 = float3(0,1,0);
                float4 q  = quatFromTo(n0, vec);

                float3 wv = mul(unity_ObjectToWorld, v.vertex).xyz * .2;
                wv = rotateByQuat(wv, q);

                wv += center;

                v.vertex =  mul(unity_WorldToObject, float4(wv, 1));

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv2 = TRANSFORM_TEX(v.uv2, _MainTex);

                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // single pass instanced rendering
                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                float2 st = i.uv2;

                // sample the texture
                fixed4 col = tex2D(_MainTex, st);
                col = 0;

                col.rg = st;

                float dist = 1 - length(st - .5);

                clip(dist - .5);


                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
