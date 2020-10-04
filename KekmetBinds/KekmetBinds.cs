using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        private FsmString _playerCurrentVehicle;
        private string _playerLastVehicle;

        //Frontloader Arm
        private LeverHandler _frontHydArm;

        //Frontloader Loader (the fork thing)
        private LeverHandler _frontHydLoader;

        //All LeverHandlers that use the default thing in UpdateFsm()
        private List<LeverHandler> _leverHandlers;


        private readonly Keybind _frontHydArmKeybindFore =
            new Keybind("frontHydArmFore", "Front loader arm forward (lower)", KeyCode.Keypad2);

        private readonly Keybind _frontHydArmKeybindAft =
            new Keybind("frontHydArmAft", "Front loader arm backward (raise)", KeyCode.Keypad5);

        private readonly Keybind _frontHydLoaderKeybindFore =
            new Keybind("frontHydLoaderFore", "Front loader fork forward (lower)", KeyCode.Keypad1);

        private readonly Keybind _frontHydLoaderKeybindAft =
            new Keybind("frontHydLoaderAft", "Front loader fork backward (raise)", KeyCode.Keypad4);

        /// <summary>
        /// All settings should be created here. 
        /// DO NOT put anything else here that settings.
        /// </summary>
        public override void ModSettings()
        {
            Keybind.Add(this, _frontHydArmKeybindFore);
            Keybind.Add(this, _frontHydArmKeybindAft);
            Keybind.Add(this, _frontHydLoaderKeybindFore);
            Keybind.Add(this, _frontHydLoaderKeybindAft);
        }

        /// <summary>
        /// Called once, when mod is loading after game is fully loaded
        /// </summary>
        public override void OnLoad()
        {
            _playerCurrentVehicle = PlayMakerGlobals.Instance.Variables.FindFsmString("PlayerCurrentVehicle");

            _frontHydArm = new LeverHandler(
                GameObject.Find("KEKMET(350-400psi)").transform.Find("Dashboard/FrontHydArm").gameObject
                    .GetComponent<PlayMakerFSM>(),
                0.7f,
                new Vector3(0.35f, 1.25f, 0.5f)
            );
            _frontHydLoader = new LeverHandler(
                GameObject.Find("KEKMET(350-400psi)").transform.Find("Dashboard/FrontHydLoader").gameObject
                    .GetComponent<PlayMakerFSM>(),
                0.7f,
                new Vector3(0.35f, 1.25f, 0.5f)
            );

            _leverHandlers = new List<LeverHandler> {_frontHydArm, _frontHydLoader};
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

                _leverHandlers.ForEach(leverHandler => leverHandler.IsInVehicle = false);

                _playerLastVehicle = _playerCurrentVehicle.Value;
                return;
            }

            // Check if the current vehicle has changed and tell the handlers
            if (_playerLastVehicle != _playerCurrentVehicle.Value)
            {
                _playerLastVehicle = _playerCurrentVehicle.Value;
                _leverHandlers.ForEach(leverHandler =>
                    leverHandler.IsInVehicle = _playerCurrentVehicle.Value == "Kekmet");
            }

            _frontHydArm.HandleKeyBinds(_frontHydArmKeybindFore, _frontHydArmKeybindAft);
            _frontHydLoader.HandleKeyBinds(_frontHydLoaderKeybindFore, _frontHydLoaderKeybindAft);
        }
    }
}
