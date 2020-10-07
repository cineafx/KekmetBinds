using System;
using System.Collections.Generic;
using System.Linq;
using HutongGames.PlayMaker;
using MSCLoader;
using UnityEngine;

namespace KekmetBinds
{
    public class KekmetBinds : Mod
    {
        public override string ID => "KekmetBinds"; //Your mod ID (unique)
        public override string Name => "Kekmet Binds"; //You mod name
        public override string Author => "icdb"; //Your Username
        public override string Version => "1.0"; //Version

        // Set this to true if you will be load custom assets from Assets folder.
        // This will create subfolder in Assets folder for your mod.
        public override bool UseAssetsFolder => false;


        //Used to tell if the player has entered / exited a vehicle.
        private FsmString _currentVic;
        private string _lastVic;


        //All LeverHandlers checked every frame.
        private readonly List<LeverHandler> _leverHandlers = new List<LeverHandler>();

        private readonly string[] _joystickNames =
        {
            "None", "Not connected", "Not connected", "Not connected", "Not connected", "Not connected",
            "Not connected", "Not connected", "Not connected"
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
        private readonly Settings _frontHydArmJoystick = new Settings("kekMetFrontHydArmJoystick", "Joystick", 0);
        private readonly Settings _frontHydArmAxis = new Settings("kekMetFrontHydArmAxis", "Axis", 1);
        private readonly Settings _frontHydArmLowered = new Settings("kekMetFrontHydArmLowered", "Fully lowered %", -100);
        private readonly Settings _frontHydArmRaised = new Settings("kekMetFrontHydArmRaised", "Fully raised %", 100);

        private readonly Settings _frontHydLoaderJoystick = new Settings("kekMetFrontHydLoaderJoystick", "Joystick", 0);
        private readonly Settings _frontHydLoaderAxis = new Settings("kekMetFrontHydLoaderAxis", "Axis", 1);
        private readonly Settings _frontHydLoaderLowered = new Settings("kekMetFrontHydLoaderLowered", "Fully lowered %", -100);
        private readonly Settings _frontHydLoaderRaised = new Settings("kekMetFrontHydLoaderRaised", "Fully raised %", 100);

        private readonly Settings _throttleJoystick = new Settings("kekMetThrottleJoystick", "Joystick", 0);
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
            Settings.AddText(this, "Axis use the same numbering system as the \"car controls\" menu. 1 - 10 not 0 - 9");
            Settings.AddText(this, "");

            Settings.AddHeader(this, "Front hydraulic arm");
            Settings.AddSlider(this, _frontHydArmJoystick, 0, _joystickNames.Length - 1, _joystickNames);
            Settings.AddSlider(this, _frontHydArmAxis, 1, 10, _axisNames);
            Settings.AddSlider(this, _frontHydArmLowered, -100, 100);
            Settings.AddSlider(this, _frontHydArmRaised, -100, 100);

            Settings.AddHeader(this, "Front hydraulic fork");
            Settings.AddSlider(this, _frontHydLoaderJoystick, 0, _joystickNames.Length - 1, _joystickNames);
            Settings.AddSlider(this, _frontHydLoaderAxis, 1, 10, _axisNames);
            Settings.AddSlider(this, _frontHydLoaderLowered, -100, 100);
            Settings.AddSlider(this, _frontHydLoaderRaised, -100, 100);

            Settings.AddHeader(this, "Hand throttle");
            Settings.AddSlider(this, _throttleJoystick, 0, _joystickNames.Length - 1, _joystickNames);
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
        /// Called once, when mod is loading after game is fully loaded.
        /// </summary>
        public override void OnLoad()
        {
            _currentVic = PlayMakerGlobals.Instance.Variables.FindFsmString("PlayerCurrentVehicle");

            _leverHandlers.Add(new LeverHandler(
                GameObject.Find("KEKMET(350-400psi)").transform.Find("Dashboard/FrontHydArm")
                    .gameObject.GetComponent<PlayMakerFSM>(),
                _frontHydArmKeybindFore,
                _frontHydArmKeybindAft,
                _frontHydArmJoystick,
                _frontHydArmAxis,
                _frontHydArmLowered,
                _frontHydArmRaised
            ));
            _leverHandlers.Add(new LeverHandler(
                GameObject.Find("KEKMET(350-400psi)").transform.Find("Dashboard/FrontHydLoader")
                    .gameObject.GetComponent<PlayMakerFSM>(),
                _frontHydLoaderKeybindFore,
                _frontHydLoaderKeybindAft,
                _frontHydLoaderJoystick,
                _frontHydLoaderAxis,
                _frontHydLoaderLowered,
                _frontHydLoaderRaised
            ));
            _leverHandlers.Add(new LeverHandler(
                GameObject.Find("KEKMET(350-400psi)").transform.Find("LOD/Dashboard/Throttle")
                    .gameObject.GetComponent<PlayMakerFSM>(),
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
