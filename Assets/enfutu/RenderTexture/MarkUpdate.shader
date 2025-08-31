Shader "enfutu/MarkUpdate"
{
    Properties
    {
        _Target ("Target_UV", Vector)  = (0,0,0,0)
        _MainTex ("Texture", 2D) = "black" {}
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

            sampler2D _MainTex, _Src;
            float4 _MainTex_ST, _Target;

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

                float a = (st.y * 4096) % 1;
                a = step(.3, a) * step(a, .7); 

                fixed4 src = tex2D(_Src, st);

                
                //double offset = 0.0024414062;

                int _x = 0;
                int _y = 0;

                int2 fst = floor(st * 4096);
                int2 fst_min = floor((st - 0.0024414062) * 4096);
                int2 fst_max = floor((st + 0.0024414062) * 4096);
                _Target = floor(_Target * 4096);

                if(fst_min.x < _Target.x && _Target.x < fst_max.x)
                { 
                    _x = 1;
                } 

                if(fst.y == _Target.y)
                { 
                    _y = 1;
                } 

                int isTouch = _x * _y * a;

                fixed4 col = saturate(src + isTouch);

                return col;
            }
            ENDCG
        }
    }
}
