Shader "Unlit/canvasBack"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [Toggle] _Reset ("リセット", int) = 0
        [Toggle] _Dry("乾きのON/OFF", int) = 0
        _Noise ("Noise", 2D) = "white" {}
    }
        SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry-1"}
        LOD 100

        Stencil
        {
            Ref 200
            Comp Greater //自身のRefが他より大きいときコンパイル
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

            sampler2D _MainTex, _Noise;
            float4 _MainTex_ST, _MainTex_TexelSize;
            int _Reset, _Dry;

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v); //SPSI
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o); //SPSI
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }


            float fmod(float x, float y)
            {
                return x - y * floor(x / y);
            }

            float4 frag (v2f i) : SV_Target
            {  
                if (UNITY_MATRIX_P[3][3] == 0) { discard; }

                float2 st = i.uv;
               
                //偶数行
                float Eve = step(fmod(st.x * _MainTex_TexelSize.z, 2), 1);
                //奇数行
                float Odd = step(1, fmod(st.x * _MainTex_TexelSize.z, 2));

                //half noise = tex2D(_Noise, st + _Time.y * .1).r;

                float2 stIn = st;
                //偶数行であれば1ピクセル左から取得。
                stIn.x = lerp(stIn.x, stIn.x - _MainTex_TexelSize.x, Eve);
                //奇数行は元の色の保存領域として扱う。
                float2 stBaf = st;
                stBaf.x = lerp(stBaf.x, stBaf.x + _MainTex_TexelSize.x, Odd);

                half4 col;
                half4 colIn = tex2D(_MainTex, stIn);
                half4 colBaf = tex2D(_MainTex, stBaf);
  
                //InとBafどちらに対し筆が書き込まれているかを調べる
                //なんかよく分かんないけど、色で確認した感じこれで行ってる。
                //Baffer⇒Inputの一巡だけFragが立っている。
                //【重要】筆のアルファ値が1では上手く通らない。最大値は.99とかにすると良い。
                int RevFrag = step(colIn.a, colBaf.a); //Bafに書き込まれていれば0を返す。
                //確認用↓
                //if (RevFrag == 1) { return half4(1, 0, 0, 1); } else { return half4(0, 1, 0, 1); }
                
                //Reverse判定。Bafに書き込まれているならば、BafとInを入れ替える。
                //alphaは1にする。でなければDryなカラーがInputに持ち込まれた時Wetに変化してしまうから。
                colIn.rgb = lerp(colBaf.rgb, colIn.rgb, RevFrag);
                colIn.a = lerp(1, colIn.a, RevFrag);
                colBaf.rgb = lerp(colIn.rgb, colBaf.rgb, RevFrag);
                colBaf.a = lerp(1, colBaf.a, RevFrag);

                
                //Inのアルファ値が低くなるほど高くなるint値a
                int a = floor(colIn.a * 10);
                a = step(a, 1) + step(a, 2) + step(a, 3) + step(a, 4);
                a *= 4; //にじみの範囲。広すぎると思わぬ場所の色を拾ってきてしまうし、小さいとQuestで変な色が出る

                half2 offset = half2(_MainTex_TexelSize.x * 2, _MainTex_TexelSize.y) * a;

                //にじんだ色。
                half4 colWet = tex2D(_MainTex, stIn + half2(offset.x, 0)) +
                               tex2D(_MainTex, stIn + half2(0, offset.y)) +
                               tex2D(_MainTex, stIn - half2(offset.x, 0)) +
                               tex2D(_MainTex, stIn - half2(0, offset.y));
                colWet *= .25;

                //元の色をにじませた色。//変な色が出る原因になっていたのでバッファをにじませるのはやめる。
                /*half4 colWetBaf = tex2D(_MainTex, stBaf + half2(offset.x, 0)) +
                                  tex2D(_MainTex, stBaf + half2(0, offset.y)) +
                                  tex2D(_MainTex, stBaf - half2(offset.x, 0)) +
                                  tex2D(_MainTex, stBaf - half2(0, offset.y));*/

                //白が混ざって良くないので全て反転。
                //colIn.rgb = 1 - colIn.rgb;
                colWet.rgb = 1 - colWet.rgb;
                colBaf.rgb = 1 - colBaf.rgb;
                //colWetBaf.rgb = 1 - colWetBaf.rgb;

                //混ぜる
                colWet.rgb = lerp(colBaf.rgb, colWet.rgb, RevFrag * .5); //0であればBafを、1であれば混色
             
                //渇きの速度。
                colWet.a += lerp(.001, .01, _Dry); //常に乾燥させ続けなければwetとdryの判定が取れない
                                             
                //反転を戻す。マイナスの値があると白が出てくるのでクランプする。
                colWet.rgb = 1 - clamp(colWet.rgb, 0, 1) ;

                col = colWet;
                col.a = lerp(col.a, 1.1, step(1, col.a)); //1以上なら1.1

                //リセット
                col = lerp(col, float4(1,1,1,1), _Reset);

                
                return col;
            }
            ENDCG
        }
    }
}
