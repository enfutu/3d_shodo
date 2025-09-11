Shader "Unlit/stencilFilter1"
{
    Properties
    {
        //_MainTex ("Texture", 2D) = "white" {}
        _TexSize ("TexSize", int) = 0
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Geometry" }
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 100

        //Ref
        //100:stencilFilter0
        //101:stencilFilter1
        Stencil
        {
            Ref 101
            Comp Less //自身のRefが他より小さいとき(キャンバスより小さい)コンパイル
            Pass Replace //リファレンス値を置き換え
        }

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
                UNITY_VERTEX_INPUT_INSTANCE_ID //SPSI
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO //SPSI
            };

            //sampler2D _MainTex;
            //float4 _MainTex_ST, _MainTex_TexelSize;
            int _TexSize;


            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v); //SPSI
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o); //SPSI
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float fmod(float x, float y)
            {
                return x - y * floor(x / y);
            }

            half4 frag(v2f i) : SV_Target
            {
                if (UNITY_MATRIX_P[3][3] == 0) { discard; }

                float2 st = i.uv;
                //偶数行に色をつける
                float stripe = floor(st.x * _TexSize);
                stripe = fmod(stripe, 2);
                stripe = step(stripe, 0);
                
                half4 col = stripe;
                
                clip(0 - stripe);                          

                return col;
            }
            ENDCG
        }
    }
}
