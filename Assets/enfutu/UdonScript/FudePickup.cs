
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Components;

namespace enfutu.UdonScript
{
    //[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class FudePickup : UdonSharpBehaviour
    {
        [SerializeField] FudeScaler _scalerSc;
        private VRCPickup _pickup;

        void Start()
        {
            _pickup = (VRCPickup)GetComponent(typeof(VRCPickup));
        }

        public override void OnPickup()
        {
            if (!Networking.LocalPlayer.IsOwner(this.gameObject))
            {
                Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
            }

            int pickupHand = -1;
            if (_pickup.currentHand == VRC_Pickup.PickupHand.Left) { pickupHand = 0; }
            if (_pickup.currentHand == VRC_Pickup.PickupHand.Right) { pickupHand = 1; }

            _scalerSc.CurrentHand = pickupHand;
        }
    }
}
