using UdonSharp;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using VRC.SDKBase;
using VRC.Udon;

public class Efude_pickup : UdonSharpBehaviour
{
    [SerializeField] Efude_FudeManager _fudeManagerSc; //筆の参照元
    [SerializeField] Efude_PalletManager _PalletManagerSc; //パレットの参照元

    [HideInInspector] public bool boot = false; //fude_mainのUpdateを走らせるかどうかに使用する
    
    //PickUp
    private VRC_Pickup pickup;

    Ray ray;
    public RaycastHit hit; //PalletManagerでも使う
    LayerMask layerMask;
       
    Vector2 UV;
    public Vector4 fudeColors; //syncへ(R,G,B,A)
    public Vector4 PaletColors; //PalletManagerへ渡しパレットへ表示する(R,G,B,A)。Aのみ筆にも転送する
    public Vector4 fudeMode; //X:Wetモード　Y:粘りモード　Z:W:予備
    public Vector4 fudeSize; //syncへ(SizeA,SizeB,0,mode)

    bool UsePallet = false;
    bool UsePicker = false;
    bool ModeChanged = false;
    int UpdateCounter = 0;

    //public float test;

    /// <summary>
    /// ■メモ■
    /// カメラから色を取る→　Vector4 textureCol = _fudeManagerSc._pickerCameraSc.pickerBuffer.GetPixel(0, 0);
    /// 
    /// 同期を走らせる(パレットを出して且つrayHit)→_fudeManagerSc.ChangeVariables();
    /// 
    /// 色について、パレットにはRGBAを渡すけど、筆にはAだけを渡す。RGBはカメラからのみ取得。
    /// カラーピッカーは各筆につける。
    /// </summary>

    void Start(){
        //■マネージャーからレイヤー参照
        layerMask = 1 << _fudeManagerSc.palletLayer;

        fudeColors = new Vector4(0, 0, 0, 1);
        PaletColors = new Vector4(0, 0, 0, 1);
        fudeMode = new Vector4(101, 0, 0, 0);
        fudeSize = new Vector4(1, 1, 0, 0);

        _fudeManagerSc.raycastCylinderOb.SetActive(false);
        DisablePallet();
        DisableCamera();

        //デバッグ
        //SendCustomEventDelayedSeconds("ON", 5);
        //UsePallet = true;
        //_fudeManagerSc.raycastCylinderOb.SetActive(true);
    }

    void FixedUpdate(){

        if (!UsePallet) return;
        PalletHitRay();

        if (!UsePicker) return;
        //5Fに一度カメラを起動して色の取得をする。
        UpdateCounter++;
        DisableCamera();
        if (UpdateCounter <= 5) return;
        UpdateCounter = 0;

        EnableCamera();
    }

    public override void OnPlayerJoined(VRCPlayerApi player) {
        if (boot)
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "ON");
        }
        else
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "OFF");
        }

        //オーナーであればパラメーターの同期を走らせる
        if (Networking.IsOwner(Networking.LocalPlayer, this.gameObject))
        {
            SetMat();
        }
    }


    public override void OnPickup()
    {
        ChangeOwner();
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "ON");
    }

    private void pickUpHandGetter()
    {
        bool leftHand = pickup.currentHand == VRC_Pickup.PickupHand.Left; //trueならleft
        if (leftHand)
        {
            return;
        }
    }

    public void ChangeOwner()
    {
        Networking.SetOwner(Networking.LocalPlayer, _fudeManagerSc.fude_mainSystemOb);
        Networking.SetOwner(Networking.LocalPlayer, _fudeManagerSc.FudeManagerOb);
    }

    public override void OnDrop() 
    {
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "OFF");
        PalletModeOFF(); //Useしたままdropした時の予防
    }

    public void ON()
    {
        boot = true;
        _fudeManagerSc.mainSystemEnable();
    }

    public void OFF()
    {
        boot = false;
        _fudeManagerSc.mainSystemDisable();
    }

    public override void OnPickupUseDown(){
        PalletModeON();
    }

    public override void OnPickupUseUp(){
        PalletModeOFF();
    }

    private void PalletModeON()
    {
        UsePallet = true;
        UsePicker = true;
        _fudeManagerSc.raycastCylinderOb.SetActive(true);
        EnablePallet();
    }

    private void PalletModeOFF()
    {
        UsePallet = false;
        UsePicker = false;
        _fudeManagerSc.raycastCylinderOb.SetActive(false);
        DisablePallet();
    }

    private void EnablePallet(){
        _fudeManagerSc.palletOb.transform.position = _fudeManagerSc.palletTargetOb.transform.position;
        _fudeManagerSc.palletOb.transform.LookAt(_fudeManagerSc.lookAtTargetOb.transform.position);
        _fudeManagerSc.palletOb.SetActive(true);
    }

    private void DisablePallet(){
        _fudeManagerSc.palletOb.SetActive(false);
        DisableCamera(); //カメラを起動したままパレットを消すとカメラが残ってしまう
    }


    private void PalletHitRay(){

        //カメラがUIのカラーを撮らないよう、カメラよりも早い段階でレイキャストの判定を取る必要があるため少し長めのレイを飛ばしている
        ray = new Ray(transform.position, transform.up * .122f);
        //Debug.DrawRay(transform.position, transform.up * .122f, Color.green);

        if (Physics.Raycast(ray, out hit, .122f, layerMask))  
        {
            UV = hit.textureCoord;
            

            //上段パレットの処理
            if (.55f <= UV.y && UV.y <= .95f)
            {
                //■■■UI操作中はカラーピッカーを使用しない■■■
                UsePicker = false;
                DisableCamera();
                UpdateCounter = 0;
                //■■■■■■

                UV.y = (UV.y - .55f) * 2;//.55～.95を0～.8に。
                UV.y /= .8f; //0～.8を0～1へ
                //Debug.Log(UV.y);

                if (.05f <= UV.x && UV.x <= .15f)
                {
                    PaletColors.x = UV.y;
                }
                else if (.2f <= UV.x && UV.x <= .3f)
                {
                    PaletColors.y = UV.y;
                }
                else if (.35f <= UV.x && UV.x <= .45f)
                {
                    PaletColors.z = UV.y;
                }
                else if (.5f <= UV.x && UV.x <= .6f)
                {
                    PaletColors.w = UV.y;
                }
                else if (.65f <= UV.x && UV.x <= .7f)
                {
                    if (ModeChanged) return; //Modeを一度変えたなら、筆が離れるまで再度変更させない。

                    if(.9f <= hit.textureCoord.y && hit.textureCoord.y <= .95f) //WET
                    {
                        ModeChanged = true;
                        if(fudeMode.x == 100) { fudeMode.x = 101; }
                        else { fudeMode.x = 100; }
                    }
                    else if(.8f <= hit.textureCoord.y && hit.textureCoord.y <= .85f) //Illustration
                    {
                        ModeChanged = true;
                        if (fudeMode.y == 0) { fudeMode.y = 1; }
                        else { fudeMode.y = 0; }
                    }
                }                       
                else if (.75f <= UV.x && UV.x < .85f)
                {
                    fudeSize.x = 1 - UV.y;
                    fudeSize.w = 0;
                }
                else if (.85f < UV.x && UV.x <= .95f)
                {
                    fudeSize.y = 1 - UV.y;
                    fudeSize.w = 1;
                }
            }
            else if (.45f <= UV.y && UV.y <= 1f)
            {
                UsePicker = false; //上段パレットの意図しない箇所でのカラーピックを防ぐ目的
            }
            else
            {
                //その他のパレットではカラーピッカーを使用する。
                UsePicker = true;
            }

            SetMat();
        }
        else //パレットから筆が離れた
        {
            //パレットの外ではカラーピッカーを使用する
            UsePicker = true;
            ModeChanged = false;
        }
    }
    
    private void EnableCamera()
    {
        _fudeManagerSc.PickerCamOb.SetActive(true);
        getFudeColors();
    }

    private void DisableCamera()
    {
        _fudeManagerSc.PickerCamOb.SetActive(false);
    }

    private void getFudeColors()
    {
        Vector4 picColor = _fudeManagerSc._pickerCameraSc.pickerBuffer.GetPixel(0, 0);
        fudeColors = picColor;
        SetMat();
    }
    
    private void SetMat(){
        //アルファは即時更新
        fudeColors.w = PaletColors.w;

        _fudeManagerSc.ChangeVariables(); //同期
        //ローカルでの筆の変更は同期処理の後に走らせた方が良いので、
        //_fudeManagerSc.ChangeVariables内で動かすことに。

        _PalletManagerSc.palletMat.SetVector("_ColorRGBA", PaletColors); //パレットの表示変更
        _PalletManagerSc.palletMat.SetVector("_Mode", fudeMode); //パレットの表示変更
    }
}
