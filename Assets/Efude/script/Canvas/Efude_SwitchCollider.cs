
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Efude_SwitchCollider : UdonSharpBehaviour
{
    [SerializeField] Efude_OnOffSwitch _OnOffSwitchSc;
    [SerializeField] Efude_CanvasManager  _CanvasManagerSc;
    Rigidbody rb;
    [HideInInspector] public bool countEnable = false; //カウントをする場合はTrue。これはCanvasManagerから設定する
    bool countStart = false;
    bool touch = false;
    int closeCount = 0;

    //意図せず筆に触れ電源がONになったら良くないので、筆による電源ONは削除
    //筆先のmassは123です。
    //スイッチコライダー自体のmassは144です。
    void OnTriggerEnter(Collider other) 
    {
        rb = other.attachedRigidbody;
        if (rb == null) return;

        //渇きの処理と電源自動オフの処理
        if (rb.mass >= 122 && rb.mass <= 124)
        {
            DryOff();
            touch = true;
            closeCount = 0; //触れたときに初期化
        }

        //キャンバスやパレットが触れたとき書けないようにする処理
        if(rb.mass >= 143 && rb.mass <= 145)
        {
            canvasError();
        }
    }

    void OnTriggerExit(Collider other) 
    {
        rb = other.attachedRigidbody;
        if (rb == null) return;

        //渇きの処理と電源自動オフの処理
        if (rb.mass >= 122 && rb.mass <= 124) 
        {
            DryOn();
            touch = false;
            closeCount = 0; //離れたときも初期化
        }

        //キャンバスやパレットが触れたとき書けないようにする処理
        if (rb.mass >= 143 && rb.mass <= 145)
        {
            canvasErrorClear();
        }

        if (!countEnable) return;
        if (countStart) return; //すでにカウントがはじまっていれば
        countStart = true;
        Delayed_5();
    }

    //5秒ごとのチェック
    public void Delayed_5() 
    {
        if (closeCount >= 10)
        {
            //触れ続けて30秒経っている場合はカウントを戻す。
            if (touch) { closeCount = 0; return;}

            AutoClose();
            return;
        }

        closeCount++;
        SendCustomEventDelayedSeconds("Delayed_5", 5);
    }

    private void DryOn()
    {
        _CanvasManagerSc.BackCanvasMat.SetInt("_Dry", 1);
    }

    private void DryOff()
    {
       _CanvasManagerSc.BackCanvasMat.SetInt("_Dry", 0);
    }

    private void AutoClose()
    {
        _OnOffSwitchSc.OFF();
        closeCount = 0;
        countStart = false;
    }

    private void canvasError()
    {
        _CanvasManagerSc.CameraOb.SetActive(false);
        _CanvasManagerSc.ErrorOb.SetActive(true);
    }

    private void canvasErrorClear()
    {
        _CanvasManagerSc.CameraOb.SetActive(true);
        _CanvasManagerSc.ErrorOb.SetActive(false);
    }
}
