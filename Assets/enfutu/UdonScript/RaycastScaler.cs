
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
        [SerializeField] GameObject _source;
        public BlitSystem BlitSc;

        void Start()
        {
            if(count <= 0) { return; }

            //BlitSystemSetup
            BlitSc.count = count;
            BlitSc.Boot();

            _raycaster = new GameObject[count];

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

                var script = _raycaster[i].GetComponent<Raycaster>();
                
                script.StartBase.position = _startBase.position;
                script.EndBase.position = _endBase.position;
                script.ID = i;
                script.BlitSc = BlitSc;

                script.Boot();
            }
        }
    }
}
