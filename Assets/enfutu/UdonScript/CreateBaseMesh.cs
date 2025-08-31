
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace enfutu.UdonScript
{
    //[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class CreateBaseMesh : UdonSharpBehaviour
    {
        [SerializeField] GameObject empty;

        void Start()
        {
            Create();
        }


        private MeshFilter _filter;
        private Mesh _mesh;
        
        //mesh Data
        private Vector3[] _vert = new Vector3[4];
        private int[] _tr = new int[6];
                
        private float width = 4.096f;
        private float hight = .001f;
        public void Create()
        {
            _filter = empty.GetComponent<MeshFilter>();
            _mesh = new Mesh();

            _vert[0] = new Vector3(0, 0, 0);
            _vert[1] = new Vector3(width, 0, 0);
            _vert[2] = new Vector3(0, hight, 0);
            _vert[3] = new Vector3(width, hight, 0);

            _mesh.SetVertices(_vert);

            _tr[0] = 0;
            _tr[1] = 2;
            _tr[2] = 1;
            _tr[3] = 2;
            _tr[4] = 3;
            _tr[5] = 1;

            _mesh.SetTriangles(_tr, 0);

            _filter.mesh = _mesh;
        }
    }
}
