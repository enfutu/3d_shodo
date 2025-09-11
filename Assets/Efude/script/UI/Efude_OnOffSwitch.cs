
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Efude_OnOffSwitch : UdonSharpBehaviour
{
    [SerializeField] Efude_CanvasManager _CanvasManagerSc;

    bool toggle = false;

    public override void Interact()
    {
        setOwner();

        if (!toggle)
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "ON");
        }
        else
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "OFF");
        }
    }

    public void ON()
    {
        _CanvasManagerSc.SystemOn();
        toggle = true;
    }

    public void OFF()
    {
        _CanvasManagerSc.SystemOff();
        toggle = false;
    }

    private void setOwner()
    {
        if (!Networking.IsOwner(Networking.LocalPlayer, this.gameObject))
        {
            Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
        }
    }

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        //オーナーであればパラメーターの同期を走らせる
        if (Networking.IsOwner(Networking.LocalPlayer, this.gameObject))
        {
            SendCustomEventDelayedSeconds("sendStatusForSwitch", 5); //あちこちのStartが確実に終わるのを待つ
        }
    }

    public void sendStatusForSwitch()
    {
        //trueであればOnの処理を、falseであればOffの処理を。Interactの逆になる。
        if (toggle)
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "ON");
        }
        else
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "OFF");
        }
    }
}
