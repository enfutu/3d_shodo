Shader "enfutu/sumi_gpuparticle"
{
    Properties
    {
        _Map ("Map", 2D) = "white"{}
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                //uint vid : SV_VertexID
                float4 texcoord1 : TEXCOORD1;
                float4 texcoord2 : TEXCOORD2;
                float4 texcoord3 : TEXCOORD3;
                // single pass instanced rendering
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2g
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                //uint vid : SV_VertexID
                //float4 texcoord1 : TEXCOORD1;
                //float4 texcoord2 : TEXCOORD2;
                //float4 texcoord3 : TEXCOORD3;
            };

            struct g2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                //float4 texcoord1 : TEXCOORD1;
                //float4 texcoord2 : TEXCOORD2;
                //float4 texcoord3 : TEXCOORD3;

                // single pass instanced rendering
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex, _Map;
            float4 _MainTex_ST;

			v2g vert (appdata v)
			{
				v2g o;
				o.vertex = v.vertex;
				o.uv = v.uv;
                //o.vid = v.vid;
                //o.texcoord1 = v.texcoord1;
                //o.texcoord2 = v.texcoord2;
                //o.texcoord3 = v.texcoord3;
				return o;
			}


            static const float3 p[4] = 
            {
                float3(-1, -1, 0),
                float3(-1,  1, 0),
                float3( 1, -1, 0),
                float3( 1,  1, 0)
            };

            //複製できる頂点数は、出力する構造体の(1024 / 要素数)となる。
            [maxvertexcount(168)]
            void geom (triangle v2g input[3], inout TriangleStream<g2f> stream) 
            {
                //中心座標を得る
                //float3 wv0 = mul(unity_ObjectToWorld, input[0].vertex).xyz;
                //float3 wv1 = mul(unity_ObjectToWorld, input[1].vertex).xyz;
                //float3 wv2 = mul(unity_ObjectToWorld, input[2].vertex).xyz;
                //float3 center = (wv0 + wv1 + wv2) * .333;

                float size = .1;

                g2f o[168];

                [unroll]
                for (int i = 0; i < 20; i++)
                {
                    int id = floor(i / 4);
                    float _x = id % 4096;
                    float _y = floor(id / 4096);
                    float2 st = (float2(_x, _y) + .5) / 4096;

                    float3 center = tex2Dlod(_Map, float4(st, 0, 0));

                    //if(length(center) <= .01){ center = float3(-1000, -1000, -1000); }

                    float3 lv = center + p[i % 4] * size;
                    float4 vert = mul(unity_WorldToObject, float4(lv, 1));
                    o[i].vertex = UnityObjectToClipPos(vert); 
                    o[i].uv = 0;
                    stream.Append(o[i]);
                }

                stream.RestartStrip();
            }


            fixed4 frag (g2f i) : SV_Target
            {
                // single pass instanced rendering
                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDCG
        }
    }
}
