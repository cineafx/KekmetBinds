using System.Collections.Generic;
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
        private FsmString _playerCurrentVehicle;
        private string _playerLastVehicle;


        //All LeverHandlers checked every frame.
        private List<LeverHandler> _leverHandlers;


        // Keybinds
        private readonly Keybind _frontHydArmKeybindFore =
            new Keybind("kekMetFrontHydArmFore", "forward (lower)", KeyCode.Keypad2);

        private readonly Keybind _frontHydArmKeybindAft =
            new Keybind("kekMetFrontHydArmAft", "backward (raise)", KeyCode.Keypad5);

        private readonly Keybind _frontHydLoaderKeybindFore =
            new Keybind("kekMetFrontHydLoaderFore", "forward (lower)", KeyCode.Keypad1);

        private readonly Keybind _frontHydLoaderKeybindAft =
            new Keybind("kekMetFrontHydLoaderAft", "backward (raise)", KeyCode.Keypad4);

        private readonly Keybind _throttleKeybindFore =
            new Keybind("kekMetThrottleFore", "forward (lower)", KeyCode.Keypad3);

        private readonly Keybind _throttleKeybindAft =
            new Keybind("kekMetThrottleAft", "backward (raise)", KeyCode.Keypad6);

        /// <summary>
        /// All settings should be created here. 
        /// DO NOT put anything else here that settings.
        /// </summary>
        public override void ModSettings()
        {
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
        /// Called once, when mod is loading after game is fully loaded
        /// </summary>
        public override void OnLoad()
        {
            _playerCurrentVehicle = PlayMakerGlobals.Instance.Variables.FindFsmString("PlayerCurrentVehicle");

            _leverHandlers = new List<LeverHandler>
            {
                //Frontloader Arm
                new LeverHandler(
                    GameObject.Find("KEKMET(350-400psi)").transform.Find("Dashboard/FrontHydArm")
                        .gameObject.GetComponent<PlayMakerFSM>(),
                    _frontHydArmKeybindFore,
                    _frontHydArmKeybindAft
                ),
                //Frontloader Loader (the fork thing)
                new LeverHandler(
                    GameObject.Find("KEKMET(350-400psi)").transform.Find("Dashboard/FrontHydLoader")
                        .gameObject.GetComponent<PlayMakerFSM>(),
                    _frontHydLoaderKeybindFore,
                    _frontHydLoaderKeybindAft
                ),
                //Handthrottle
                new LeverHandler(
                    GameObject.Find("KEKMET(350-400psi)").transform.Find("LOD/Dashboard/Throttle")
                        .gameObject.GetComponent<PlayMakerFSM>(),
                    _throttleKeybindFore,
                    _throttleKeybindAft
                )
            };
        }

        /// <summary>
        /// Update is called once per frame.
        /// </summary>
        public override void Update()
        {
            //Reset if player leaves vehicle
            if (_playerCurrentVehicle.Value.Length == 0)
            {
                if (_playerLastVehicle == _playerCurrentVehicle.Value) return;

                _playerLastVehicle = _playerCurrentVehicle.Value;
                _leverHandlers.ForEach(leverHandler => leverHandler.IsInVehicle = false);
                return;
            }

            // Check if the current vehicle has changed and tell the handlers
            if (_playerLastVehicle != _playerCurrentVehicle.Value)
            {
                _playerLastVehicle = _playerCurrentVehicle.Value;
                _leverHandlers.ForEach(leverHandler =>
                    leverHandler.IsInVehicle = _playerCurrentVehicle.Value == "Kekmet");
            }

            _leverHandlers.ForEach(leverHandler =>
                leverHandler.HandleKeyBinds());
        }
    }
}
