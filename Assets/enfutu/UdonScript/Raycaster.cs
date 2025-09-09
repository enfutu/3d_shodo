
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace enfutu.UdonScript
{
    //[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class Raycaster : UdonSharpBehaviour
    {
        //base
        public Transform StartBase;
        public Transform EndBase;
        public MeshRenderer Fude;

        private Material _mat;

        //initialID
        [HideInInspector] public int ID;
        [HideInInspector] public BlitSystem BlitSc;

        void Start() { }

        bool _boot = false;
        public void Boot()
        {
            _start = StartBase.position;

            rayDistance = Vector3.Distance(EndBase.position, _start);

            Vector3 vec = (_end - _start).normalized;
            float halfRayDistance = rayDistance * .5f;

            _hit = _start + vec * halfRayDistance;
            _end = _hit + vec * halfRayDistance;

            EndBase.position = _end;

            _mat = Fude.material;

            _boot = true;
        }

        void Update()
        {
            if (!_boot) return;
            raychan();
        }

        //raycast
        int layerMask = 1 << 25;
        private float rayDistance = 0f;
        private void raychan()
        {
            Vector3 vec = (_end - _start).normalized;
            Ray ray = new Ray(_start, vec);
            Debug.DrawRay(_start, vec * rayDistance, Color.red);

            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, rayDistance, layerMask))
            {
                Vector2 uv = hit.textureCoord;
                BlitSc.PositionsArray[ID] = uv;
                setThreePoints(hit.point, true);
            }
            else
            {
                Vector3 _temp = _start + vec * rayDistance * .5f;
                setThreePoints(_temp, false);
            }
        }

        //前回の座標を保持
        private Vector3 _start;
        private Vector3 _hit;
        private Vector3 _end;
        private void setThreePoints(Vector3 hitPos, bool isHit)
        {
            _start = this.transform.position;
            _hit = Vector3.Lerp(_hit, hitPos, .1f);

            float offset = 0f; 
            if (isHit) { offset = .001f; }
            else { offset = .1f; }

            _end = Vector3.Lerp(_end, EndBase.position, offset);     //戻ろうとする

            //_endの真の座標を決める
            Vector3 vec = (_hit - _start).normalized;
            Vector3 endTarget = _start + vec * rayDistance;
            _end = Vector3.Lerp(_end, endTarget, .1f);

            _mat.SetVector("_Start", _start);
            _mat.SetVector("_Hit", _hit);
            _mat.SetVector("_End", _end);

        }
    }
}
