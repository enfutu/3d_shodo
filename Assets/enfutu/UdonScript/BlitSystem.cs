
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace enfutu.UdonScript
{
    //[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class BlitSystem : UdonSharpBehaviour
    {
        public Material MarkUpdateMat;
        [SerializeField] RenderTexture mark0;
        [SerializeField] RenderTexture mark1;
        public Material FrashFiberMat;
        [SerializeField] RenderTexture frashMap;

        public int count;
        public Vector4[] PositionsArray;

        void Start() { }

        bool boot = false;
        public void Boot()
        {
            PositionsArray = new Vector4[count];
            MarkUpdateMat.SetInt("_MaxLength", count);
            FrashFiberMat.SetInt("_MaxLength", count);
            boot = true;
        }

        bool blink = false;
        void Update()
        {
            if (!boot) return;

            MarkUpdateMat.SetVectorArray("_Positions", PositionsArray);
            FrashFiberMat.SetVectorArray("_Positions", PositionsArray);

            if (blink)
            {
                Blit0();
            }
            else
            {
                Blit1();
            }

            Blit_Frash();
        }

        //Blit
        public void Blit0()
        {
            blink = !blink;
            MarkUpdateMat.SetTexture("_Src", mark0);
            VRCGraphics.Blit(null, mark1, MarkUpdateMat);
        }

        public void Blit1()
        {
            blink = !blink;
            MarkUpdateMat.SetTexture("_Src", mark1);
            VRCGraphics.Blit(null, mark0, MarkUpdateMat);
        }

        public void Blit_Frash()
        {
            VRCGraphics.Blit(null, frashMap, FrashFiberMat);
        }
    }
}
