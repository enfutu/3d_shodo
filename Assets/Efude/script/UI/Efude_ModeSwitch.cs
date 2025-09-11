
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Efude_ModeSwitch : UdonSharpBehaviour
{
    [SerializeField] Efude_CanvasManager _CanvasManagerSc;

    int ModeNum = 1;

    public override void Interact()
    {
        setOwner();

        _CanvasManagerSc.PhotoFrameOb.SetActive(false);
        _CanvasManagerSc.GridOb.SetActive(false);

        if (!_CanvasManagerSc.boot) return; //電源OFF時には動かさない。
          
        if(ModeNum == 0)
        {
            ModeNum++;
        }
        else if (ModeNum == 1)
        {
            //GridMode
            _CanvasManagerSc.GridOb.SetActive(true);
            ModeNum++;
        }
        else if (ModeNum == 2)
        {
            //PhotoFrame
            _CanvasManagerSc.PhotoFrameOb.SetActive(true);
            ModeNum = 0;
        }
        else
        {
            ModeNum = 0;
        }
    }

    private void setOwner()
    {
        if (!Networking.IsOwner(Networking.LocalPlayer, this.gameObject))
        {
            Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
        }
    }

}
