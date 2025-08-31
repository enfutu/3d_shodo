
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace enfutu.UdonScript
{
    //[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class sortSystem : UdonSharpBehaviour
    {
        [SerializeField] private Material _generateMat;
        [SerializeField] private RenderTexture _generateRT;

        [SerializeField] private Material _activeMat;
        [SerializeField] private RenderTexture _activeRT;

        [SerializeField] private Material _sortMat;
        [SerializeField] private RenderTexture _sortRT;


        void Start()
        {

        }

        int step = 0;
        bool blink = false;
        void Update()
        {
            /*
            if(step == 0) { genrateTestMap(); step++; }
            if(step == 1) { createActiveMap(); step++; }
            if(step == 2) { sortMap(); step = 0; }
            */

            if (!blink) { createActiveMap(); }
            else { sortMap(); }
            blink = !blink;

        }

        private void genrateTestMap()
        {
            VRCGraphics.Blit(null, _generateRT, _generateMat);
        }

        private void createActiveMap()
        {
            VRCGraphics.Blit(null, _activeRT, _activeMat);
        }

        private void sortMap()
        {
            VRCGraphics.Blit(null, _sortRT, _sortMat);
        }


    }
}
