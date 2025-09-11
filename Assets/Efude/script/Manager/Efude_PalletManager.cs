
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Efude_PalletManager : UdonSharpBehaviour
{
    [Header("―――　参照するスクリプト　―――")]
    public Efude_pickup _pickupSc;
    //public Efude_mainSystem _mainSystemSc;
    //public Efude_pickerCamera _pickerCameraSc;

    [Header("―――　参照するオブジェクト　―――")]
    public GameObject pallet_TexureOb;
    public GameObject pallet_ColliderOb;

    [Header("―――　参照するマテリアル　―――")]
    public Material palletMat;

    /*
    public void SetMat()
    {
        palletMat.SetVector("_ColorRGBA", _pickupSc.PaletColors);
    }*/

}
