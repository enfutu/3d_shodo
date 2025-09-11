Shader "Unlit/pallet"
{
    Properties
    {
        _MainTex ("UIの背景", 2D) = "white" {}
        _Map ("UIマップ" , 2D) = "white" {}
        _Noise ("Noise" , 2D) = "white" {}
        _UseColorPicker("UseColPick", int) = 0
        _ColorRGBA ("ColorAll", Vector) =(0,0,0,0)
        _Mode("MODE", Vector) = (0,0,0,0)
    }
    SubShader
    {
        Tags {  "RenderType"="Transparent" "Queue" = "Transparent" }
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

            sampler2D _MainTex, _Map, _Noise;
            float4 _MainTex_ST, _ColorRGBA, _Mode;
            float _UseColorPicker;

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v); //SPSI
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o); //SPSI
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                float2 st = i.uv;

                // sample the texture
                half4 map = tex2D(_Map, i.uv);
                half noise = tex2D(_Noise, i.uv - half2(sin(_Time.y) * .05, _Time.y * .1));

                half4 col;

                //カラーパレットの作成
                //分割
                float R = step(.5, map.r) * step(map.g, .5) * step(map.b, .5);
                float G = step(map.r, .5) * step(.5, map.g) * step(map.b, .5);
                float B = step(map.r, .5) * step(map.g, .5) * step(.5, map.b);
                float A = step(.5, map.r) * step(.5, map.g) * step(map.b, .5);

                half stHalf = i.uv.y;
                stHalf -= .5;
                
                float lineSize = .005;
                
                half4 LineOffsetColor = lerp(.05, .45, _ColorRGBA);

                float4 LineR = step(stHalf, LineOffsetColor.r + lineSize) * step(LineOffsetColor.r - lineSize, stHalf) * R;
                float4 LineG = step(stHalf, LineOffsetColor.g + lineSize) * step(LineOffsetColor.g - lineSize, stHalf) * G;
                float4 LineB = step(stHalf, LineOffsetColor.b + lineSize) * step(LineOffsetColor.b - lineSize, stHalf) * B;
                float4 LineA = step(stHalf, LineOffsetColor.a + lineSize) * step(LineOffsetColor.a - lineSize, stHalf) * A;
            
                col = LineR + LineG + LineB + LineA;

                //Mode
                _Mode.x -= 100; //Refの値を100～101で指定しているため0～1にする。
                _Mode.y = 1 - _Mode.y; //入れ替え
                float Mode_x = step(.655, i.uv.x) * step(i.uv.x, .695); //本来は.65<i.uv.x<.7だけど見た目を良くするために小さく
                float Mode_y_Wet = step(.905, i.uv.y) * step(i.uv.y, .945) * _Mode.x;
                float Mode_y_stickiness = step(.805, i.uv.y) * step(i.uv.y, .845) * _Mode.y;
                int ModeMark = (Mode_y_Wet + Mode_y_stickiness) * Mode_x;
                col += half4(1, 1, 1, 1) * ModeMark;

                half4 Tex = tex2D(_MainTex, i.uv);
                col.a = Tex.a;

                //Wetの時のエフェクト
                float TexWetSpace = step(.47, i.uv.x) * step(i.uv.x, .63) * step(.5, i.uv.y);
                half4 TexWet = tex2D(_MainTex, i.uv + half2(noise - .25, 0) * .2 * (1 - i.uv.y));
                TexWet *= TexWetSpace;
                //Wetエフェクト貼り付け
                Tex = lerp(Tex, TexWet + Tex *(1 - TexWetSpace), step(_Mode.x, .5));

                //Stickinessの時のエフェクト
                half Asian = distance(frac((i.uv + half2(0, _Time.y * -.01)) * 40), .5);
                Asian = step(.5, Asian) * (1 - i.uv.y);
                half Asian2 = distance(frac((i.uv + half2(0, _Time.y * -.01)) * 20), .5);
                Asian2 = step(.3, Asian2) * (1 - i.uv.y);
                Asian -= Asian2;
                Asian *= TexWetSpace;
                //Stickinessエフェクト貼り付け
                Tex -= Asian * step(_Mode.y, .5);


                col.rgb += lerp(Tex, _ColorRGBA.rgb, map.r) * step(st.y, .5); //下カラーパレット
                col.rgb += Tex * (1 - step(st.y, .5)); //カラーパレットを除いたカラー
                
                return col;
            }
            ENDCG
        }
    }
}
