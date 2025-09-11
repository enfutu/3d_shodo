Shader "Unlit/canvasGrid"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
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

            half4 frag(v2f i) : SV_Target
            {
                half2 st = i.uv;
                half4 col = half4(1,0,0,.75);
               
                //グリッド
                int w = _MainTex_TexelSize.z / 125;
                int h = _MainTex_TexelSize.w / 125;
                half gridw = frac(st.x * w);
                half gridh = frac(st.y * h);
                half grid = step(.98, gridw) + step(gridw, .02) + step(.98, gridh) + step(gridh, .02);
                grid = 1 - grid;

                //平衡投影カメラには写さない。
                grid = lerp(1, grid, step(UNITY_MATRIX_P[3][3], .5));
                clip(.5 - grid);
                             
                return col;
            }
            ENDCG
        }
    }
}
