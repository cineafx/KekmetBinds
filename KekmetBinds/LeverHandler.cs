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
        public LeverHandler(PlayMakerFSM fsm, Keybind fore, Keybind aft)
        {
            _fsm = fsm;
            _fore = fore;
            _aft = aft;

            _capsuleCollider = _fsm.gameObject.GetComponent<CapsuleCollider>();
            _defaultColliderCenter = _capsuleCollider.center;

            _camera = (Camera) Object.FindObjectOfType(typeof(Camera));
        }

        private readonly PlayMakerFSM _fsm;
        private readonly CapsuleCollider _capsuleCollider;
        private readonly Keybind _fore;
        private readonly Keybind _aft;
        private readonly Vector3 _defaultColliderCenter;
        private readonly Camera _camera;

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

        /// <summary>
        /// Move the colliders based on their offset.
        /// </summary>
        /// <param name="fsmEvent"></param>
        private void SetLeverHandler(string fsmEvent)
        {
            // Position 1m away from the center of the camera
            Vector3 pos = _camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 1f));
            _capsuleCollider.center = _capsuleCollider.transform.InverseTransformPoint(pos);
            _fsm.SendEvent(fsmEvent);
        }

        /// <summary>
        /// Move the colliders back to their default location.
        /// </summary>
        private void ResetLeverHandler()
        {
            _capsuleCollider.center = _defaultColliderCenter;
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
