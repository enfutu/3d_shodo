
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace enfutu.UdonScript
{
    //[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class BaseMeshCopy : UdonSharpBehaviour
    {
        [SerializeField] GameObject _baseMesh;

        void Start()
        {

        }

        /*
        private GameObject[] _clone = new GameObject[5];
        public override void Interact() 
        {
            for(int i = 0; i < 5; i++)
            {
                _clone[i] = Clone(_baseMesh);
            }
        }

        public GameObject Clone(GameObject obj)
        {
            var clone = Instantiate(obj) as GameObject;
            clone.transform.parent = obj.transform.parent;
            clone.transform.localPosition = obj.transform.localPosition;
            clone.transform.localScale = obj.transform.localScale;
            return clone;
        }*/
    }
}
