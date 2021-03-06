﻿using System;
using MSCLoader;
using UnityEngine;
using Object = UnityEngine.Object;

namespace KekmetBinds
{
    public class LeverHandler
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
            SettingKeybind fore,
            SettingKeybind aft,
            SettingSlider joystick,
            SettingSlider axis,
            SettingSlider lowered,
            SettingSlider raised,
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

            _camera = (Camera) Object.FindObjectOfType(typeof(Camera));
        }

        private readonly PlayMakerFSM _fsm;
        private readonly CapsuleCollider _capsuleCollider;
        private readonly SettingKeybind _fore;
        private readonly SettingKeybind _aft;
        private readonly SettingSlider _joystick;
        private readonly SettingSlider _axis;
        private readonly SettingSlider _lowered;
        private readonly SettingSlider _raised;
        private readonly float _leverPosMax;
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
            if (KeybindBothHoldOrEitherUp(_fore, _aft))
                ResetLeverHandler();
            else if (_fore.GetKey())
                SetLeverHandler("DECREASE");
            else if (_aft.GetKey())
                SetLeverHandler("INCREASE");

            //No joystick configured
            if (Convert.ToInt32(_joystick.Value) == 0) return;

            float currentLevelPos = _fsm.FsmVariables.GetFsmFloat("LeverPos").Value / _leverPosMax;
            float currentJoystickPos = GetAdjustedAxisPercentage(_joystick, _axis, _lowered, _raised);

            if ((currentLevelPos - currentJoystickPos) / 2f > JoystickFloatComparator)
            {
                // Don't activate the state if it's already active
                // These two IFs can't be merged. The else path would run every update and reset the handlers!
                if (_fsm.ActiveStateName != "DECREASE")
                    SetLeverHandler("DECREASE");
            }
            else if ((currentLevelPos - currentJoystickPos) / 2f < -JoystickFloatComparator)
            {
                // Don't activate the state if it's already active
                // These two IFs can't be merged. The else path would run every update and reset the handlers!
                if (_fsm.ActiveStateName != "INCREASE")
                    SetLeverHandler("INCREASE");
            }
            else
                ResetLeverHandler();
        }

        /// <summary>
        /// Returns true if both Keybinds are held at the same time or either Keybind got released this frames.
        /// Use case: Holding both buttons should do nothing. --> Reset the capsule colliders.
        /// </summary>
        /// <param name="kb1">Setting: Keybind 1.</param>
        /// <param name="kb2">Setting: Keybind 2.</param>
        /// <returns></returns>
        private static bool KeybindBothHoldOrEitherUp(SettingKeybind kb1, SettingKeybind kb2)
        {
            return kb1.GetKey() && kb2.GetKey() || kb1.GetKeyUp() || kb2.GetKeyUp();
        }

        /// <summary>
        /// Inversed Lerp adjustment based on settings..
        /// </summary>
        /// <param name="joystick">Setting: Joystick index.</param>
        /// <param name="axis">Setting: Axis index.</param>
        /// <param name="lowered">Setting: Joystick % for the axis to be fully lowered.</param>
        /// <param name="raised">Setting: Joystick % for the axis to be fully raised.</param>
        /// <returns>Inversed lerp adjusted joystick position.</returns>
        private static float GetAdjustedAxisPercentage(SettingSlider joystick, SettingSlider axis, 
            SettingSlider lowered, SettingSlider raised)
        {
            float lower = Convert.ToInt32(lowered.Value) / 100f;
            float raise = Convert.ToInt32(raised.Value) / 100f;
            float input = GetJoystickInput(joystick, axis);
            return Mathf.InverseLerp(lower, raise, input);
        }

        /// <summary>
        /// Get the raw -1...1 values from a joystick + axis setting.
        /// </summary>
        /// <param name="joystick">Setting: Joystick index.</param>
        /// <param name="axis">Setting: Axis index.</param>
        /// <returns>Current joystick value from -1 to 1.</returns>
        private static float GetJoystickInput(SettingSlider joystick, SettingSlider axis)
        {
            return Input.GetAxis($"Joy{joystick.Value} Axis {axis.Value}");
        }
    }
}
