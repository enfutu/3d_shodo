
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace enfutu.UdonScript
{
    //[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class RaycastScaler : UdonSharpBehaviour
    {
        [SerializeField] Transform _startBase;
        [SerializeField] Transform _endBase;
        public float Radius = .02f;
        public int count;
        private GameObject[] _raycaster;
        private Raycaster[] _script;
        [SerializeField] GameObject _source;
        public BlitSystem BlitSc;

        void Start()
        {
            if(count <= 0) { return; }

            _oldPos = _startBase.position;
            _raycaster = new GameObject[count];
            _script = new Raycaster[count];

            Vector3 forward = this.transform.forward;

            Vector3 up = Vector3.up;
            if (Vector3.Dot(forward, up) > 0.99f)
            {
                up = Vector3.right;
            }

            Vector3 right = Vector3.Cross(up, forward).normalized;
            Vector3 planeUp = Vector3.Cross(forward, right).normalized;

            for (int i = 0; i < count; i++)
            {               
                float angle = (Mathf.PI * 2 / count) * i;
                Vector3 offset = (Mathf.Cos(angle) * right + Mathf.Sin(angle) * planeUp) * Radius;
                Vector3 pos = this.transform.position + offset;

                _raycaster[i] = Instantiate(_source, pos, Quaternion.identity, this.transform);

                float randomAngle = Random.Range(0f, 360f);
                Quaternion randomRot = Quaternion.AngleAxis(randomAngle, forward);
                _raycaster[i].transform.rotation = randomRot;

                _script[i] = _raycaster[i].GetComponent<Raycaster>();

                //_script[i].StartBase.position = _startBase.position;
                _script[i].StartBase.position = pos;
                _script[i].EndBase.position = _endBase.position;
                _script[i].ID = i;
                _script[i].BlitSc = BlitSc;

                _script[i].Boot();
            }

            _source.SetActive(false);
            
            //BlitSystemSetup
            BlitSc.count = count;
            BlitSc.Boot();
        }

        private Vector3 _oldPos; 
        private bool _isFreeze = false;
        private int _freezeCount = 0;
        private int _sumiCount = 0;
        void Update()
        {
            float length = (_startBase.position - _oldPos).sqrMagnitude;
            if(.01f < length) 
            {
                calcFreeze();
            }

            /*
            if(.001f < length)
            {
                updateRaycasters();
            }
            */
        }

        private void calcFreeze()
        {
            Vector3 currentVec = (_startBase.position - _oldPos).normalized;
            Vector3 forward = this.transform.forward;

            float d = Vector3.Dot(forward, currentVec);

            if (0f < d) 
            {
                _freezeCount = 10;
                if(0 < _sumiCount)
                {
                    _sumiCount--;
                }
            }
            else
            {
                _freezeCount--;
                if(_sumiCount < 100)
                {
                    _sumiCount++;
                }
            }

            if (0 < _freezeCount) { _isFreeze = true; }
            else { _isFreeze = false; }
            
            _oldPos = _startBase.position;

            Debug.Log("isFreeze : " + _isFreeze);

            for (int i = 0; i < count; i++)
            {
                _script[i].IsFreeze = _isFreeze;
                _script[i].SumiCount = _sumiCount;
            }
        }

        /*
        private void updateRaycasters()
        {
            for (int i = 0; i < count; i++)
            {
                _script[i].CalledUpdate();
            }
        }*/
    }
}
