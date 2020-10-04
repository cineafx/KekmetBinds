using System.Net.Configuration;
using MSCLoader;
using UnityEngine;

namespace KekmetBinds
{
    public class LeverHandler
    {
        public LeverHandler(PlayMakerFSM fsm, float capsuleNewRadius, Vector3 leverOffset)
        {
            _fsm = fsm;
            _capsuleNewRadius = capsuleNewRadius;
            _leverOffset = leverOffset;
            
            _capsuleCollider = _fsm.gameObject.GetComponent<CapsuleCollider>();
            _defaultCapsuleRadius = _capsuleCollider.radius;
            _defaultLocalPosColl = _capsuleCollider.transform.localPosition;

            _lever = _fsm.gameObject.transform.Find("Lever");
            _defaultLocalPosLever = _lever.localPosition;
        }

        private bool _isInVehicle;
        public bool IsInVehicle
        {
            get => _isInVehicle;
            set
            {
                _isInVehicle = value;
                if (!_isInVehicle)
                {
                    ResetLeverHandler();
                }
            }
        }

        private PlayMakerFSM _fsm;
        private readonly CapsuleCollider _capsuleCollider;
        private readonly Transform _lever;
        private readonly float _defaultCapsuleRadius;
        private readonly Vector3 _defaultLocalPosColl;
        private readonly Vector3 _defaultLocalPosLever;
        private readonly float _capsuleNewRadius;
        private readonly Vector3 _leverOffset;
        private Vector3 LeverOffsetDown => new Vector3(_leverOffset.x, _leverOffset.y, -_leverOffset.z);

        private void SetLeverHandler(string fsmEvent)
        {
            _capsuleCollider.radius = _capsuleNewRadius;
            _capsuleCollider.transform.localPosition = _defaultLocalPosColl + _leverOffset;
            _lever.localPosition = _defaultLocalPosLever + LeverOffsetDown;
            _fsm.SendEvent(fsmEvent);
        }

        private void ResetLeverHandler()
        {
            _capsuleCollider.radius = _defaultCapsuleRadius;
            _capsuleCollider.transform.localPosition = _defaultLocalPosColl;
            _lever.localPosition = _defaultLocalPosLever;
            _fsm.SendEvent("FINISHED");
        }

        public void HandleKeyBinds(Keybind fore, Keybind aft)
        {
            if (!IsInVehicle) return;
            
            // Holding both buttons should do nothing
            if (KeybindBothHoldOrEitherUp(fore, aft))
                ResetLeverHandler();
            else if (fore.GetKeybind())
                SetLeverHandler("DECREASE");
            else if (aft.GetKeybind())
                SetLeverHandler("INCREASE");
        }
        

        /// <summary>
        /// Returns true if both Keybinds are held at the same time or either Keybind got released this frames.
        /// Use case: Holding both buttons should do nothing. --> Reset the capsule colliders.
        /// </summary>
        /// <param name="kb1"></param>
        /// <param name="kb2"></param>
        /// <returns></returns>
        private static bool KeybindBothHoldOrEitherUp(Keybind kb1, Keybind kb2)
        {
            return kb1.GetKeybind() && kb2.GetKeybind() || kb1.GetKeybindUp() || kb2.GetKeybindUp();
        }
    }
}
