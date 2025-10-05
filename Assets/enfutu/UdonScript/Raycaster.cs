
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace enfutu.UdonScript
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class Raycaster : UdonSharpBehaviour
    {
        //baseもendもpositionは筆の動きに追従する必要があるため、
        //gameObjectを保持し移動させ逐次確認した方が都合が良い
        //base
        public Transform EndBase;
        [HideInInspector] public Transform HitBase;           //Scalerから渡す
        [HideInInspector] public Transform EndCenterBase;     //Scalerから渡す
        //keep
        public Transform KeepHit;
        public Transform KeepEnd;

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
            //全ての初期値
            _start = this.transform.position;
            _end = EndBase.position;
            _endBase = EndBase.position;
            _hit = Vector3.Lerp(_start, _end, .5f);
            KeepEnd.position = _end;
            KeepHit.position = _hit;

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
        
        void Update()
        {
            if (!_boot) return;

            _start = this.transform.position;

            raychan();
        }
        

        /*
        public void CallUpdate() 
        {
            if (!_boot) return;

            _start = this.transform.position;

            raychan();
        }*/

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
            float sphereRadius = .01f;
            //if (Physics.Raycast(ray, out hit, rayLength, layerMask))
            if (Physics.SphereCast(ray, sphereRadius, out hit, rayLength, layerMask))
            {
                Vector2 uv = hit.textureCoord;
                BlitSc.PositionsArray[ID] = uv;
                hitDistance = hit.distance;
                setThreePoints(hit.point, true);
            }
            else
            {
                //Vector3 _temp = _start + vec * rayLength * .5f;
                Vector3 _temp = _start + (EndCenterBase.position - _start).normalized * rayLength * .5f;
                hitDistance = rayLength;
                setThreePoints(_temp, false);
            }
        }

        private int _maxHitCount = 60;
        private int _hitCount;
        public float FreezeCount; //Scalerから渡る(0～1)
        
        //前回の座標を保持
        private Vector3 _start;
        private Vector3 _hit;
        private Vector3 _end;
        private Vector3 _endBase;
        private void setThreePoints(Vector3 hitPos, bool isHit)
        {
            
            //hitしたかどうかを瞬間的に切り替えるのではなく、Countで幅を持たせる
            if (isHit)
            {
                _hitCount++;
            }
            else
            {
                _hitCount--;
            }
            _hitCount = (int)Mathf.Clamp(_hitCount, 0, _maxHitCount);

            float offset = _hitCount / _maxHitCount;
            offset = Mathf.Clamp01(offset - FreezeCount);
            float power_rePosHit = Mathf.Lerp(1, .034f, offset);
            float power_rePosEnd = Mathf.Lerp(1, .01f, offset);

            if (0 < _hitCount)
            {
                //まとまりながら動く筆の動き
                float switchEndBaseOffset = FreezeCount * .1f;
                Vector3 targetEndBase = EndCenterBase.position;
                Vector3 targetVec = (targetEndBase - _start).normalized;
                Vector3 target_hit = _start + targetVec * rayLength * .5f;
                Vector3 target_end = _start + targetVec * rayLength;
                Vector3 hitPos_follow = Vector3.Lerp(_hit, target_hit, .1f);
                Vector3 endPos_follow = Vector3.Lerp(_end, target_end, .05f);

                //ぶつかることでばらける筆の動き
                Vector3 currentHitPos = KeepHit.position;
                Vector3 currentEndPos = KeepEnd.position;
                Vector3 hitVec = (_hit - currentHitPos);
                Vector3 hitPos_stay = _hit + hitVec * .05f;
                Vector3 endPos_stay = _end + hitVec * .1f;

                //二つの動きをhitCountの割合で混ぜる
                _hit = Vector3.Lerp(hitPos_follow, hitPos_stay, offset);
                _end = Vector3.Lerp(endPos_follow, endPos_stay, offset);
            }
            else
            {
                //hit中でない時は_endBaseはEndBase.positionにとどまるようにする
                _endBase = EndBase.position;
                _hit = rePosHit(power_rePosHit);
                _end = rePosEnd(power_rePosEnd);
            }

            //筆が伸びないように長さを整える。
            float lengthToHit = Vector3.Distance(_hit, _start);
            //Vector3 vecToHit = (_hit - _start).normalized;
            //_hit = _start + vecToHit * rayLength * .5f;

            Vector3 vecToEnd = (_end - _hit).normalized;
            //_end = _hit + vecToEnd * rayLength * .5f;
            _end = _hit + vecToEnd * Mathf.Clamp(rayLength - lengthToHit, 0, rayLength);
            
            //keep
            KeepHit.position = _hit;
            KeepEnd.position = _end;

            setMaterialValue();
        }

        //元の位置へ戻ろうとする処理
        private Vector3 rePosHit(float power)
        {
            float reposOffset_hit = (hitDistance / rayLength) * power;
            return Vector3.Lerp(_hit, Vector3.Lerp(_start, _endBase, .5f), reposOffset_hit);
        }
        private Vector3 rePosEnd(float power)
        {
            float reposOffset_end = (hitDistance / rayLength) * power;
            return Vector3.Lerp(_end, _endBase, reposOffset_end);
        }


        private void setMaterialValue()
        {
            _mat.SetVector("_Start", _start);
            _mat.SetVector("_Hit", _hit);
            _mat.SetVector("_End", _end);
            _mat.SetVector("_EndBase", EndBase.position);
            
            //_mat.SetFloat("_InnerRange", InnerRange);
        }
    }
}
