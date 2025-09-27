
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Components;

namespace enfutu.UdonScript
{
    //ObjectSyncに関係するので自動同期。
    //ついでなのでここで同期変数も持つ。
    [UdonBehaviourSyncMode(BehaviourSyncMode.Continuous)]
    public class FudePickup : UdonSharpBehaviour
    {
        [SerializeField] FudeScaler _scalerSc;
        private VRCPickup _pickup;

        void Start()
        {
            _pickup = (VRCPickup)GetComponent(typeof(VRCPickup));
        }
        
        [UdonSynced(UdonSyncMode.None)] public int CurrentPlayerID = -1;
        private int _currentPlayerID = -1;
        void Update()
        {
            if(_currentPlayerID != CurrentPlayerID)
            {
                _currentPlayerID = CurrentPlayerID;
                _scalerSc.CurrentPlayerID = CurrentPlayerID;
                _scalerSc.ChangeCurrentPlayer();
            }

        }

        public override void OnPickup()
        {
            if (!Networking.LocalPlayer.IsOwner(this.gameObject))
            {
                Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
                Networking.SetOwner(Networking.LocalPlayer, _scalerSc.gameObject);
            }

            int pickupHand = -1;
            if (_pickup.currentHand == VRC_Pickup.PickupHand.Left) { pickupHand = 0; }
            if (_pickup.currentHand == VRC_Pickup.PickupHand.Right) { pickupHand = 1; }

            CurrentPlayerID = VRCPlayerApi.GetPlayerId(_pickup.currentPlayer);

            _scalerSc.CurrentHand = pickupHand;
        }
    }
}
