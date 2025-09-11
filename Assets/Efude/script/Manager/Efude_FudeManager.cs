
using UdonSharp;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using VRC.SDKBase;
using VRC.Udon;

public class Efude_FudeManager : UdonSharpBehaviour
{

    [Header("★筆システムの参照元を全てここで設定")]
    [Header("―――　筆システムで使用するレイヤー番号　―――")]
    [Header("〇キャンバスに書き込まれるレイヤー")]
    public int writeLayer;
    [Header("〇筆が衝突するレイヤー")]
    public int collisionLayer;
    [Header("〇パレットが所属するレイヤー(人にぶつからないレイヤーを推奨)")]
    public int palletLayer;

    [Header("―――　参照するスクリプト　―――")]
    public Efude_pickup _pickupSc;
    public Efude_mainSystem _mainSystemSc;
    public Efude_pickerCamera _pickerCameraSc;

    [Header("―――　参照するオブジェクト　―――")]
    [HideInInspector] public GameObject FudeManagerOb;
    public GameObject fude_mainSystemOb;
    public GameObject fude_geometryOb;
    public GameObject hirafude_geometryOb;
    public GameObject WposFrontOb;
    public GameObject FollowWposTargetOb;
    //パレット
    public GameObject palletOb; //パレットオブジェクト
    public GameObject palletTargetOb; //パレットの発生位置
    [HideInInspector] public GameObject lookAtTargetOb = null; //パレットが向く方向
    public GameObject PickerCamOb;
    public GameObject raycastCylinderOb;

    [Header("―――　参照するメッシュ　―――")]
    public MeshRenderer fude_geometryRender; 
    public MeshRenderer hirafude_geometryRender; 


    [Header("―――　参照するアニメーター　―――")]
    public Animator FollowWposTargetAnim;

    //参照するマテリアル
    [HideInInspector] public Material fude_geometryMat = null;
    [HideInInspector] public Material hirafude_geometryMat = null;

    [UdonSynced(UdonSyncMode.None)] public Vector4 sync_ColorParameter;
    [UdonSynced(UdonSyncMode.None)] public Vector4 sync_SizeParameter;
    [UdonSynced(UdonSyncMode.None)] public Vector4 sync_ModeParameter;

    void Start()
    {
        FudeManagerOb = this.gameObject;
        lookAtTargetOb = palletTargetOb.transform.GetChild(0).gameObject;
        fude_geometryOb.SetActive(true);
        hirafude_geometryOb.SetActive(false);

        fude_geometryMat = fude_geometryRender.material;
        hirafude_geometryMat = hirafude_geometryRender.material;
        fude_geometryOb.layer = writeLayer; //レイヤーの決定
        hirafude_geometryOb.layer = writeLayer; //レイヤーの決定
        palletOb.layer = palletLayer;

        SendCustomEventDelayedSeconds("mainSystemDisable", 3);
    }

    //準備が整ったらfalseに。以降はPickUpとDropのタイミングでEfude_pickupから呼び出す。
    public void mainSystemDisable(){ _mainSystemSc.enabled = false; }
    public void mainSystemEnable(){ _mainSystemSc.enabled = true; }

    //■■■■以下同期変数の処理■■■■
    public void ChangeVariables(){
    
        sync_ColorParameter = _pickupSc.fudeColors;
        sync_SizeParameter = _pickupSc.fudeSize;
        sync_ModeParameter = _pickupSc.fudeMode;

        Debug.Log(sync_SizeParameter);

        clampColorAlpha(); ; //ローカルでの筆の変更。
        RequestSerialization();
    }

    public override void OnDeserialization(){
        clampColorAlpha();
    }

    private void clampColorAlpha()
    {
        //ここでAlpha値を良い感じにクランプしたい。
        float clampCol = sync_ColorParameter.w;

        //1～.5を1～.3に
        if(clampCol >= .5f)
        {
            clampCol = (clampCol - .5f) * 2; //1～0に
            sync_ColorParameter.w = Mathf.Lerp(.3f, .99f, clampCol);
            SetMat();
        }
        //.5～0を.3～.001に
        else if (clampCol <= .5f)
        {
            clampCol *= 2; //1～0に
            sync_ColorParameter.w = Mathf.Lerp(0.01f, .3f, clampCol);
            SetMat();
        }
        
    }
    
    public void SetMat() {
           
        if (sync_SizeParameter.w == 0)
        {
            //丸筆
            fude_geometryOb.SetActive(true);
            hirafude_geometryOb.SetActive(false);
            fude_geometryMat.SetVector("_Color", sync_ColorParameter);
            fude_geometryMat.SetFloat("_SetSize", sync_SizeParameter.x);
            fude_geometryMat.SetFloat("_Ref", sync_ModeParameter.x);
            fude_geometryMat.SetFloat("_Illustration", sync_ModeParameter.y);
            fude_geometryOb.transform.localScale = new Vector3(sync_SizeParameter.x, 1, sync_SizeParameter.x);
        }
        else if (sync_SizeParameter.w == 1)
        {
            //刷毛
            fude_geometryOb.SetActive(false);
            hirafude_geometryOb.SetActive(true);
            hirafude_geometryMat.SetVector("_Color", sync_ColorParameter);
            hirafude_geometryMat.SetFloat("_SetSize", sync_SizeParameter.y);
            hirafude_geometryMat.SetFloat("_Ref", sync_ModeParameter.x);
            hirafude_geometryMat.SetFloat("_Illustration", sync_ModeParameter.y);
            hirafude_geometryOb.transform.localScale = new Vector3(sync_SizeParameter.y, 1, 1);
        }
    }
}
