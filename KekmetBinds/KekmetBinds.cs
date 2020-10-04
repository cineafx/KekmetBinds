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
        private readonly LeverHandler _frontHydArm = new LeverHandler(0.7f, new Vector3(0.35f, 1.25f, 0.5f));

        //Frontloader Loader (the fork thing)
        private readonly LeverHandler _frontHydLoader = new LeverHandler(0.7f, new Vector3(0.35f, 1.25f, 0.5f));

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
        /// Called once, when mod is loading after game is fully loaded
        /// </summary>
        public override void OnLoad()
        {
            _playerCurrentVehicle = PlayMakerGlobals.Instance.Variables.FindFsmString("PlayerCurrentVehicle");
            _leverHandlers = new List<LeverHandler> {_frontHydArm, _frontHydLoader};
        }

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
        /// Update is called once per frame.
        /// </summary>
        public override void Update()
        {
            //Reset if player leaves vehicle
            if (_playerCurrentVehicle.Value.Length == 0)
            {
                if (_playerLastVehicle == _playerCurrentVehicle.Value) return;

                _playerLastVehicle = _playerCurrentVehicle.Value;
                //ModConsole.Print("New vehicle: None");
                return;
            }

            UpdateFsm();

            if (_frontHydArm.Fsm)
            {
                // Holding both buttons should do nothing
                if (KeybindBothHoldOrEitherUp(_frontHydArmKeybindFore, _frontHydArmKeybindAft))
                    ResetLeverHandler(_frontHydArm, true);
                else if (_frontHydArmKeybindFore.GetKeybind())
                    SetLeverHandler(_frontHydArm, "DECREASE");
                else if (_frontHydArmKeybindAft.GetKeybind())
                    SetLeverHandler(_frontHydArm, "INCREASE");
            }

            if (_frontHydLoader.Fsm)
            {
                // Holding both buttons should do nothing
                if (KeybindBothHoldOrEitherUp(_frontHydLoaderKeybindFore, _frontHydLoaderKeybindAft))
                    ResetLeverHandler(_frontHydLoader, true);
                else if (_frontHydLoaderKeybindFore.GetKeybind())
                    SetLeverHandler(_frontHydLoader, "DECREASE");
                else if (_frontHydLoaderKeybindAft.GetKeybind())
                    SetLeverHandler(_frontHydLoader, "INCREASE");
            }
        }

        /// <summary>
        /// Returns true if both Keybinds are held at the same time or either Keybind got released this frames.
        /// Use case: Holding both buttons should do nothing. --> Reset the capsule colliders.
        /// </summary>
        /// <param name="kb1"></param>
        /// <param name="kb2"></param>
        /// <returns></returns>
        private static bool KeybindBothHoldOrEitherUp(Keybind kb1, Keybind kb2)
        {
            return kb1.GetKeybind() && kb2.GetKeybind() || kb1.GetKeybindUp() || kb2.GetKeybindUp();
        }

        private static void SetLeverHandler(LeverHandler lh, string fsmEvent)
        {
            lh.CapsuleCollider.radius = lh.CapsuleNewRadius;
            lh.CapsuleCollider.transform.localPosition = lh.LocalPosColl + lh.LeverOffset;
            lh.Lever.localPosition = lh.LocalPosLever + lh.LeverOffsetDown;
            lh.Fsm.SendEvent(fsmEvent);
        }

        private static void ResetLeverHandler(LeverHandler lh, bool stillInVehicle)
        {
            if (lh.CapsuleCollider)
            {
                lh.CapsuleCollider.radius = lh.DefaultCapsuleRadius;
                lh.CapsuleCollider.transform.localPosition = lh.LocalPosColl;
                lh.Lever.localPosition = lh.LocalPosLever;
            }

            lh.Fsm.SendEvent("FINISHED");
            if (!stillInVehicle)
                lh.Fsm = null;
        }

        /// <summary>
        /// Check if the current vehicle has changed and update PlayMakerFSM, colliders, ... if needed.
        /// </summary>
        private void UpdateFsm()
        {
            if (_playerLastVehicle == _playerCurrentVehicle.Value) return;
            _playerLastVehicle = _playerCurrentVehicle.Value;
            //ModConsole.Print("New Vehicle: " + _playerLastVehicle);

            switch (_playerCurrentVehicle.Value)
            {
                default:
                {
                    ResetLeverHandler(_frontHydArm, false);
                    ResetLeverHandler(_frontHydLoader, false);
                    break;
                }
                case "Kekmet":
                {
                    _frontHydArm.Fsm = GameObject.Find("KEKMET(350-400psi)").transform
                        .Find("Dashboard/FrontHydArm").gameObject.GetComponent<PlayMakerFSM>();
                    _frontHydLoader.Fsm = GameObject.Find("KEKMET(350-400psi)").transform
                        .Find("Dashboard/FrontHydLoader").gameObject.GetComponent<PlayMakerFSM>();
                    break;
                }
            }

            //Array.ForEach(_leverHandlers, _leverHandler => _leverHandler.Update());

            _leverHandlers.ForEach(leverHandler => leverHandler.Update());
        }
    }
}
