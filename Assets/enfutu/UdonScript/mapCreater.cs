
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace enfutu.UdonScript
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class mapCreater : UdonSharpBehaviour
    {
        [SerializeField] MeshRenderer mesh;

        [SerializeField] Material _testMat; //view wpos
        [SerializeField] Material _liveMat;
        [SerializeField] Camera cam;

        void Start()
        {
            mesh.material = _testMat;
        }

        int count = 0;
        void Update()
        {
            count++;
            if(10 < count)
            {
                cam.enabled = false;
                mesh.material = _liveMat;

                this.enabled = false;
            }
        }
    }
}
