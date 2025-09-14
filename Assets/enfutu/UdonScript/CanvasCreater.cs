
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace enfutu.UdonScript
{
    //[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class CanvasCreater : UdonSharpBehaviour
    {
        public Vector3 Bounds;
        public int Length;
        [SerializeField] GameObject _source;
        private GameObject[] _canvas;
        private Material[] _canvasMat;

        void Start()
        {
            if(Length <= 0) return;

            _canvas = new GameObject[Length];
            _canvasMat = new Material[Length];

            float offset = Bounds.z / Length * 2;
            for(int i = 0; i < Length; i++)
            {
                Vector3 pos = this.transform.position;
                pos.z -= Bounds.z;
                pos.z += offset * i;
                _canvas[i] = Instantiate(_source, pos, Quaternion.identity, this.transform);

                _canvasMat[i] = _canvas[i].GetComponent<MeshRenderer>().material;
                _canvasMat[i].SetInt("_ID", i);
                _canvasMat[i].SetInt("_MaxLength", Length);
            }

            _source.SetActive(false);
        }
    }
}
