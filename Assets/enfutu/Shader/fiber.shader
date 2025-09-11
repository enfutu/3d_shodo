Shader "enfutu/fiber"
{
    Properties
    {
        _Color ("Color", Color) = (0,0,0,0)
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "Queue" = "Geometry" }
        //Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        cull off

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
                float4 vc : COLOR;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 vc : COLOR;

                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_ST, _Color;

            v2f vert (appdata v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.vc = v.vc;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                float2 st = i.uv;

                fixed4 col = 1;//tex2D(_MainTex, st);
                
                col = length(tex2D(_MainTex, i.uv)) * _Color;
                
                              
                //float offset = lerp(.01, 1, col.a);
                float offset = lerp(.01, .01, col.a);

                //å©ÇΩñ⁄ÇêÆÇ¶ÇÈÇ‚Ç¬Å´
                float a = (st.y * 4096) % 1;
                a = step(.5 - offset, a) * step(a, .5 + offset); 
                clip(a - .1);
               
                return col;
            }
            ENDCG
        }
    }
}
