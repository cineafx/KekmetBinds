using System;
using System.Collections.Generic;
using HutongGames.PlayMaker;
using MSCLoader;
using UnityEngine;
using UnityEngine.UI;

namespace KekmetBinds
{
    public class KekmetBinds : Mod
    {
        public override string ID => "KekmetBinds";
        public override string Name => "Kekmet Binds";
        public override string Author => "icdb / cineafx";
        public override string Version => "1.1";

        public override string Description => "";
        

        //Used to tell if the player has entered / exited a vehicle.
        private FsmString _currentVic;
        private string _lastVic;
        private Transform _playerTransform;
        private Transform _kekmetTransform;

        //All LeverHandlers checked every frame.
        private readonly List<LeverHandler> _leverHandlers = new List<LeverHandler>();

        private readonly string[] _joystickNames =
        {
            "None", "Not connected", "Not connected", "Not connected", "Not connected",
            "Not connected", "Not connected", "Not connected", "Not connected"
        };

        private readonly string[] _axisNames = {"", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10"};

        // @formatter:off
        // Keybinds
        private SettingKeybind _frontHydArmKeybindFore;
        private SettingKeybind _frontHydArmKeybindAft;

        private SettingKeybind _frontHydLoaderKeybindFore;
        private SettingKeybind _frontHydLoaderKeybindAft; 

        private SettingKeybind _throttleKeybindFore;   
        private SettingKeybind _throttleKeybindAft;        

        //Settings
        private SettingSlider _allowOutside;
        
        private SettingSlider _frontHydArmJoystick;
        private SettingSlider _frontHydArmAxis;
        private SettingSlider _frontHydArmLowered;
        private SettingSlider _frontHydArmRaised;

        private SettingSlider _frontHydLoaderJoystick;
        private SettingSlider _frontHydLoaderAxis;
        private SettingSlider _frontHydLoaderLowered;
        private SettingSlider _frontHydLoaderRaised;

        private SettingSlider _throttleJoystick;
        private SettingSlider _throttleAxis;
        private SettingSlider _throttleLowered;
        private SettingSlider _throttleRaised;

        // @formatter:on

        /// <summary>
        /// All settings should be created here. 
        /// DO NOT put anything else here that settings.
        /// </summary>
        public override void ModSettings()
        {
            //Prep
            Array.Copy(Input.GetJoystickNames(), 0, _joystickNames, 1, Input.GetJoystickNames().Length);

            //Settings
            modSettings.AddText("Max distance from Kekmet");
            _allowOutside = modSettings.AddSlider("kekMetAllowOutside", "(0 = player needs to be in \"driving mode\")", 0, 0, 50, 2);

            modSettings.AddHeader("Joystick settings:");
            modSettings.AddText("Axis use the same numbering system as the \"car controls\" menu. 1 - 10 not 0 - 9");
            modSettings.AddText("\"Fully lowered / raised %\" means at this joystick % the in-game lever will be at -100% / 100%.");

            modSettings.AddHeader("Front hydraulic arm");
            _frontHydArmJoystick = modSettings.AddSlider("kekMetFrontHydArmJoystick", "Joystick: Error", 0, 0, _joystickNames.Length - 1, () => UpdateJoystickName(_frontHydArmJoystick));
            UpdateJoystickName(_frontHydArmJoystick);
            _frontHydArmAxis = modSettings.AddSlider("kekMetFrontHydArmAxis", "Axis", 1, 1, 10, () => UpdateAxisName(_frontHydArmAxis));
            UpdateAxisName(_frontHydArmAxis);
            _frontHydArmLowered = modSettings.AddSlider("kekMetFrontHydArmLowered", "Fully lowered %", -100, -100, 100);
            _frontHydArmRaised = modSettings.AddSlider("kekMetFrontHydArmRaised", "Fully raised %", 100, -100, 100);

            modSettings.AddHeader("Front hydraulic fork");
            _frontHydLoaderJoystick = modSettings.AddSlider("kekMetFrontHydLoaderJoystick", "Joystick: Error", 0, 0, _joystickNames.Length - 1, () => UpdateJoystickName(_frontHydLoaderJoystick));
            UpdateJoystickName(_frontHydLoaderJoystick);
            _frontHydLoaderAxis = modSettings.AddSlider("kekMetFrontHydLoaderAxis", "Axis", 1, 1, 10, () => UpdateAxisName(_frontHydLoaderAxis));
            UpdateAxisName(_frontHydLoaderAxis);
            _frontHydLoaderLowered = modSettings.AddSlider("kekMetFrontHydLoaderLowered", "Fully lowered %", -100, -100, 100);
            _frontHydLoaderRaised = modSettings.AddSlider("kekMetFrontHydLoaderRaised", "Fully raised %", 100, -100, 100);
            
            modSettings.AddHeader("Front hydraulic fork");
            _throttleJoystick = modSettings.AddSlider("kekMetFrontHydLoaderJoystick", "Joystick: Error", 0, 0, _joystickNames.Length - 1, () => UpdateJoystickName(_throttleJoystick));
            UpdateJoystickName(_throttleJoystick);
            _throttleAxis = modSettings.AddSlider("kekMetFrontHydLoaderAxis", "Axis", 1, 1, 10, () => UpdateAxisName(_throttleAxis));
            UpdateAxisName(_throttleAxis);
            _throttleLowered = modSettings.AddSlider("kekMetFrontHydLoaderLowered", "Fully lowered %", -100, -100, 100);
            _throttleRaised = modSettings.AddSlider("kekMetFrontHydLoaderRaised", "Fully raised %", 100, -100, 100);

            //Keybinds
            modSettings.AddHeader("Front hydraulic arm");
            _frontHydArmKeybindFore = modSettings.AddKeybind("kekMetFrontHydArmFore", "forward (lower)", KeyCode.Keypad2);
            _frontHydArmKeybindAft = modSettings.AddKeybind("kekMetFrontHydArmAft", "backward (raise)", KeyCode.Keypad5);
            modSettings.AddHeader("Front hydraulic fork");
            _frontHydLoaderKeybindFore = modSettings.AddKeybind("kekMetFrontHydLoaderFore", "forward (lower)", KeyCode.Keypad1);
            _frontHydLoaderKeybindAft = modSettings.AddKeybind("kekMetFrontHydLoaderAft", "backward (raise)", KeyCode.Keypad4);
            modSettings.AddHeader("Hand throttle");
            _throttleKeybindFore = modSettings.AddKeybind("kekMetThrottleFore", "forward (lower)", KeyCode.Keypad3);
            _throttleKeybindAft = modSettings.AddKeybind("kekMetThrottleAft", "backward (raise)", KeyCode.Keypad6);
        }

        /// <summary>
        /// Update the joystick name without redrawing the entire settings menu. This allows for dragging the sliders all the way.
        /// This isn't nice. But works...
        /// </summary>
        /// <param name="setting">Setting of the joystick.</param>
        private void UpdateJoystickName(SettingSlider setting)
        {
            setting.Name = $"Joystick: {_joystickNames[Convert.ToInt32(setting.Value)]}";
        }
        
        /// <summary>
        /// Update the Axis name without redrawing the entire settings menu. This allows for dragging the sliders all the way.
        /// This isn't nice. But works...
        /// </summary>
        /// <param name="setting">Setting of the Axis.</param>
        private void UpdateAxisName(SettingSlider setting)
        {
            setting.Name = $"Axis: {_axisNames[Convert.ToInt32(setting.Value)]}";
        }

        /// <summary>
        /// Called once, when mod is loading after game is fully loaded.
        /// </summary>
        public override void OnLoad()
        {
            _currentVic = PlayMakerGlobals.Instance.Variables.FindFsmString("PlayerCurrentVehicle");
            _playerTransform = GameObject.Find("PLAYER").transform;
            _kekmetTransform = GameObject.Find("KEKMET(350-400psi)").transform;

            _leverHandlers.Add(new LeverHandler(
                _kekmetTransform.Find("Dashboard/FrontHydArm").gameObject.GetComponent<PlayMakerFSM>(),
                _frontHydArmKeybindFore,
                _frontHydArmKeybindAft,
                _frontHydArmJoystick,
                _frontHydArmAxis,
                _frontHydArmLowered,
                _frontHydArmRaised
            ));
            _leverHandlers.Add(new LeverHandler(
                _kekmetTransform.Find("Dashboard/FrontHydLoader").gameObject.GetComponent<PlayMakerFSM>(),
                _frontHydLoaderKeybindFore,
                _frontHydLoaderKeybindAft,
                _frontHydLoaderJoystick,
                _frontHydLoaderAxis,
                _frontHydLoaderLowered,
                _frontHydLoaderRaised
            ));
            _leverHandlers.Add(new LeverHandler(
                _kekmetTransform.Find("LOD/Dashboard/Throttle").gameObject.GetComponent<PlayMakerFSM>(),
                _throttleKeybindFore,
                _throttleKeybindAft,
                _throttleJoystick,
                _throttleAxis,
                _throttleLowered,
                _throttleRaised,
                70
            ));
        }

        /// <summary>
        /// Update is called once per frame.
        /// </summary>
        public override void Update()
        {
            int allowOutsideDist = Convert.ToInt32(_allowOutside.Value);
            // it's 0 if the feature is disabled
            if (allowOutsideDist > 0)
            {
                float distanceToKekmet = Vector3.Distance(_playerTransform.position, _kekmetTransform.position);
                _leverHandlers.ForEach(leverHandler => leverHandler.IsInVehicle = distanceToKekmet < allowOutsideDist);
                _leverHandlers.ForEach(leverHandler => leverHandler.Handle());
                return;
            }

            //Reset if player leaves vehicle
            if (_currentVic.Value.Length == 0)
            {
                if (_lastVic == _currentVic.Value) return;

                _lastVic = _currentVic.Value;
                _leverHandlers.ForEach(leverHandler => leverHandler.IsInVehicle = false);
                return;
            }

            // Check if the current vehicle has changed and tell the handlers
            if (_lastVic != _currentVic.Value)
            {
                _lastVic = _currentVic.Value;
                _leverHandlers.ForEach(leverHandler => leverHandler.IsInVehicle = _currentVic.Value == "Kekmet");
            }

            _leverHandlers.ForEach(leverHandler => leverHandler.Handle());
        }
    }
}
