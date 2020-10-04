using System.Net.Configuration;
using MSCLoader;
using UnityEngine;

namespace KekmetBinds
{
    public class LeverHandler
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fsm">Fsm of the controlling component.</param>
        /// <param name="fore">Fore movement keybind.</param>
        /// <param name="aft">Aft movement keybind.</param>
        /// <param name="capsuleNewRadius">Radius for the capsule collider.</param>
        /// <param name="offsetCollider">By how much and in which direction should the collider be moved.</param>
        /// <param name="offsetLeverDirection">In which direction should be visible lever be moved back. Vector3 with either -1 or 1 inside the fields.</param>
        /// <param name="leverName">Name of the visible lever to be moved back.</param>
        public LeverHandler(PlayMakerFSM fsm, Keybind fore, Keybind aft, float capsuleNewRadius,
            Vector3 offsetCollider, Vector3 offsetLeverDirection, string leverName = "Lever")
        {
            _fsm = fsm;
            _fore = fore;
            _aft = aft;
            _capsuleNewRadius = capsuleNewRadius;
            _offsetCollider = offsetCollider;
            _offsetLever = Vector3.Scale(offsetCollider, offsetLeverDirection);

            _capsuleCollider = _fsm.gameObject.GetComponent<CapsuleCollider>();
            _defaultCapsuleRadius = _capsuleCollider.radius;
            _defaultLocalPosCollider = _capsuleCollider.transform.localPosition;

            _lever = _fsm.gameObject.transform.Find(leverName);
            _defaultLocalPosLever = _lever.localPosition;
        }

        private bool _isInVehicle;

        public bool IsInVehicle
        {
            private get => _isInVehicle;
            set
            {
                _isInVehicle = value;
                if (!_isInVehicle) ResetLeverHandler();
            }
        }

        private readonly PlayMakerFSM _fsm;
        private readonly CapsuleCollider _capsuleCollider;
        private readonly Transform _lever;
        private readonly Keybind _fore;
        private readonly Keybind _aft;
        private readonly float _defaultCapsuleRadius;
        private readonly Vector3 _defaultLocalPosCollider;
        private readonly Vector3 _defaultLocalPosLever;
        private readonly float _capsuleNewRadius;
        private readonly Vector3 _offsetCollider;
        private readonly Vector3 _offsetLever;

        /// <summary>
        /// Move the colliders based on their offset.
        /// </summary>
        /// <param name="fsmEvent"></param>
        private void SetLeverHandler(string fsmEvent)
        {
            _capsuleCollider.radius = _capsuleNewRadius;
            _capsuleCollider.transform.localPosition = _defaultLocalPosCollider + _offsetCollider;
            _lever.localPosition = _defaultLocalPosLever + _offsetLever;
            _fsm.SendEvent(fsmEvent);
        }

        /// <summary>
        /// Move the colliders back to their default location.
        /// </summary>
        private void ResetLeverHandler()
        {
            _capsuleCollider.radius = _defaultCapsuleRadius;
            _capsuleCollider.transform.localPosition = _defaultLocalPosCollider;
            _lever.localPosition = _defaultLocalPosLever;
            _fsm.SendEvent("FINISHED");
        }

        /// <summary>
        /// Should be run every frame (inside of Update()).
        /// Requires the public variable IsInVehicle to be set correctly.
        /// </summary>
        public void HandleKeyBinds()
        {
            if (!IsInVehicle) return;

            // Holding both buttons should do nothing
            if (KeybindBothHoldOrEitherUp(_fore, _aft))
                ResetLeverHandler();
            else if (_fore.GetKeybind())
                SetLeverHandler("DECREASE");
            else if (_aft.GetKeybind())
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
