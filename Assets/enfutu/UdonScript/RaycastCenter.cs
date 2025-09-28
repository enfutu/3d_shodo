
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace enfutu.UdonScript
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class RaycastCenter : UdonSharpBehaviour
    {
        //Raycast不要になった

        public float RayLength;
        public Transform EndBase;

        //void Start() { }

        private Vector3 _start;
        private Vector3 _end;
        void Update()
        {
            Vector3 diff = (this.transform.position - _start).normalized;
            _start = this.transform.position;

            Vector3 forward = this.transform.forward;
            Vector3 targetEndPos = _start + (forward + diff).normalized * RayLength;
            _end = Vector3.Lerp(_end, targetEndPos, .0314f);
            EndBase.position = _end;
        }
    }
}
