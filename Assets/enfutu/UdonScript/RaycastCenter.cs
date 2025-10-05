
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
        
        //Debug
        public Transform Viewer;
        
        private float range = 0f;
        void Start() 
        {
            range = RayLength * 2f;
            Viewer.localScale = Vector3.one * range;

            //Vector3 halfFrontVec = this.transform.forward * range * .5f;
            //Viewer.position = this.transform.position + halfFrontVec;

        }

        public bool IsFreeze = false;
        public float FreezeCount = 0;
        private Vector3 previousForward;
        private Vector3 previousVec;
        private Vector3 _start;
        private Vector3 _end;
        void Update()
        {
            _start = this.transform.position;

            Vector3 forward = this.transform.forward;
            Vector3 targetPos = _start + forward * RayLength;
            Vector3 lerpPos = Vector3.Lerp(_end, targetPos, .05f);

            Quaternion rot = Quaternion.FromToRotation(previousForward, forward);
            Vector3 previousEnd = _start + rot * previousVec * RayLength;

            //抑えに応じ更新度合を変更
            float updateOffset = FreezeCount;
            lerpPos = Vector3.Lerp(lerpPos, previousEnd, updateOffset);

            //角度を制限し長さを整えて保存
            Vector3 currentVec = (lerpPos - _start).normalized;
            
            //正面ベクトルを基準に、最大角度を制限
            float angle_base = Vector3.Angle(forward, currentVec);
            float maxAngle_base = 80f;
            if (maxAngle_base < angle_base)
            {
                currentVec = Vector3.RotateTowards(forward, currentVec, Mathf.Deg2Rad * maxAngle_base, 0f);
            }

            _end = _start + currentVec * RayLength;
            
            EndBase.position = _end;

            previousForward = forward;
            previousVec = currentVec;
        }
    }
}
