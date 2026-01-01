using System;
using MSCLoader;
using UnityEngine;

namespace KekmetBinds.LeverHandling
{
    public static class InputHelper
    {
        /// <summary>
        /// Returns true if both Keybinds are held at the same time or either Keybind got released this frames.
        /// Use case: Holding both buttons should do nothing. --> Reset the capsule colliders.
        /// </summary>
        /// <param name="kb1">Setting: Keybind 1.</param>
        /// <param name="kb2">Setting: Keybind 2.</param>
        /// <returns></returns>
        public static bool KeybindBothHoldOrEitherUp(SettingsKeybind kb1, SettingsKeybind kb2)
        {
            return kb1.GetKeybind() && kb2.GetKeybind() || kb1.GetKeybindUp() || kb2.GetKeybindUp();
        }

        /// <summary>
        /// Inversed Lerp adjustment based on settings..
        /// </summary>
        /// <param name="joystick">Setting: Joystick index.</param>
        /// <param name="axis">Setting: Axis index.</param>
        /// <param name="lowered">Setting: Joystick % for the axis to be fully lowered.</param>
        /// <param name="raised">Setting: Joystick % for the axis to be fully raised.</param>
        /// <returns>Inversed lerp adjusted joystick position.</returns>
        public static float GetAdjustedAxisPercentage(SettingsSliderInt joystick, SettingsSliderInt axis, 
            SettingsSliderInt lowered, SettingsSliderInt raised)
        {
            float lower = Convert.ToInt32(lowered.GetValue()) / 100f;
            float raise = Convert.ToInt32(raised.GetValue()) / 100f;
            float input = GetJoystickInput(joystick, axis);
            return Mathf.InverseLerp(lower, raise, input);
        }

        /// <summary>
        /// Get the raw -1...1 values from a joystick + axis setting.
        /// </summary>
        /// <param name="joystick">Setting: Joystick index.</param>
        /// <param name="axis">Setting: Axis index.</param>
        /// <returns>Current joystick value from -1 to 1.</returns>
        private static float GetJoystickInput(SettingsSliderInt joystick, SettingsSliderInt axis)
        {
            return Input.GetAxis($"Joy{joystick.GetValue()} Axis {axis.GetValue()}");
        }
    }
}