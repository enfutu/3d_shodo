Shader "enfutu/generate"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _TexelProbability("Texel Probability", Range(0, 1)) = 0.25
        _RandomSeed("Random Seed", Range(0, 1)) = 0
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
                float4 pos : SV_POSITION;

                // single pass instanced rendering
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _TexelProbability, _RandomSeed;

            v2f vert (appdata v)
            {
                v2f o;

                // single pass instanced rendering
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            uint pcg_hash(uint seed)
			{
				uint state = seed * 747796405u + 2891336453u;
				uint word = ((state >> ((state >> 28u) + 4u)) ^ state) * 277803737u;
				return (word >> 22u) ^ word;
			}

            fixed4 frag (v2f i) : SV_Target
            {
				uint seed = pcg_hash(asuint(_RandomSeed) ^ pcg_hash((uint)i.pos.x ^ pcg_hash((uint)i.pos.y)));
				if (seed * exp2(-32) > _TexelProbability)
					return 0;
				uint4 random;
				random.x = pcg_hash(seed);
				random.y = pcg_hash(random.x ^ seed);
				random.z = pcg_hash(random.y ^ seed);
				random.w = pcg_hash(random.z ^ seed);
				float4 color = random * exp2(-32);
				color.rgb = pow(color.rgb, 2.2);
				return color;
            }
            ENDCG
        }
    }
}
