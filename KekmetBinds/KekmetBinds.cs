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
        private readonly Keybind _frontHydArmKeybindFore = new Keybind("kekMetFrontHydArmFore", "forward (lower)", KeyCode.Keypad2);
        private readonly Keybind _frontHydArmKeybindAft = new Keybind("kekMetFrontHydArmAft", "backward (raise)", KeyCode.Keypad5);

        private readonly Keybind _frontHydLoaderKeybindFore = new Keybind("kekMetFrontHydLoaderFore", "forward (lower)", KeyCode.Keypad1);
        private readonly Keybind _frontHydLoaderKeybindAft = new Keybind("kekMetFrontHydLoaderAft", "backward (raise)", KeyCode.Keypad4);

        private readonly Keybind _throttleKeybindFore = new Keybind("kekMetThrottleFore", "forward (lower)", KeyCode.Keypad3);
        private readonly Keybind _throttleKeybindAft = new Keybind("kekMetThrottleAft", "backward (raise)", KeyCode.Keypad6);

        //Settings
        private readonly Settings _allowOutside = new Settings("kekMetAllowOutside", "(0 = player needs to be in \"driving mode\")", 0);
        
        private Settings _frontHydArmJoystick ;
        private readonly Settings _frontHydArmAxis = new Settings("kekMetFrontHydArmAxis", "Axis", 1);
        private readonly Settings _frontHydArmLowered = new Settings("kekMetFrontHydArmLowered", "Fully lowered %", -100);
        private readonly Settings _frontHydArmRaised = new Settings("kekMetFrontHydArmRaised", "Fully raised %", 100);

        private Settings _frontHydLoaderJoystick;
        private readonly Settings _frontHydLoaderAxis = new Settings("kekMetFrontHydLoaderAxis", "Axis", 1);
        private readonly Settings _frontHydLoaderLowered = new Settings("kekMetFrontHydLoaderLowered", "Fully lowered %", -100);
        private readonly Settings _frontHydLoaderRaised = new Settings("kekMetFrontHydLoaderRaised", "Fully raised %", 100);

        private Settings _throttleJoystick;
        private readonly Settings _throttleAxis = new Settings("kekMetThrottleAxis", "Axis", 1);
        private readonly Settings _throttleLowered = new Settings("kekMetThrottleLowered", "Fully lowered %", -100);
        private readonly Settings _throttleRaised = new Settings("kekMetThrottleRaised", "Fully raised %", 100);

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
            Settings.AddText(this, "Max distance from Kekmet");
            Settings.AddSlider(this, _allowOutside, 0, 50);

            Settings.AddHeader(this, "Joystick settings:");
            Settings.AddText(this, "Axis use the same numbering system as the \"car controls\" menu. 1 - 10 not 0 - 9");
            Settings.AddText(this, "\"Fully lowered / raised %\" means at this joystick % the in-game lever will be at -100% / 100%.");

            Settings.AddHeader(this, "Front hydraulic arm");
            _frontHydArmJoystick = new Settings("kekMetFrontHydArmJoystick", "Joystick: Error", 0,
                () => UpdateJoystickName(_frontHydArmJoystick, 7));
            _frontHydArmJoystick.Name = $"Joystick: {_joystickNames[Convert.ToInt32(_frontHydArmJoystick.Value)]}";
            Settings.AddSlider(this, _frontHydArmJoystick, 0, _joystickNames.Length - 1);
            Settings.AddSlider(this, _frontHydArmAxis, 1, 10, _axisNames);
            Settings.AddSlider(this, _frontHydArmLowered, -100, 100);
            Settings.AddSlider(this, _frontHydArmRaised, -100, 100);

            Settings.AddHeader(this, "Front hydraulic fork");
            _frontHydLoaderJoystick = new Settings("kekMetFrontHydLoaderJoystick", "Joystick: Error", 0,
                () => UpdateJoystickName(_frontHydLoaderJoystick, 16));
            _frontHydLoaderJoystick.Name = $"Joystick: {_joystickNames[Convert.ToInt32(_frontHydLoaderJoystick.Value)]}";
            Settings.AddSlider(this, _frontHydLoaderJoystick, 0, _joystickNames.Length - 1);
            Settings.AddSlider(this, _frontHydLoaderAxis, 1, 10, _axisNames);
            Settings.AddSlider(this, _frontHydLoaderLowered, -100, 100);
            Settings.AddSlider(this, _frontHydLoaderRaised, -100, 100);

            Settings.AddHeader(this, "Hand throttle");
            _throttleJoystick = new Settings("kekMetThrottleJoystick", "Joystick: Error", 0,
                () => UpdateJoystickName(_throttleJoystick, 25));
            _throttleJoystick.Name = $"Joystick: {_joystickNames[Convert.ToInt32(_throttleJoystick.Value)]}";
            Settings.AddSlider(this, _throttleJoystick, 0, _joystickNames.Length - 1);
            Settings.AddSlider(this, _throttleAxis, 1, 10, _axisNames);
            Settings.AddSlider(this, _throttleLowered, -100, 100);
            Settings.AddSlider(this, _throttleRaised, -100, 100);

            //Keybinds
            Keybind.AddHeader(this, "Front hydraulics arm");
            Keybind.Add(this, _frontHydArmKeybindFore);
            Keybind.Add(this, _frontHydArmKeybindAft);
            Keybind.AddHeader(this, "Front hydraulics fork");
            Keybind.Add(this, _frontHydLoaderKeybindFore);
            Keybind.Add(this, _frontHydLoaderKeybindAft);
            Keybind.AddHeader(this, "Hand throttle");
            Keybind.Add(this, _throttleKeybindFore);
            Keybind.Add(this, _throttleKeybindAft);
        }

        /// <summary>
        /// Update the joystick name without redrawing the entire settings menu. This allows for dragging the sliders all the way.
        /// This isn't nice. But works...
        /// </summary>
        /// <param name="setting">Setting of the joystick.</param>
        /// <param name="labelIndex">Label index of the modSettingsList child to update.</param>
        private void UpdateJoystickName(Settings setting, int labelIndex)
        {
            setting.Name = $"Joystick: {_joystickNames[Convert.ToInt32(setting.Value)]}";
            SettingsView sv = UnityEngine.Object.FindObjectOfType<SettingsView>();
            try
            {
                sv.modSettingsList.transform.GetChild(labelIndex).gameObject.GetComponent<Text>().text = setting.Name;
            }
            catch (Exception)
            {
                ModConsole.Error("Label index doesn't have a text component!");
                throw;
            }
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
