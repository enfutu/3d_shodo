
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace enfutu.UdonScript
{
    //[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class RTResetSwitch : UdonSharpBehaviour
    {
        public Material ResetRTMat;
        [SerializeField] private RenderTexture[] _rt;


        void Start()
        {

        }

        public override void Interact() 
        {
            if (!Networking.LocalPlayer.IsOwner(this.gameObject))
            {
                Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
            }

            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(Reset));
        }

        public void Reset()
        {
            for (int i = 0; i < _rt.Length; i++)
            {
                VRCGraphics.Blit(null, _rt[i], ResetRTMat);
            }
        }
    }
}

