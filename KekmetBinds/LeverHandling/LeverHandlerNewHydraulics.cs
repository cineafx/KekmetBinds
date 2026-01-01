using System;
using MSCLoader;

namespace KekmetBinds.LeverHandling
{
    public class LeverHandlerNewHydraulics : ILeverHandler
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
        public LeverHandlerNewHydraulics(
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
        }

        private readonly PlayMakerFSM _fsm;
        private readonly SettingsKeybind _fore;
        private readonly SettingsKeybind _aft;
        private readonly SettingsSliderInt _joystick;
        private readonly SettingsSliderInt _axis;
        private readonly SettingsSliderInt _lowered;
        private readonly SettingsSliderInt _raised;
        private readonly float _leverPosMax;

        private bool _isInVehicle;

        public bool IsInVehicle
        {
            get => _isInVehicle;
            set
            {
                _isInVehicle = value;
                if (!_isInVehicle)
                    _fsm.SendEvent("OFF");
            }
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
                _fsm.SendEvent("OFF");
            else if (_fore.GetKeybind())
                _fsm.SendEvent("DECREASE");
            else if (_aft.GetKeybind())
                _fsm.SendEvent("INCREASE");

            //No joystick configured
            if (Convert.ToInt32(_joystick.GetValue()) == 0) return;

            float currentLeverPos = _fsm.FsmVariables.GetFsmFloat("LeverPos").Value / _leverPosMax;
            float currentJoystickPos = InputHelper.GetAdjustedAxisPercentage(_joystick, _axis, _lowered, _raised);

            if ((currentLeverPos - currentJoystickPos) / 2f > JoystickFloatComparator)
            {
                // Don't activate the state if it's already active
                // These two IFs can't be merged. The else path would run every update and reset the handlers!
                if (_fsm.ActiveStateName != "DECREASE")
                    _fsm.SendEvent("DECREASE");
            }
            else if ((currentLeverPos - currentJoystickPos) / 2f < -JoystickFloatComparator)
            {
                // Don't activate the state if it's already active
                // These two IFs can't be merged. The else path would run every update and reset the handlers!
                if (_fsm.ActiveStateName != "INCREASE")
                    _fsm.SendEvent("INCREASE");
            }
            else
                _fsm.SendEvent("OFF");
        }
    }
}