Shader "enfutu/Canvas"
{
    Properties
    {
        _ID ("ID", int) = 0
        _MaxLength ("MaxLength", int) = 0
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue" = "Geometry+1"}
        LOD 100

        Cull Off
        Stencil
        {
            Ref 2
            Comp NotEqual
            Pass replace
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
            int _ID, _MaxLength;

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

                //float offset = 1 / (_MaxLength + .00001);
                //float num = offset * _ID;
                

                float2 st = i.uv;
                st.x = 1 - st.x;

                //ID : 0Å`100
                float _x = _ID % 10;        //0Å`10
                float _y = floor(_ID * .1); //0Å`10
                
                st += float2(_x, _y);
                
                st *= .1;

                fixed lod0 = tex2Dlod(_MainTex, float4(st, 0, 0)).r;
                fixed lod1 = tex2Dlod(_MainTex, float4(st, 0, 1)).r;
                fixed lod2 = tex2Dlod(_MainTex, float4(st, 0, 2)).r;
                
                //lod0 = step(.9, lod0);
                //lod1 = step(.54, lod1);
                //lod2 = step(.4, lod2);
                
                fixed marge = lod0;//saturate(lod0 + lod1 + lod2);// + lod2;
                
                //0Å`1ÇÃíl
                float depth = marge;
                
                clip(depth - .01);

                fixed4 col = 1;

                return col;
            }
            ENDCG
        }
    }
}
