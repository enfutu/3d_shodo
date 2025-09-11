Shader "Unlit/canvasError"
{
    Properties
    {
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

            sampler2D _MainTex;
            float4 _MainTex_ST, _MainTex_TexelSize;

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v); //SPSI
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o); //SPSI
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed2 st = i.uv;
                fixed4 col = fixed4(1,0,0,1);
               
                //バツ印
                fixed lineA = step((st.x + st.y) * .5, .51);
                lineA -= step((st.x + st.y) * .5, .49);
                fixed lineB = step((st.x - st.y), .02);
                lineB -= step((st.x - st.y), -.02);
                
                lineA += lineB;
                lineA = 1 - lineA;

                //平衡投影カメラには写さない。
                lineA = lerp(1, lineA, step(UNITY_MATRIX_P[3][3], .5));
                clip(.5 - lineA);
                             
                return col;
            }
            ENDCG
        }
    }
}
