
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace enfutu.UdonScript
{
    //[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class Raycaster : UdonSharpBehaviour
    {
        public Material MarkUpdateMat;
        [SerializeField] RenderTexture mark0;
        [SerializeField] RenderTexture mark1;

        void Start()
        {

        }

        void Update()
        {
            raychan();
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

        //raycast
        int layerMask = 1 << 25;
        private float rayDistance = 1f;
        bool blink = false;
        private void raychan()
        {
            Ray ray = new Ray(this.transform.position, this.transform.forward);
            Debug.DrawRay(this.transform.position, this.transform.forward * rayDistance, Color.red);

            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, rayDistance, layerMask))
            {
                Vector2 uv = hit.textureCoord;

                MarkUpdateMat.SetVector("_Target", uv);
            }
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

    }
}
