
using UdonSharp;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using VRC.SDKBase;
using VRC.Udon;

public class Efude_mainSystem : UdonSharpBehaviour
{
    Ray ray;
    Ray ray2;
    RaycastHit hit;
    LayerMask layerMask;

    [SerializeField] Efude_FudeManager _fudeManagerSc; //全ての参照元

    //筆のONOFF
    [SerializeField] Efude_pickup _pickupSc;

    float size = 6.5f; //筆の長さ
    [SerializeField] float length; //筆の大きさを変更した時の倍率
    
    //lpos→local position　wPos→world position
    Vector3 lposFront;
    Vector3 lposBack;
    public Vector3 lposFollow;
    float inAngle;

    //筆の折れに関わる■■■■■■■■■
    public float disHit; //rayの衝突距離
    float sizeOffset; //sizeを100分の1した数。hit.distanceの数と対応。
    Vector3 posHit; //ray1の衝突位置
    Vector3 canvasNormal; //レイがヒットした紙の法線(紙に対し必ず90度の線)
    Quaternion canvasRote; //キャンバスの回転
    //平筆用■■■■■■■■■■■■■■
    [SerializeField] GameObject rayTarget1;
    [SerializeField] GameObject rayTarget2;
    Vector3 canvasNormal_1;
    Vector3 canvasNormal_2;
    float disHit_1;
    float disHit_2;
    Vector3 posHit_1;
    Vector3 posHit_2;
    float fudeSide; //平筆のどちら側を曲げるか。0～1。
    bool rayOn_1 = false;
    bool rayOn_2 = false;
    //■■■■■■■■■■■■■■■■■
    Vector3 freezePos; //筆がある深さに達したとき、保存されるFollowの位置。ある深さ以上では、Followがこの位置とFollowWposTargetの位置の間を移動する。
    float _lerp;//筆の曲がりの変化
    float o_lerp;//筆の曲がりの変化の1フレーム古いやつ。
    bool push = true; //筆を押しているならtrue引いているならfalse。
    
    [HideInInspector] public bool rayOn = false; //hitしたらパレットoffにするため

    //※シェーダ―で扱える座標はローカル座標のみ。

    void Start()
    {
        //■マネージャーからレイヤーマスク参照
        layerMask = 1 << _fudeManagerSc.collisionLayer;

        //初期化
        sizeOffset = size * length * 0.01f;
        disHit = size * length * 0.01f;
        size *= length;
        inAngle = 0;
        localpos(); //ポジションを取得
    }

    void Update()
    {
        if (_fudeManagerSc.fude_geometryMat == null) return;

        //ヒットしていなければ動かさない
        if (!rayOn) return;
        //レイキャストで情報を取得→筆先の位置の計算まで。
        if (_fudeManagerSc.sync_SizeParameter.w == 0) { raychan(); }
        else if(_fudeManagerSc.sync_SizeParameter.w == 1) { hira_raychan(); }

        localpos(); //ポジションを取得
        setMat(); //シェーダーへ渡す
    }

    void FixedUpdate()
    {
        //raycastのヒット判定はこっちで
        //ヒットしていたら動かない
        if (rayOn) return;
        if (_fudeManagerSc.sync_SizeParameter.w == 0) { raychan(); }
        else if (_fudeManagerSc.sync_SizeParameter.w == 1) { hira_raychan(); }
        ifRayOff(); //初期化   
    }


    private void localpos()
    {
        lposBack = this.transform.localPosition;
        lposBack.x = 0f;
        lposBack.y = 0f;//x,y軸は回転の影響もあって使いにくい値になってるから無視してしまう。

        //筆先の揺れを決めるオブジェクトの位置を取得
        _lerp = disHit / sizeOffset; //大体1～0の値を返す(筆と紙の距離→1が遠い、0が近い)
        if(_lerp <= 0) { _lerp = 0; }
        if(_lerp >= 1) { _lerp = 1; }

        //筆を押しているか引いているかの判定
        if(_lerp - o_lerp >= 0) //1フレーム前の方が紙に近かった。
        {
            push = false;
        }
        else //静止はpush判定
        {
            push = true;
        }
        o_lerp = _lerp; //古いlerpとして保存。

        _fudeManagerSc.FollowWposTargetAnim.SetFloat("mix", _lerp); //configrablejointのlimit値を_lerpに合わせて変更。

        //筆先の移動
        //ローカル座標上に変換。
        //↓元々はFollowオブジェクトを置いて位置を取ってた変数。計算できるようになったのでそのまま座標の入れ物として利用。
        Vector3 FollowPos = transform.InverseTransformPoint(_fudeManagerSc.FollowWposTargetOb.transform.position);
        
        
        //その後ローカル座標上での位置を決定。
        //平筆では使用しない
        if (push == true && _fudeManagerSc.sync_SizeParameter.w == 0)
        {
            freezePos = Vector3.Slerp(freezePos, FollowPos, _lerp + .7f); //微調整…_lerpにプラスすることで筆を持ち上げたときに変化しすぎないよう。VRでは不可能な抑えの補助的な意味合いになる…
            //抑えであればターゲットを固定する
            _fudeManagerSc.FollowWposTargetOb.transform.position = transform.TransformPoint(freezePos);
        }

        freezePos.z = FollowPos.z; //zの値のみリアルタイムに更新。

        //結果をシェーダ―に反映
        lposFollow = FollowPos;
        
    }

    private void ifRayOff()
    {
        //初期化
        disHit = sizeOffset;
        _lerp = 1;
        inAngle = 0;

        //ポジションの初期化
        lposBack = this.transform.localPosition;
        lposBack.x = 0f;
        lposBack.y = 0f;//x,y軸は回転の影響もあって使いにくい値になってるから無視してしまう。

        lposFront = lposBack;
        lposFront.z += sizeOffset;
        Vector3 lposFrontDef = lposFront; //左右に移動しないデフォルトのFront値

        _fudeManagerSc.WposFrontOb.transform.localPosition = lposFrontDef; //位置の初期化(ローカル座標にて)
        freezePos = lposFrontDef; //位置の初期化(ローカル座標にて)
        _fudeManagerSc.FollowWposTargetOb.transform.position = _fudeManagerSc.WposFrontOb.transform.position; //位置の初期化(親子関係が違うためワールド座標にて)

        lposFollow = lposFront;

        setMat(); //シェーダーへ渡す
    }

    private void raychan()
    {
        //レイで必要なものを取得
        ray = new Ray(transform.position, transform.forward);
        if (Physics.Raycast(ray, out hit, sizeOffset, layerMask)) 
        {
            rayOn = true;

            //必要な数字を取る
            canvasNormal = hit.normal;
            disHit = hit.distance;
            posHit = hit.point;
            canvasRote = hit.transform.rotation;

            getlposFront();//レイキャストで取得した情報から筆先の位置を決める
        }
        else
        {
            rayOn = false;
        }
    }

    private void hira_raychan()
    {
        //レイで必要なものを取得
        ray = new Ray(rayTarget1.transform.position, transform.forward);
        ray2 = new Ray(rayTarget2.transform.position, transform.forward);

        //Debug.DrawRay(rayTarget1.transform.position, transform.forward * sizeOffset, Color.red);
        //Debug.DrawRay(rayTarget2.transform.position, transform.forward * sizeOffset, Color.green);

        if (Physics.Raycast(ray, out hit, sizeOffset, layerMask))
        {
            //レイ1の要素を取得
            rayOn = true;
            rayOn_1 = true;

            //必要な数字を取る
            canvasNormal_1 = hit.normal;
            disHit_1 = hit.distance;
            posHit_1 = hit.point;
            canvasRote = hit.transform.rotation;
        }
        else { rayOn_1 = false; }

        if (Physics.Raycast(ray2, out hit, sizeOffset, layerMask))
        {
            //レイ2の要素を取得
            rayOn = true;
            rayOn_2 = true;

            //必要な数字を取る
            canvasNormal_2 = hit.normal;
            disHit_2 = hit.distance;
            posHit_2 = hit.point;
            canvasRote = hit.transform.rotation;
        }
        else { rayOn_2 = false; }

        //rayOnの判定
        if(!rayOn_1 && !rayOn_2) { rayOn = false; return; }

        //出力する値を決める。
        //両方のレイがhitしていれば
        if (rayOn_1 && rayOn_2)
        {
            //Debug.Log("両方が動いてる");
            //二本のレイからの情報を元に計算
            canvasNormal = (canvasNormal_1 + canvasNormal_2) * .5f;
            disHit = (disHit_1 + disHit_2) * .5f;
            
            float fudeSide_1 = disHit_1 / sizeOffset;
            float fudeSide_2 = fudeSide = 1 - disHit_2 / sizeOffset;
            fudeSide = (fudeSide_1 + fudeSide_2) * .5f;

            Debug.Log(fudeSide);
            posHit = Vector3.Lerp(posHit_1, posHit_2, fudeSide);
            posHit = Vector3.Lerp(posHit, this.transform.position + this.transform.forward * sizeOffset, disHit / sizeOffset);
            getlposFront();//レイキャストで取得した情報から筆先の位置を決める
        }
        //もしレイ1しかhitしていなければ
        else if (rayOn_1)
        {
            //Debug.Log("1が動いてる");
            canvasNormal = canvasNormal_1;
            disHit = disHit_1;
            fudeSide = .2f;
            posHit = posHit_1;
            posHit = Vector3.Lerp(posHit, this.transform.position + this.transform.forward * sizeOffset, disHit / sizeOffset);
            getlposFront();//レイキャストで取得した情報から筆先の位置を決める
        }
        //もしレイ2しかhitしていなければ
        else if (rayOn_2)
        {
            //Debug.Log("2が動いてる");
            canvasNormal = canvasNormal_2;
            disHit = disHit_2;
            fudeSide = .8f;
            posHit = posHit_2;
            posHit = Vector3.Lerp(posHit, this.transform.position + this.transform.forward * sizeOffset, disHit / sizeOffset);
            getlposFront();//レイキャストで取得した情報から筆先の位置を決める
        }
    }

    private void getlposFront()
    {
        //正規化(1)の法線に、size - disHitをかけて紙を突き抜けた長さ分の法線を作る
        //このオブジェクトが向いている方向のベクトル。
        Vector3 thisNormal = this.transform.forward * (sizeOffset - disHit);

        //Vector3 n = Vector3.ProjectOnPlane(thisNormal, -canvasNormal); //canvasの法線が反対でないと正しい値が出ない
        Vector3 n = thisNormal + canvasNormal * Vector3.Dot(thisNormal, -canvasNormal);

        inAngle = Vector3.Angle(thisNormal, -canvasNormal);

        Vector3 pos = posHit + (n); //筆先の目指す座標。近似値。

        //WposFrontオブジェクトを毛の曲がった先へ少しずつ移動させる
        _fudeManagerSc.WposFrontOb.transform.position = pos;
        _fudeManagerSc.WposFrontOb.transform.rotation = canvasRote; //回転を取らないと、canvasの傾きに応じた移動をFollowが行わない
        //ローカル座標をシェーダ―へ渡す
        lposFront = _fudeManagerSc.WposFrontOb.transform.localPosition;
    }

    private void setMat()
    {
        if (_fudeManagerSc.sync_SizeParameter.w == 0) 
        {
            _fudeManagerSc.fude_geometryMat.SetVector("_posC", lposFollow);
            _fudeManagerSc.fude_geometryMat.SetFloat("_hit", disHit);
            _fudeManagerSc.fude_geometryMat.SetFloat("_size", size);
            _fudeManagerSc.fude_geometryMat.SetFloat("_inAngle", inAngle);
        }
        else if (_fudeManagerSc.sync_SizeParameter.w == 1) 
        {
            _fudeManagerSc.hirafude_geometryMat.SetVector("_posC", lposFollow);
            _fudeManagerSc.hirafude_geometryMat.SetFloat("_hit", disHit);
            _fudeManagerSc.hirafude_geometryMat.SetFloat("_size", size);
            _fudeManagerSc.hirafude_geometryMat.SetFloat("_inAngle", inAngle);
            _fudeManagerSc.hirafude_geometryMat.SetFloat("_Side", fudeSide);
        }
    }

}