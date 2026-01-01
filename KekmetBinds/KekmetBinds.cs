using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using HutongGames.PlayMaker;
using KekmetBinds.LeverHandling;
using MSCLoader;
using UnityEngine;

namespace KekmetBinds
{
    [SuppressMessage("ReSharper", "UnusedType.Global")]
    public class KekmetBinds : Mod
    {
        public override string ID => "KekmetBinds";
        public override string Name => "Kekmet Binds";
        public override string Author => "icdb / cineafx";
        public override string Version => "3.0";
        public override string Description => "Adds extra binds to the Kekment Tractor.";
        public override byte[] Icon => Properties.Resources.icon;
        

        //Used to tell if the player has entered / exited a vehicle.
        private FsmString _currentVic;
        private string _lastVic;
        private Transform _playerTransform;
        private Transform _kekmetTransform;

        //All LeverHandlers checked every frame.
        private readonly List<ILeverHandler> _leverHandlers = new List<ILeverHandler>();

        private readonly string[] _joystickNames =
        {
            "None", "Not connected", "Not connected", "Not connected", "Not connected",
            "Not connected", "Not connected", "Not connected", "Not connected"
        };

        public override void ModSetup()
        {
            base.ModSetup();
            
            SetupFunction(Setup.ModSettings, ModSettings);
            SetupFunction(Setup.OnLoad, OnLoad);
            SetupFunction(Setup.Update, Update);
        }

        // Keybinds
        private SettingsKeybind _frontHydArmKeybindFore;
        private SettingsKeybind _frontHydArmKeybindAft;

        private SettingsKeybind _frontHydLoaderKeybindFore;
        private SettingsKeybind _frontHydLoaderKeybindAft; 

        private SettingsKeybind _throttleKeybindFore;   
        private SettingsKeybind _throttleKeybindAft;        

        //Settings
        private SettingsSliderInt _allowOutside;
        
        private SettingsSliderInt _frontHydArmJoystick;
        private SettingsSliderInt _frontHydArmAxis;
        private SettingsSliderInt _frontHydArmLowered;
        private SettingsSliderInt _frontHydArmRaised;

        private SettingsSliderInt _frontHydLoaderJoystick;
        private SettingsSliderInt _frontHydLoaderAxis;
        private SettingsSliderInt _frontHydLoaderLowered;
        private SettingsSliderInt _frontHydLoaderRaised;

        private SettingsSliderInt _throttleJoystick;
        private SettingsSliderInt _throttleAxis;
        private SettingsSliderInt _throttleLowered;
        private SettingsSliderInt _throttleRaised;


        private void ModSettings()
        {
            //Prep
            Array.Copy(Input.GetJoystickNames(), 0, _joystickNames, 1, Input.GetJoystickNames().Length);

            //Settings
            Settings.AddText("Max distance from Kekmet");
            _allowOutside = Settings.AddSlider("kekMetAllowOutside", "(0 = player needs to be in \"driving mode\")", 0, 50);

            Settings.CreateGroup();
            Settings.AddHeader("Joystick settings:");
            Settings.AddText("Axis use the same numbering system as the \"car controls\" menu. 1 - 10 not 0 - 9");
            Settings.AddText("\"Fully lowered / raised %\" means at this joystick % the in-game lever will be at -100% / 100%.");

            Settings.AddHeader("Front hydraulic arm");
            _frontHydArmJoystick = Settings.AddSlider("kekMetFrontHydArmJoystick", "Joystick", 0, _joystickNames.Length - 1, 0, null, _joystickNames);
            _frontHydArmAxis = Settings.AddSlider("kekMetFrontHydArmAxis", "Axis", 1, 10, 1);
            _frontHydArmLowered = Settings.AddSlider("kekMetFrontHydArmLowered", "Fully lowered %", -100, 100, -100);
            _frontHydArmRaised = Settings.AddSlider("kekMetFrontHydArmRaised", "Fully raised %", -100, 100, 100);

            Settings.AddHeader("Front hydraulic fork");
            _frontHydLoaderJoystick = Settings.AddSlider("kekMetFrontHydLoaderJoystick", "Joystick", 0, _joystickNames.Length - 1, 0, null, _joystickNames);
            _frontHydLoaderAxis = Settings.AddSlider("kekMetFrontHydLoaderAxis", "Axis", 1, 10, 1);
            _frontHydLoaderLowered = Settings.AddSlider("kekMetFrontHydLoaderLowered", "Fully lowered %", -100, 100, -100);
            _frontHydLoaderRaised = Settings.AddSlider("kekMetFrontHydLoaderRaised", "Fully raised %", -100, 100, 100);
            
            Settings.AddHeader("Hand throttle");
            _throttleJoystick = Settings.AddSlider("kekMetFrontHydLoaderJoystick", "Joystick", 0, _joystickNames.Length - 1, 0, null, _joystickNames);
            _throttleAxis = Settings.AddSlider("kekMetFrontHydLoaderAxis", "Axis", 1, 10, 1);
            _throttleLowered = Settings.AddSlider("kekMetFrontHydLoaderLowered", "Fully lowered %", -100, 100, -100);
            _throttleRaised = Settings.AddSlider("kekMetFrontHydLoaderRaised", "Fully raised %", -100, 100, 100);
            
            //modSettings.AddSpacer(20);
            
            //Keybinds
            Keybind.AddHeader("Front hydraulic arm");
            _frontHydArmKeybindFore = Keybind.Add("kekMetFrontHydArmFore", "forward (lower)", KeyCode.Keypad2);
            _frontHydArmKeybindAft = Keybind.Add("kekMetFrontHydArmAft", "backward (raise)", KeyCode.Keypad5);
            Keybind.AddHeader("Front hydraulic fork");
            _frontHydLoaderKeybindFore = Keybind.Add("kekMetFrontHydLoaderFore", "forward (lower)", KeyCode.Keypad1);
            _frontHydLoaderKeybindAft = Keybind.Add("kekMetFrontHydLoaderAft", "backward (raise)", KeyCode.Keypad4);
            Keybind.AddHeader("Hand throttle");
            _throttleKeybindFore = Keybind.Add("kekMetThrottleFore", "forward (lower)", KeyCode.Keypad3);
            _throttleKeybindAft = Keybind.Add("kekMetThrottleAft", "backward (raise)", KeyCode.Keypad6);
        }

        private void OnLoad()
        {
            _currentVic = PlayMakerGlobals.Instance.Variables.FindFsmString("PlayerCurrentVehicle");
            _playerTransform = GameObject.Find("PLAYER").transform;
            _kekmetTransform = GameObject.Find("KEKMET(350-400psi)").transform;

            _leverHandlers.Add(new LeverHandlerNewHydraulics(
                _kekmetTransform.Find("Dashboard/NewHydraulics/FrontHydArm").gameObject.GetComponent<PlayMakerFSM>(),
                _frontHydArmKeybindFore,
                _frontHydArmKeybindAft,
                _frontHydArmJoystick,
                _frontHydArmAxis,
                _frontHydArmLowered,
                _frontHydArmRaised
            ));
            _leverHandlers.Add(new LeverHandlerNewHydraulics(
                _kekmetTransform.Find("Dashboard/NewHydraulics/FrontHydLoader").gameObject.GetComponent<PlayMakerFSM>(),
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

        private void Update()
        {
            int allowOutsideDist = _allowOutside.GetValue();
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
