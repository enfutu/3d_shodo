Shader "Unlit/canvasPhoto"
{
    Properties
    {
        _PhotoSize("撮影される範囲", float) = 10
        _MainTex("Texture", 2D) = "white" {}
        _Washi("和紙テクスチャ", 2D) = "white" {}
        _PhotoModeTex ("PhtoModeTexture", 2D) = "black" {}
        //[Toggle] _IsVR("_IsVR(debug)", int) = 0
    }

        SubShader
        {
            Tags { "RenderType" = "Opaque" }

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

                struct v2f_vert
                {
                    float2 uv : TEXCOORD0;
                    float3 worldPosZero : TEXCOORD1;
                    float4 vertex : SV_POSITION;
                    half cameraToObjLength : TEXCOORD2;
                    UNITY_VERTEX_OUTPUT_STEREO //SPSI
                };

                struct v2f_frag
                {
                    float2 uv : TEXCOORD0;
                    float3 worldPosZero : TEXCOORD1;
                    half cameraToObjLength : TEXCOORD2;
                    UNITY_VPOS_TYPE vpos : VPOS;
                };

                sampler2D _MainTex, _Washi, _PhotoModeTex;
                float4 _MainTex_ST;
                float4 _MainTex_TexelSize;
                //int _IsVR;
                float _PhotoSize;

                v2f_vert vert(appdata v)
                {
                    v2f_vert o;
                    UNITY_SETUP_INSTANCE_ID(v); //SPSI
                    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o); //SPSI
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                    //撮影機能に関わる部分
                    //オブジェクト原点。ローカル座標系をワールド座標系に変換
                    half3 worldPosZero = mul(unity_ObjectToWorld, half4(0,0,0,1));
                    o.worldPosZero = worldPosZero;
                    
                    //メモ//_WorldSpaceCameraPos == UNITY_MATRIX_I_V._m03_m13_m23
                    half cameraToObjLength = length(_WorldSpaceCameraPos - worldPosZero);
                    o.cameraToObjLength = cameraToObjLength;

                    return o;
                }

                half4 frag(v2f_frag i) : SV_Target
                {
                    half2 st = i.uv;
                    //反転
                    st.y = 1 - st.y;
                    half cameraToObjLength = i.cameraToObjLength;

                    half4 col = tex2D(_MainTex, st);
                    col *= tex2D(_Washi, st) + .2;
                    col -= tex2D(_PhotoModeTex, half2(1 - st.x, st.y));

                    //メイン画像の切り取り
                    half Frame = step(.05, st.x); 
                    Frame -= step(.95, st.x);
                    Frame -= step(.95, st.y);
                    Frame -= step(st.y, .05);
                    Frame = step(.5, Frame);       

                    col *= Frame;
                    
                    //col *= Frame;
                    //col += Frame2 * half4(.9, .9, .7, 1);
                    
                    //■■■■■■ここから撮影機能に関わる記述■■■■■■

                    //stにスクリーンの座標を突っ込んで正規化
                    half2 st2 = i.vpos.xy;
                    st2 = (st2 * 2 - _ScreenParams.xy) / min(_ScreenParams.x, _ScreenParams.y);

                    //テクスチャが縦長ならUVを回転。
                    st2 = lerp(st2.yx, st2.xy, step(_MainTex_TexelSize.w, _MainTex_TexelSize.z));
                    //回転した場合は反転させる
                    st2.y *= lerp(-1, 1, step(_MainTex_TexelSize.w, _MainTex_TexelSize.z));
                    //1:1なので画像のサイズに引き延ばす。
                    //画像の比
                    half a = _MainTex_TexelSize.z / _MainTex_TexelSize.w;
                    st2.y *= a;

                    //中央へ
                    st2 = ((st2 - st2 * .5 + .5) * 2) + st2 * .5 - .5;

                    //拡縮
                    //half cameraToObjLength = i.cameraToObjLength;
                    half size = lerp(.1,_PhotoSize,cameraToObjLength - _ProjectionParams.y); //lerp(最小,最大,カメラとオブジェクトの距離 - nearClipping)
                    st2 = ((st2 - .5) * size) + .5;

                    //撮影用の像を作る
                    st2.x = 1 - st2.x;
                    half4 nearFace = tex2D(_MainTex, st2);
                    //和紙テクスチャ
                    nearFace *= tex2D(_Washi, st2) + .2;

                    //画像が伸びないように切り取る
                    //▼これだとQuestでエラーが出る
                    //nearFace = lerp(half4(1,.5,.5,1), nearFace, step(st.x, 1)* step(0, st.x) * step(st.y, 1) * step(0, st.y));
                    nearFace = lerp((half4)0, nearFace, step(st2.x, 1));
                    nearFace = lerp((half4)0, nearFace, step(0, st2.x));
                    nearFace = lerp((half4)0, nearFace, step(st2.y, 1));
                    nearFace = lerp((half4)0, nearFace, step(0, st2.y));

                    //距離で切り替え
                    half4 colPhoto = col;
                    colPhoto = lerp(col, nearFace, step(cameraToObjLength, _ProjectionParams.y + .1));

                    //■■■■■■撮影機能終わり■■■■■■

                    //▼どんな画面でも見れるようにしたので削除
                    //デスクトップか、VR機器を通して見ているのであればcolを使用。
                    /*
                    col = lerp(colPhoto, col, step(.5, UNITY_MATRIX_P[3][3] + (1 - _IsVR)
                           
                           #if defined(USING_STEREO_MATRICES)
                           +1
                           #endif
                           )); 
                     */
                                  
                    
                    return colPhoto;
                }
                ENDCG
            }
        }
}
