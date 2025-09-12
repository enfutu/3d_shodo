
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
            _end = EndBase.position;

            rayDistance = Vector3.Distance(_end, _start);

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
                if(_hitCount < 10) { _hitCount++; }
                Vector2 uv = hit.textureCoord;
                BlitSc.PositionsArray[ID] = uv;
                setThreePoints(hit.point, true);
            }
            else
            {
                if(0 < _hitCount) { _hitCount--; }
                Vector3 _temp = _start + vec * rayDistance * .5f;
                setThreePoints(_temp, false);
            }
        }

        private int _hitCount = 0;
        public bool IsFreeze = false;
        private Vector3 _freezeVec;
        //前回の座標を保持
        private Vector3 _start;
        private Vector3 _hit;
        private Vector3 _end;
        private void setThreePoints(Vector3 hitPos, bool isHit)
        {
            if (isHit)
            {
                _start = this.transform.position;

                if (IsFreeze)
                {
                    _hit = _start + _freezeVec * rayDistance * .5f;
                }
                else
                {
                    _hit = Vector3.Lerp(_hit, hitPos, .05f);
                    _freezeVec = (_hit - _start).normalized;            //押し付けていない時に_freezeVecを更新する。
                }
 
                //_end = Vector3.Lerp(_end, EndBase.position, .001f);             //戻ろうとする
                //_endの真の座標を決める
                Vector3 vec = (_hit - _start).normalized;
                Vector3 endTarget = _start + vec * rayDistance;
                _end = Vector3.Lerp(_end, endTarget, .05f);
            }
            else
            {
                _start = this.transform.position;

                //_hitと_endは戻ろうとする
                Vector3 vec = (EndBase.position - this.transform.position).normalized;
                _hit = Vector3.Lerp(_hit, _start + vec * rayDistance * .5f, .1f);
                _end = Vector3.Lerp(_end, _start + vec * rayDistance, .05f);              
            }

            //筆が伸びないように長さを整える。
            Vector3 vec0 = (_hit - _start).normalized;
            _start = _start;
            _hit = _start + vec0 * rayDistance * .5f;
             Vector3 vec1 = (_end - _hit).normalized;
            _end = _hit + vec1 * rayDistance * .5f;

            _mat.SetVector("_Start", _start);
            _mat.SetVector("_Hit", _hit);
            _mat.SetVector("_End", _end);
            _mat.SetInt("_HitCount", _hitCount);

        }
    }
}
