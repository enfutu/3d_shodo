
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Efude_CanvasManager : UdonSharpBehaviour
{
    [Header("―――　キャンバスの初期状態をONにする場合はチェック　―――")]
    public bool InitialStateIs_ON;
    [Header("―――　使用しないキャンバスが自動で電源をOFFにする場合はチェック　―――")]
    public bool TurnOffAutomatically;

    //Cameraの設定は拡張メニューで別途行う
    [Header("―――　使用するレンダーテクスチャ　―――")]
    public RenderTexture CanvasTex;
    
    [Header("■■■■■■■■■■■■■■■■■■")]
    [Header("★キャンバスの参照元を全てここで設定")]
    [Header("―――　筆システムで使用するレイヤー番号　―――")]
    [Header("〇キャンバスに書き込まれるレイヤー")]
    public int writeLayer;
    [Header("〇筆が衝突するレイヤー")]
    public int collisionLayer;
    [Header("―――　参照するスクリプト　―――")]
    public Efude_ResetSwitch _resetSwitchSc;
    public Efude_OnOffSwitch _onoffSwitchSc;
    public Efude_SwitchCollider _switchColliderSc;
    [Header("―――　参照するメッシュ　―――")]
    public MeshRenderer FrontCanvasMesh;
    public MeshRenderer BackCanvasMesh;
    public MeshRenderer PhotoFrameMesh;
    public MeshRenderer GridMesh;
    public MeshRenderer Filter0Mesh;
    public MeshRenderer Filter1Mesh;
    public MeshRenderer ErrorMesh; 
    [HideInInspector] public Material FrontCanvasMat;
    [HideInInspector] public Material BackCanvasMat;
    [HideInInspector] public Material PhotoFrameMat;
    [HideInInspector] public Material GridMat;
    [HideInInspector] public Material Filter0Mat;
    [HideInInspector] public Material Filter1Mat;
    [HideInInspector] public Material ErrorMat;
    [Header("―――　参照するオブジェクト　―――")]
    public GameObject ColliderOb;
    public GameObject BackCanvasOb;
    public GameObject Filter0Ob;
    public GameObject Filter1Ob;
    public GameObject CameraOb;
    public GameObject PhotoFrameOb;
    public GameObject GridOb;
    public GameObject ErrorOb;
    public GameObject SwitchColliderOb;
    [Header("―――　参照するカメラ　―――")]
    public Camera CameraCam;

    public bool boot = false;

    void Start()
    {
        getMat();
        SwitchColliderState();
        ColliderOb.layer = collisionLayer;
        BackCanvasOb.layer = collisionLayer;
        Filter0Ob.layer = collisionLayer;
        Filter1Ob.layer = collisionLayer;
        CameraCam.cullingMask = 1 << writeLayer;
        CameraCam.targetTexture = CanvasTex;
    }

    public void SystemOn()
    {   
        CameraOb.SetActive(true);
        BackCanvasOb.SetActive(true);
        Filter0Ob.SetActive(true);
        Filter1Ob.SetActive(true);
        SwitchColliderOb.SetActive(true);
        FrontCanvasMat.SetInt("_Off", 0);
        boot = true;
    }

    public void SystemOff()
    {
        CameraOb.SetActive(false);
        BackCanvasOb.SetActive(false);
        Filter0Ob.SetActive(false);
        Filter1Ob.SetActive(false);
        PhotoFrameOb.SetActive(false);
        GridOb.SetActive(false);
        ErrorOb.SetActive(false);
        SwitchColliderOb.SetActive(false);
        FrontCanvasMat.SetInt("_Off", 1);
        boot = false;
    }


    private void getMat()
    {
        FrontCanvasMat = FrontCanvasMesh.material;
        BackCanvasMat = BackCanvasMesh.material;
        PhotoFrameMat = PhotoFrameMesh.material;
        Filter0Mat = Filter0Mesh.material;
        Filter1Mat = Filter1Mesh.material;
        GridMat = GridMesh.material;
        ErrorMat = ErrorMesh.material;
        SendCustomEventDelayedSeconds("setTex", 3);
    }

    public void setTex()
    {
        FrontCanvasMat.SetTexture("_MainTex", CanvasTex);
        BackCanvasMat.SetTexture("_MainTex", CanvasTex);
        PhotoFrameMat.SetTexture("_MainTex", CanvasTex);
        GridMat.SetTexture("_MainTex", CanvasTex);
        ErrorMat.SetTexture("_MainTex", CanvasTex);
        Filter0Mat.SetInt("_TexSize", CanvasTex.width);
        Filter1Mat.SetInt("_TexSize", CanvasTex.width);

        _resetSwitchSc.canvasReset(); //キャンバスの初期化

        if (Networking.LocalPlayer.playerId == 1)
        {
            if (InitialStateIs_ON)
            {
                _onoffSwitchSc.ON();
            }
            else
            {
                SystemOff();
            }
        }
        else
        {
            SystemOff();
        }
    }

    public void SwitchColliderState()
    {
        if (TurnOffAutomatically) { _switchColliderSc.countEnable = true; }
    }
}
