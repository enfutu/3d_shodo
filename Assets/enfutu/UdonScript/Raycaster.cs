
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
        public Transform EndBase;
        [HideInInspector] public Transform HitBase;     //Scalerから渡す
        [HideInInspector] public Transform EndCenterBase;     //Scalerから渡す
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

            rayLength = Vector3.Distance(_end, _start);

            Vector3 vec = (_end - _start).normalized;
            float halfRayLength = rayLength * .5f;

            _hit = _start + vec * halfRayLength;
            _end = _hit + vec * halfRayLength;

            EndBase.position = _end;

            _mat = Fude.material;

            thresholdDist = (_hit - _start).sqrMagnitude;
            _boot = true;
        }

        float thresholdDist = 0f;
        /*
        void Update()
        {
            if (!_boot) return;

            _start = this.transform.position;

            raychan();
        }
        */


        public void CallUpdate() 
        {
            if (!_boot) return;

            _start = this.transform.position;

            raychan();
        }

        //raycast
        int layerMask = 1 << 25;
        private float hitDistance = 0f;
        private float rayLength = 0f;
        private void raychan()
        {
            Vector3 vec = (_hit - _start).normalized;
            Ray ray = new Ray(_start, vec);
            Debug.DrawRay(_start, vec * rayLength, Color.red);

            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, rayLength, layerMask))
            {
                Vector2 uv = hit.textureCoord;
                BlitSc.PositionsArray[ID] = uv;
                hitDistance = hit.distance;
                setThreePoints(hit.point, true);
            }
            else
            {
                Vector3 _temp = _start + vec * rayLength * .5f;
                hitDistance = rayLength;
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
        private Vector3 _endBase;
        private void setThreePoints(Vector3 hitPos, bool isHit)
        {
            if (isHit)
            {
                if (_hitCount < 100) { _hitCount += 5; }
                
                if (IsFreeze)
                {
                    //_endBase = Vector3.Lerp(_endBase, targetEndBase, .05f);
                    _hit = _start + _freezeVec * rayLength * .5f;
                }
                else
                {
                    //hitしているときは筆の動きに応じて_endBaseが動くようにする
                    float switchEndBaseOffset = rayLength / hitDistance;
                    Vector3 targetEndBase = Vector3.Lerp(EndBase.position, EndCenterBase.position, switchEndBaseOffset);
                    _endBase = Vector3.Lerp(_endBase, targetEndBase, .2f);
                    _hit = rePosHit(.05f);
                    _end = rePosEnd(.1f);
                }
            }
            else
            {
                //hitしていない時は_endBaseはEndBase.positionにとどまるようにする
                _endBase = EndBase.position;
                _hit = rePosHit(.01f);
                _end = rePosEnd(.034f);
            }


            //押し付けていない時、_freezeVecを更新する。
            if (!IsFreeze) 
            {
                if (0 < _hitCount) { _hitCount--; }
                _freezeVec = (_hit - _start).normalized;
            }

            //筆が伸びないように長さを整える。
            Vector3 vecToHit = (_hit - _start).normalized;
            _hit = _start + vecToHit * rayLength * .5f;

            Vector3 vecToEnd = (_end - _hit).normalized;
            _end = _hit + vecToEnd * rayLength * .5f;

            setMaterialValue();
        }

        private Vector3 rePosHit(float offset)
        {
            float reposOffset_hit = 1f - Mathf.Clamp01(offset * _hitCount);
            return Vector3.Lerp(_hit, Vector3.Lerp(_start, _endBase, .5f), reposOffset_hit);
        }

        private Vector3 rePosEnd(float offset)
        {
            float reposOffset_end = 1f - Mathf.Clamp01(offset * _hitCount);
            return Vector3.Lerp(_end, _endBase, reposOffset_end);
        }


        private void setMaterialValue()
        {
            _mat.SetVector("_Start", _start);
            _mat.SetVector("_Hit", _hit);
            _mat.SetVector("_End", _end);
            _mat.SetVector("_EndBase", _endBase);
            //_mat.SetFloat("_InnerRange", InnerRange);
        }
    }
}
