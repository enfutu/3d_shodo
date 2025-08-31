
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace enfutu.UdonScript
{
    //[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class RTUploader : UdonSharpBehaviour
    {
        public Material UpdateMat;
        [SerializeField] private RenderTexture update0;
        [SerializeField] private RenderTexture update1;

        void Start()
        {

        }

        bool blink = false;
        void Update()
        {
            //UpdateTexture
            if (blink)
            {
                Blit0();
            }
            else
            {
                Blit1();
            }
        }

        public void Blit0()
        {
            blink = !blink;
            UpdateMat.SetTexture("_Src", update0);
            VRCGraphics.Blit(null, update1, UpdateMat);
        }

        public void Blit1()
        {
            blink = !blink;
            UpdateMat.SetTexture("_Src", update1);
            VRCGraphics.Blit(null, update0, UpdateMat);
        }
    }
}
