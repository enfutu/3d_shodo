using UnityEngine;
using UnityEditor;
using VRC.Udon;
using UdonSharpEditor;
using VRC.Udon.Common.Interfaces;

class Efude_CanvasEditor : EditorWindow
{
    public GameObject Canvas;
    GameObject o_Canvas; //Canvasが切り替わったか判定用。
    GameObject cameraOb;
    Camera cameraSc;
    GameObject sizeOb;

    //初期サイズ
    float sizeOffset = 0.15f;

    string renderTextureName = null;

    bool onVcc = false;
    bool createFolder = false;
    bool canvasError = false;
    bool settingRenderTextureName = false;
    bool settingNow = false;

    [MenuItem("Window/Efude_CanvasEditor")]
    static void Open()
    {
        EditorWindow.GetWindow(typeof(Efude_CanvasEditor));
    }


    private void OnGUI()
    {
        

        //メッセージテキスト
        if (Canvas == null) { EditorGUILayout.HelpBox("変更するcanvasを指定してください", MessageType.Warning); }
        if (canvasError) { EditorGUILayout.HelpBox("指定されたオブジェクトが正しいcanvasではありません", MessageType.Warning); }
        if (createFolder) { EditorGUILayout.HelpBox("保存先のフォルダが見つからないため[Efude/RenderTexture]フォルダを作成しました", MessageType.Info); }
        if (EditorApplication.isPlaying && settingNow)
        {
            EditorApplication.isPlaying = false ;
            settingNow = false;
        }

        GUILayout.Label("");//余白
        onVcc = EditorGUILayout.Toggle("VCCで使用", onVcc);
        GUILayout.Label("");//余白
        Canvas = (GameObject)EditorGUILayout.ObjectField("Canvas", Canvas, typeof(GameObject), true);

        //Canvasが未指定の場合、以下を処理しない。Canvasが指定された場合に一度だけ、作成するRenderTextureの名前を更新する。
        //Canvasが未指定の場合、各bool値を初期化する。
        if (Canvas == null) { settingRenderTextureName = false; createFolder = false; canvasError = false; return; }
        if (Canvas != o_Canvas) { settingRenderTextureName = false; }
        o_Canvas = Canvas;

        if (!settingRenderTextureName) { renderTextureName = "rt_" + System.DateTime.Now.ToString("yyMMddHHmmss"); settingRenderTextureName = true; }

        //カメラが存在していれば、カメラを取得。取得できなければエラーメッセージを出す。
        if (Canvas.transform.Find("canvas/size/Camera") == null) { canvasError = true; return; }
        cameraOb = Canvas.transform.Find("canvas/size/Camera").gameObject;
        cameraSc = cameraOb.GetComponent<Camera>();
        //通った場合はエラーメッセージを取り消す。
        canvasError = false;

        //sizeを取得
        sizeOb = Canvas.transform.Find("canvas/size").gameObject;

        //canvasManagerに関するもろもろを取得
        GameObject CanvasManagerOb = Canvas.transform.Find("canvas/CanvasManager").gameObject; //GameObjectを取得

        //初期化
        Efude_CanvasManager CanvasManagerSc = null;
        UdonBehaviour CanvasManagerUdon = null;
        IUdonVariableTable CanvasManagerPublicVar = null;

        if (onVcc)
        {
            CanvasManagerSc = CanvasManagerOb.GetComponent<Efude_CanvasManager>(); //UdonBehaviourを経由せずそのまんま取得
        }
        else
        {
            //U#1.0からTrySetVariableValueが使えなくなった
            CanvasManagerUdon = CanvasManagerOb.GetComponent<UdonBehaviour>(); //UdonBehaviourを取得
            CanvasManagerPublicVar = CanvasManagerUdon.publicVariables; //publicVariablesを取得
            //そして→publicVariables.TrySetVariableValue("publicな変数名", アタッチしたい変数);
            //⑤でやってる
        }

        //各操作
        GUILayout.Label("―――PositionとRotationの変更―――", EditorStyles.boldLabel);
        GUILayout.Label("▼必ずcanvasを操作してください");
        if (GUILayout.Button("canvasを選択"))
        {
            Selection.activeGameObject = Canvas;
        }
        GUILayout.Label("―――Scaleの変更―――", EditorStyles.boldLabel);
        GUILayout.Label("▼必ずsizeを操作してください");
        if (GUILayout.Button("sizeを選択"))
        {
            Selection.activeGameObject = sizeOb;
        }

        GUILayout.Label(""); //余白
        GUILayout.Label("―――変更の確定―――", EditorStyles.boldLabel);
        GUILayout.Label("▼新規の場合はRenderTextureが作成されます");
        if (GUILayout.Button("確定 / 更新"))
        {
            //保存先のフォルダが存在するか確認する。ない場合は作成する。
            string FolderName = "Assets/Efude/RenderTexture";
            if (!AssetDatabase.IsValidFolder(FolderName))
            {
                AssetDatabase.CreateFolder("Assets", "Efude");
                AssetDatabase.CreateFolder("Assets/Efude", "RenderTexture");
                createFolder = true;
            }

            //①canvasのサイズを取得する
            Vector3 LS = sizeOb.transform.localScale;
            sizeOb.transform.localScale = new Vector3(LS.x, 1f, LS.z); //y軸方向に変更が加えられていたらここでリセットする。
            Vector3 canvasScale = new Vector3(LS.x * 3, 1f, LS.z * 3); //なぜか3倍しないとおかしくなるようになった(2022/03/01)

            //②初期のcanvasサイズに対する現在のcanvasサイズの比率
            float sizeX = canvasScale.x / sizeOffset;
            float sizeZ = canvasScale.z / sizeOffset;

            //③RenderTextureのサイズを決定
            sizeX *= 1024;
            sizeZ *= 1024;
            int rtX = Mathf.FloorToInt(sizeX);
            int rtZ = Mathf.FloorToInt(sizeZ);

            //④RenderTextureを生成
            RenderTexture rt;
            rt = new RenderTexture(rtX, rtZ, 16, RenderTextureFormat.ARGBHalf);
            rt.antiAliasing = 2;
            rt.depth = 24;
            rt.filterMode = 0;
            rt.anisoLevel = 0;
            rt.useMipMap = false;
            rt.autoGenerateMips = false;

            AssetDatabase.CreateAsset(rt, FolderName + "/" + renderTextureName + ".renderTexture");

            //⑤camera等にアタッチ
            cameraSc.targetTexture = rt;

            if (onVcc)
            {
                CanvasManagerSc.CanvasTex = rt; //直接入れられるようになった
            }
            else
            {
                CanvasManagerPublicVar.TrySetVariableValue("CanvasTex", rt); //U#1.0から使えなくなった
            }

            //⑥cameraのsizeを設定
            cameraSc.orthographicSize = (canvasScale.z / sizeOffset) * 0.25f;

            //⑦Sceneを再生せずとも良いようにしたい。
            //rt.Release();
            //上手く行かない。
        }
        EditorGUILayout.LabelField("RenderTextureの名前 : ", renderTextureName);


        GUILayout.Label(""); //余白
        GUILayout.Label(""); //余白
        GUILayout.Label("―――終了処理―――", EditorStyles.boldLabel);
        GUILayout.Label("▼一度もsceneを再生しないまま生成されたRenderTextureを操作するとUnityがクラッシュします!");
        if (GUILayout.Button("sceneを再生→終了"))
        {
                //⑦Sceneを再生する。一度も再生しないままRenderTextureを削除するなどの操作を行うとUnityがクラッシュするため。
                EditorApplication.isPlaying = true;
                settingNow = true;
        }          
    }
}
