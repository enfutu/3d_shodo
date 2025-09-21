
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Components;

namespace enfutu.UdonScript
{
    //[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class FudeScaler : UdonSharpBehaviour
    {
        //Debug
        [SerializeField] Transform _viewerIn;
        [SerializeField] Transform _viewerOut;

        public float InnerRange;
        public float OuterRange;
        [SerializeField] Transform _startBase;
        [SerializeField] Transform _hitBase;
        [SerializeField] Transform _endBase;


        public BlitSystem BlitSc;
        private VRCPlayerApi _localPlayer;
        void Start()
        {
            if (FiberCount <= 0) { return; }

            _localPlayer = Networking.LocalPlayer;

            clone();
            FibersSetPosition(1f);
            BootScripts();
            
            //BlitSystemSetup
            BlitSc.count = FiberCount;
            BlitSc.Boot();

            _boot = true;
        }

        public int FiberCount;
        private GameObject[] _raycaster;
        private Raycaster[] _script;
        [SerializeField] GameObject _source;
        private void clone()
        {
            _raycaster = new GameObject[FiberCount];
            _script = new Raycaster[FiberCount];
            for (int i = 0; i < FiberCount; i++)
            {
                _raycaster[i] = Instantiate(_source, this.transform.position, Quaternion.identity, this.transform);
                _script[i] = _raycaster[i].GetComponent<Raycaster>();
            }
            _source.SetActive(false);
        }

        public void BootScripts()
        {
            for (int i = 0; i < FiberCount; i++)
            {
                _script[i].EndBase.position = _endBase.position;
                //_script[i].OpenVec = offset;
                _script[i].ID = i;
                _script[i].BlitSc = BlitSc;
                _script[i].Boot();
            }
        }

        public float Radius;
        public void FibersSetPosition(float size)
        {
            //fudeOpenRadiusOffset = (FudeOpenRadius_Max - FudeOpenRadius_Min) * .1f;
            //fudeOpenRadius = Radius;
            //_oldPos = _startBase.position;


            //_hitBase.position = Vector3.Lerp(_endBase.position, _startBase.position, .5f);

            float _min = .1f;
            float fixedSize = Mathf.Clamp01(size + _min);

            if (!_boot)
            {
                Vector3 forward = this.transform.forward;

                Vector3 up = Vector3.up;
                if (Vector3.Dot(forward, up) > 0.99f) { up = Vector3.right; }
                
                Vector3 right = Vector3.Cross(up, forward).normalized;
                Vector3 planeUp = Vector3.Cross(forward, right).normalized;
                
                for (int i = 0; i < FiberCount; i++)
                {
                    float angle = (Mathf.PI * 2 / FiberCount) * i;
                    Vector3 offset = (Mathf.Cos(angle) * right + Mathf.Sin(angle) * planeUp);
                    float rad = Radius * fixedSize;
                    Vector3 pos = this.transform.position + offset * rad;
                    _raycaster[i].transform.position = pos;

                    float randomAngle = Random.Range(0f, 360f);
                    Quaternion randomRot = Quaternion.AngleAxis(randomAngle, forward);
                    _raycaster[i].transform.rotation = randomRot;
                }
            }
            else
            {
                Vector3 center = this.transform.position;
                for (int i = 0; i < FiberCount; i++)
                {
                    Vector3 vec = (_raycaster[i].transform.position - center).normalized;
                    _raycaster[i].transform.position = center + vec * Radius * fixedSize;
                    _script[i].EndBase.position = _endBase.position;
                    _script[i].InnerRange = InnerRange;
                }
            }
        }


        bool _boot = false;
        void Update()
        {
            if (!_boot) return;
            calcScale();
        }

        public int CurrentHand = -1;
        private void calcScale()
        {
            Vector3 pos = this.transform.position;

            Vector3 center = _localPlayer.GetBonePosition(HumanBodyBones.Head);
            //if(CurrentHand == 0) { center = _localPlayer.GetBonePosition(HumanBodyBones.LeftHand); }
            //if(CurrentHand == 1) { center = _localPlayer.GetBonePosition(HumanBodyBones.RightHand); }

            Vector3 vec = (pos - center).normalized;
            Vector3 pos_inner = center + vec * InnerRange * .5f;
            Vector3 pos_outer = center + vec * OuterRange * .5f;

            _viewerIn.position = pos_inner;
            _viewerOut.position = pos_outer;

            //毎フレームは必要ない気がする
            _viewerIn.localScale = Vector3.one * InnerRange;
            _viewerOut.localScale = Vector3.one * OuterRange;

            float baseLength = (OuterRange - InnerRange);
            baseLength = Mathf.Pow(baseLength, 2);
            float currentLength = (center + vec * OuterRange - pos).sqrMagnitude;

            float size = 1f - Mathf.Clamp01(currentLength / baseLength);

            //もしvecが反転していたら0とする
            float d = Vector3.Dot(this.transform.forward, vec);
            if (d <= 0) { size = 0f; }

            //もしOuterRangeより遠くに筆先があるなら1とする
            if (Mathf.Pow(OuterRange, 2) < (pos - center).sqrMagnitude) { size = 1f; }

            //fudeDummy.localScale = Vector3.one * size;
            //fudeDummy.position = pos;

            FibersSetPosition(size);
        }





    }
}
