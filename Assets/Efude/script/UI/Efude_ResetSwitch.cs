
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Efude_ResetSwitch : UdonSharpBehaviour
{
    [SerializeField] Efude_CanvasManager _CanvasManagerSc;

    //★起動時にローカルでリセットをかけるが、これはManager側からReset()を叩いてもらう。

    public override void Interact()
    {
        setOwner();

        //インタラクト時、ワールド全体でリセット
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "canvasReset");
    }

    public void canvasReset()
    {
        _CanvasManagerSc.BackCanvasMat.SetInt("_Reset", 1);
        SendCustomEventDelayedFrames("Done", 10);
    }

    public void Done()
    {
        _CanvasManagerSc.BackCanvasMat.SetInt("_Reset", 0);
    }

    private void setOwner()
    {
        if (!Networking.IsOwner(Networking.LocalPlayer, this.gameObject))
        {
            Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
        }
    }
}
