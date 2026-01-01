using System;
using MSCLoader;
using UnityEngine;
using Object = UnityEngine.Object;

namespace KekmetBinds.LeverHandling
{
    public class LeverHandler : ILeverHandler
    {
        // Bigger values = bigger steps
        // Smaller values = more jitter
        // TODO: Move this into a setting? 🤔
        private const float JoystickFloatComparator = 0.0025f;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fsm">Fsm of the controlling component.</param>
        /// <param name="fore">Keybind: Fore movement.</param>
        /// <param name="aft">Keybind: Aft movement.</param>
        /// <param name="joystick">Setting: Joystick index.</param>
        /// <param name="axis">Setting: Axis index.</param>
        /// <param name="lowered">Setting: Joystick % for the axis to be fully lowered.</param>
        /// <param name="raised">Setting: Joystick % for the axis to be fully raised.</param>
        /// <param name="leverPosMax">Some levers go from 0 to 1 (FrontHyd). Some go from 0 to 70 (Throttle).</param>
        public LeverHandler(
            PlayMakerFSM fsm,
            SettingsKeybind fore,
            SettingsKeybind aft,
            SettingsSliderInt joystick,
            SettingsSliderInt axis,
            SettingsSliderInt lowered,
            SettingsSliderInt raised,
            float leverPosMax = 1
        )
        {
            _fsm = fsm;
            _fore = fore;
            _aft = aft;
            _joystick = joystick;
            _axis = axis;
            _lowered = lowered;
            _raised = raised;
            _leverPosMax = leverPosMax;

            _capsuleCollider = _fsm.gameObject.GetComponent<CapsuleCollider>();
            _defaultColliderCenter = _capsuleCollider.center;

            _camera = (Camera)Object.FindObjectOfType(typeof(Camera));
        }

        private readonly PlayMakerFSM _fsm;
        private readonly CapsuleCollider _capsuleCollider;
        private readonly SettingsKeybind _fore;
        private readonly SettingsKeybind _aft;
        private readonly SettingsSliderInt _joystick;
        private readonly SettingsSliderInt _axis;
        private readonly SettingsSliderInt _lowered;
        private readonly SettingsSliderInt _raised;
        private readonly float _leverPosMax;
        private readonly Vector3 _defaultColliderCenter;
        private readonly Camera _camera;

        private bool _isInVehicle;

        public bool IsInVehicle
        {
            get => _isInVehicle;
            set
            {
                _isInVehicle = value;
                if (!_isInVehicle)
                    ResetLeverHandler();
            }
        }

        /// <summary>
        /// Move the collider in front of the cursor and send the <c>fsmEvent</c> event.
        /// </summary>
        /// <param name="fsmEvent">FSM event name like "INCREASE" or "DECREASE"</param>
        private void SetLeverHandler(string fsmEvent)
        {
            if (_fsm.ActiveStateName == fsmEvent)
            {
                //Move the levers one meter up. That way the next lever can be interacted with.
                //An alternative would be to move it to 0,0,0 but that might cause issues if you are too far away? idk.
                Vector3 posOutOfTheWay = _camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 1f) + Vector3.up);
                _capsuleCollider.center = _capsuleCollider.transform.InverseTransformPoint(posOutOfTheWay);
                return;
            }

            // Position 1m away from the center of the camera
            Vector3 pos = _camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 1f));
            _capsuleCollider.center = _capsuleCollider.transform.InverseTransformPoint(pos);
            _fsm.SendEvent(fsmEvent);
        }

        /// <summary>
        /// Move the colliders back to their default location and send the FINISHED event.
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
        public void Handle()
        {
            if (!IsInVehicle) return;
            // Holding both buttons should do nothing
            if (InputHelper.KeybindBothHoldOrEitherUp(_fore, _aft))
                ResetLeverHandler();
            else if (_fore.GetKeybind())
                SetLeverHandler("DECREASE");
            else if (_aft.GetKeybind())
                SetLeverHandler("INCREASE");

            //No joystick configured
            if (Convert.ToInt32(_joystick.GetValue()) == 0) return;

            float currentLeverPos = _fsm.FsmVariables.GetFsmFloat("LeverPos").Value / _leverPosMax;
            float currentJoystickPos = InputHelper.GetAdjustedAxisPercentage(_joystick, _axis, _lowered, _raised);

            if ((currentLeverPos - currentJoystickPos) / 2f > JoystickFloatComparator)
            {
                // Don't activate the state if it's already active
                // These two IFs can't be merged. The else path would run every update and reset the handlers!
                if (_fsm.ActiveStateName != "DECREASE")
                    SetLeverHandler("DECREASE");
            }
            else if ((currentLeverPos - currentJoystickPos) / 2f < -JoystickFloatComparator)
            {
                // Don't activate the state if it's already active
                // These two IFs can't be merged. The else path would run every update and reset the handlers!
                if (_fsm.ActiveStateName != "INCREASE")
                    SetLeverHandler("INCREASE");
            }
            else
                ResetLeverHandler();
        }
    }
}