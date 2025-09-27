
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace enfutu.UdonScript
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class Raycaster : UdonSharpBehaviour
    {
        //base
        //public Transform StartBase;
        public Transform EndBase;
        [HideInInspector] public Transform HitBase;     //Scalerから渡す
        public MeshRenderer Fude;

        private Material _mat;

        //initialID
        [HideInInspector] public int ID;
        [HideInInspector] public BlitSystem BlitSc;

        //void Start() { }

        bool _boot = false;
        //[HideInInspector] public float InnerRange;
        //[HideInInspector] public Vector3 SystemCenterPosition;
        public void Boot()
        {
            _start = this.transform.position;
            _end = EndBase.position;

            rayDistance = Vector3.Distance(_end, _start);

            Vector3 vec = (_end - _start).normalized;
            float halfRayDistance = rayDistance * .5f;

            _hit = _start + vec * halfRayDistance;
            _end = _hit + vec * halfRayDistance;

            EndBase.position = _end;

            _mat = Fude.material;

            thresholdDist = (_hit - _start).sqrMagnitude;
            _boot = true;
        }

        float thresholdDist = 0f;
        void Update()
        {
            if (!_boot) return;

            _start = this.transform.position;

            raychan();
        }

        //raycast
        int layerMask = 1 << 25;
        private float rayDistance = 0f;
        private void raychan()
        {
            //Vector3 vec = (_end - _start).normalized;
            Vector3 vec = (_hit - _start).normalized;
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

        public float OpenRadius = 0;
        public Vector3 OpenVec;
        public int SumiCount = 0;
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
                if (_hitCount < 100) { _hitCount += 10; }

                if (IsFreeze)
                {
                    _hit = _start + _freezeVec * rayDistance * .5f;
                }
                else
                {
                    _hit = Vector3.Lerp(_hit, hitPos, .05f);
                }
            }
            else
            {
                //_hitと_endは戻ろうとする
                //Vector3 temp_HitPos = HitBase.position + (OpenVec * OpenRadius);
                //Vector3 vec0 = (temp_HitPos - _start).normalized; 
                //_hit = Vector3.Lerp(_hit, _start + vec0 * rayDistance * .5f, .05f);
                //Vector3 vec1 = (EndBase.position - this.transform.position).normalized;
                //_end = Vector3.Lerp(_end, _start + vec1 * rayDistance, .01f);

                float reposOffset_hit = 1f - Mathf.Clamp01(.01f * _hitCount);
                float reposOffset_end = 1f - Mathf.Clamp01(.0314f * _hitCount);

                /*
                float reposOffset_hit = 1f;
                float reposOffset_end = 1f;
                if (0 < _hitCount)
                {
                    reposOffset_hit = .01f;
                    reposOffset_end = .005f;
                }
                */

                _hit = Vector3.Lerp(_hit, Vector3.Lerp(_start, EndBase.position, .5f), reposOffset_hit);
                _end = Vector3.Lerp(_end, EndBase.position, reposOffset_end);
            }
            
            //押し付けていない時、_freezeVecを更新する。
            if (!IsFreeze) 
            {
                if (0 < _hitCount) { _hitCount--; }
                _freezeVec = (_hit - _start).normalized;
            }

            //筆が伸びないように長さを整える。
            Vector3 vecToHit = (_hit - _start).normalized;
            _hit = _start + vecToHit * rayDistance * .5f;
            Vector3 vecToEnd = (_end - _hit).normalized;
            _end = _hit + vecToEnd * rayDistance * .5f;

            _mat.SetVector("_Start", _start);
            _mat.SetVector("_Hit", _hit);
            _mat.SetVector("_End", _end);
            _mat.SetVector("_EndBase", EndBase.position);
            //_mat.SetVector("_Center", SystemCenterPosition);
            //_mat.SetFloat("_InnerRange", InnerRange);
        }
    }
}
