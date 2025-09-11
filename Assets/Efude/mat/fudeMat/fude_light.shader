Shader "Unlit/fude_light"
{
 Properties
    {
        _Ref ("Ref", float) = 100
        _NoiseTex ("Noise", 2D) = "white" {} 
        _Color("color", Vector) = (0,0,0,1)
        _SetSize("SetSize", float) = 1
        _Illustration("Illustration", float) = 0
        //_posA("posFront", Vector) = (0, 0, 0, 0)
        //_posB("posBack", Vector) = (0, 0, 0, 0)
        [HideInInspector]_posC("posFollow", Vector) = (0, 0, 0, 0)
        [HideInInspector]_hit("hit", float) = 0.065
        [HideInInspector]_size("size", float) = 6.5
        //[HideInInspector]_thirst("thirst", float) = 0
    }

        SubShader
        {

        Tags { "RenderType"="Transparent" "Queue" = "Geometry+1" }
        Blend SrcAlpha OneMinusSrcAlpha 
        cull off
        LOD 100

        //Ref
        //100:stencilFilter0…Inputに書き込む
        //101:stencilFilter1…Bafに書き込む
        //NotEqualであれば上書き(消去)するので、使いたい方とリファレンス値をEqualにする。
        Stencil
        {
            Ref [_Ref]
            Comp NotEqual
            Pass IncrSat
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
            float4 vertexCol : COLOR;
            float3 normal : NORMAL;
            float3 tangent : TANGENT;
            UNITY_VERTEX_INPUT_INSTANCE_ID //SPSI
        };

        struct v2f
        {
            float4 pos : SV_POSITION;
            float2 uv : TEXCOORD0;
            float4 vertexCol : COLOR;
            float3 normal : NORMAL;
            UNITY_VERTEX_OUTPUT_STEREO //SPSI
        };

        sampler2D _NoiseTex;
        half4 _Color,_NoiseTex_ST, _posC;
        half _SetSize, _hit, _size, _inAngle, _Illustration, _Ref;

        //デバッグ用
        half _A, _B, _C, _Test;
          

        //half4 _posA;
        //half4 _posB;
        //half _thirst;

        v2f vert(appdata v)
        {
            v2f o;
            UNITY_SETUP_INSTANCE_ID(v); //SPSI
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o); //SPSI

            half2 st = v.uv;
            o.uv = st;

            half4 vertex = v.vertex; 

            o.vertexCol = v.vertexCol;
            half3 vc = v.vertexCol;

            //_SetSize→　太:1～0:細　へ
            //_SetSize = _SetSize;
            //_inAngle→ 直立が0
            _inAngle = _inAngle / 90;
            

            //↓筆のサイズ(_SetSize)を変更した場合、その分値を乗算して曲がりの大きさを変更する必要あり。また、1以下になってはならない。
            half3 posC = _posC.xzy * ((1 / _SetSize) + .1); //Followに追従する先端の位置。

            half hitDist = _hit * 100; //レイで測った壁までの距離。スクリプト側のsizeの1/100。
            hitDist = hitDist / _size; //正規化。hitDistがきっちりな数にならないのでちょっとズレる。
            half offset = 1 - hitDist; //紙に近づくほど1になる値
            half curvePower = distance(posC.xz, half2(0, 0)) * 10; //曲がりの強さの代替

            //posCと対応させる
            half2 curve = half2(posC.x * vc.r, -posC.z * vc.r) * lerp(1, .1, _SetSize); //細い筆が曲がりすぎないよう
            curve = lerp(curve * (clamp(st.x - .3, 0, 1.5 * _SetSize)), curve, offset); //細い筆がばらけすぎないよう
            curve = lerp(curve, curve * .001, _inAngle); //傾いている筆が曲がりすぎないよう
            vertex.xz += curve;
          
            //曲がった先で法線の再計算
            half3 tangent = v.tangent;
            half3 binormal = normalize(cross(v.normal, tangent));

            half delta = 0.2;
            half3 posT = (v.vertex + tangent * delta);
            half3 posB = (v.vertex + binormal * delta);
            posT.xz += curve;
            posB.xz += curve;

            half3 modifiedTangent = posT.xyz - v.vertex.xyz;
            half3 modifiedBinormal = posB.xyz - v.vertex.xyz;
            
            half3 modifiedNormal = normalize(cross(modifiedTangent, modifiedBinormal));
            o.normal = modifiedNormal;

            half3 canvasNormal = half3(0, 1, 0); //仮
            half a = abs(dot(o.normal, canvasNormal));
            //ここまで

            //illustrationモード
            half stickyPower = lerp(.035, .001, _Illustration);
            half openMargin = lerp(.2, .5, _Illustration);

            //筆の開き
            half2 open = lerp(0, modifiedNormal.xz * stickyPower, clamp(offset - openMargin, 0, 1));
            //開いた時の座標を頂点カラーに応じて決定
            open = lerp(vertex.xz, vertex.xz + open, vc.r); 
            //開かないとき    
            half2 openMin = lerp(vertex.xz, half2(posC.x, -posC.z) * lerp(vc.r * vc.r, 1, offset), vc.r);

            //開きの計算終わり。
            //斜めに筆を当てたときより開きやすいように。(絵画のタッチが面白くなるように)
            vertex.xz = lerp(openMin, open, offset);
            
            //開いた筆をつぶす
            //法線方向へaの値に応じてつぶす
            half2 close = modifiedNormal.xz * (lerp(0, .1, a));
            //つぶしの度合いは筆の入りの深さで変動。
            //筆が傾いているほどつぶすように。
            vertex.xz -= lerp(-close * lerp(0, 3, _SetSize) , clamp(close * 10, 0, lerp(0, .035, _inAngle)), smoothstep(.1, 1, offset * .9 + curvePower * _SetSize)); //筆の入りが浅いときは法線方向へ開き、深いときはつぶす。

            //筆の弾力を感じるためにY軸方向へ動かす。筆が浅いときは動かさない。
            //筆が細いときにもあまり動かさない。st.xで筆の中心ほど初期値を下におろす(細い線に周囲の毛が混じらないように)
            //筆が倒れているときはあまり動かさない。
            vertex.y -= lerp(.005 * st.x, lerp(0, lerp(stickyPower, 0, _inAngle), _SetSize), offset) * vc.r;
 
            o.pos = UnityObjectToClipPos(vertex);

            return o;
        }

        half4 frag(v2f i, half facing : VFACE) : SV_Target
        {
            half2 st = i.uv;
            //half noise = tex2D(_NoiseTex, st);
  
            half noise = tex2D(_NoiseTex, frac(st * 50));
            
            half4 col = half4(_Color.rgb, 1);
            
            if (!UNITY_MATRIX_P[3][3] == 0){
                col.a *= _Color.w;
                col.a *= step(noise * step(101, _Ref), _Color.w);
            }
            else 
            {
                col.rgb = lerp(col.rgb, col.rgb * .1, step(facing, 0));
            }

            //デバッグ
            //col.rgb = i.normal.rgb * 10;
            //col.xy = st.xy;
            //col.xyz = (half3)_inAngle /90;
            return col;
        }
        ENDCG
        }
    }
}