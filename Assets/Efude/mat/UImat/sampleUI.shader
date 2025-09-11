Shader "Unlit/sampleUI"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Move ("MoveTexture", 2D) = "white" {}
        _Color ("Color", COLOR) = (0,0,0,1)
        _Color2("Color2", COLOR) = (0,0,0,1)
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
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex, _Move;
            float4 _MainTex_ST, _Color, _Color2;

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                col += step(col,.9) * _Color + tex2D(_Move, fixed2(i.uv.x + sin(_Time.y) * .1, i.uv.y + _Time.y * .2)) * _Color2;

                return col;
            }
            ENDCG
        }
    }
}
