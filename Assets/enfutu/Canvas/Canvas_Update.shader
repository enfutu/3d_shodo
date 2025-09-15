Shader "Unlit/Canvas_Update"
{
    Properties
    {
        _Src ("Src", 2D) = "black"{}
        _MainTex ("Texture", 2D) = "black" {}
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

            sampler2D _MainTex, _Src;
            float4 _MainTex_ST;

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

                //É^ÉCÉäÉìÉOÇ≥ÇÍÇΩç¿ïWÇ≈éÊìæ
                float2 cst = frac(st * 10);
                fixed4 col = tex2D(_MainTex, cst);


                float depth = col.r;            //0Å`1
                //float offset = .18;             //ïMÇÃà íuÇ…çáÇÌÇπÇÈÇΩÇﬂÇÃî˜í≤êÆ
                //depth += offset * step(.0001, depth);
                
                float2 fst = floor(st * 10);    //1Å`10

                float2 id = floor(depth * 100); //0Å`100
                id.x = (id.x % 10);             //0Å`10
                id.y = floor(id.y * .1);        //0Å`10
                
                float _x = 0;
                float _y = 0;

                if((fst.x - 1) < id.x && id.x < (fst.x + 1))
                {
                    _x = 1;
                }

                if((fst.y - 1) < id.y && id.y < (fst.y + 1))
                {
                    _y = 1;
                }

                float input = _x * _y;
                depth *= input;

                //float check = tex2Dlod(_Src, float4(st, 0, 1)).r;
                
                float src = tex2D(_Src, st).r;
                //if(.5 < check)
                //{
                //    src = check;
                //}



                

                

                col = step(.0001, src.r + depth);



                return col;
            }
            ENDCG
        }
    }
}
