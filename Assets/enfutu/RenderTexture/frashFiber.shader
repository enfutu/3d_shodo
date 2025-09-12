Shader "enfutu/frashFiber"
{
    Properties
    {
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

            sampler2D _MainTex;
            float4 _MainTex_ST;

            int _MaxLength;
            float4 _Positions[100];

            v2f vert (appdata v)
            {
                v2f o;

                // single pass instanced rendering
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // single pass instanced rendering
                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                float2 st = i.uv;

                float a = (st.y * 4096) % 1;
                a = step(.3, a) * step(a, .7); 

                int2 fst = floor(st * 4096);
                int2 fst_min = floor((st - 0.0024414062) * 4096);
                int2 fst_max = floor((st + 0.0024414062) * 4096);
                
                int isTouch = 0;
                for(int i = 0; i < _MaxLength; i++)
                {
                    int _x = 0;
                    int _y = 0;

                    float2 target = floor(_Positions[i].xy * 4096);
                    if(fst_min.x < target.x && target.x < fst_max.x)
                    { 
                        _x = 1;
                    } 

                    if(fst.y == target.y)
                    {  
                        _y = 1;
                    }

                    isTouch += _y * a;
                }

                fixed4 col = saturate(isTouch);
                return col;
            }
            ENDCG
        }
    }
}
