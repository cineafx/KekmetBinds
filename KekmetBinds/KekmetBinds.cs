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

        private PlayMakerFSM _frontHydArmFsm;
        private CapsuleCollider _frontHydArmCapsuleCollider;
        private float _frontHydArmDefaultCapsuleRadius;

        private PlayMakerFSM _frontHydLoaderFsm;
        private CapsuleCollider _frontHydLoaderCapsuleCollider;
        private float _frontHydLoaderDefaultCapsuleRadius;

        private static readonly float CapsuleNewRadius = 0.6f;


        private readonly Keybind _frontHydArmKeybindFore =
            new Keybind("frontHydArmFore", "Front loader arm forward (lower)", KeyCode.Keypad2);

        private readonly Keybind _frontHydArmKeybindAft =
            new Keybind("frontHydArmAft", "Front loader arm backward (raise)", KeyCode.Keypad5);

        private readonly Keybind _frontHydLoaderKeybindFore =
            new Keybind("frontHydLoaderFore", "Front loader fork forward (lower)", KeyCode.Keypad1);

        private readonly Keybind _frontHydLoaderKeybindAft =
            new Keybind("frontHydLoaderAft", "Front loader fork backward (raise)", KeyCode.Keypad4);

        public override void OnLoad()
        {
            // Called once, when mod is loading after game is fully loaded
            _playerCurrentVehicle = PlayMakerGlobals.Instance.Variables.FindFsmString("PlayerCurrentVehicle");
        }

        public override void ModSettings()
        {
            // All settings should be created here. 
            // DO NOT put anything else here that settings.
            Keybind.Add(this, _frontHydArmKeybindFore);
            Keybind.Add(this, _frontHydArmKeybindAft);
            Keybind.Add(this, _frontHydLoaderKeybindFore);
            Keybind.Add(this, _frontHydLoaderKeybindAft);
        }

        public override void Update()
        {
            // Update is called once per frame


            if (_playerCurrentVehicle.Value.Length == 0)
            {
                if (_playerLastVehicle == _playerCurrentVehicle.Value) return;

                if (_frontHydArmCapsuleCollider)
                {
                    _frontHydArmCapsuleCollider.radius = _frontHydArmDefaultCapsuleRadius;
                    _frontHydArmCapsuleCollider = null;
                }

                if (_frontHydLoaderCapsuleCollider)
                {
                    _frontHydLoaderCapsuleCollider.radius = _frontHydLoaderDefaultCapsuleRadius;
                    _frontHydLoaderCapsuleCollider = null;
                }

                _playerLastVehicle = _playerCurrentVehicle.Value;
                ModConsole.Print("New vehicle: None");
                return;
            }


            UpdateFsm();

            if (_frontHydArmFsm)
            {
                // Holding both buttons should do nothing
                if (_frontHydArmKeybindFore.GetKeybind() && _frontHydArmKeybindAft.GetKeybind() ||
                    _frontHydArmKeybindFore.GetKeybindUp() || _frontHydArmKeybindAft.GetKeybindUp())
                {
                    _frontHydArmCapsuleCollider.radius = _frontHydArmDefaultCapsuleRadius;
                    _frontHydArmFsm.SendEvent("FINISHED");
                }
                else if (_frontHydArmKeybindFore.GetKeybind())
                {
                    _frontHydArmCapsuleCollider.radius = CapsuleNewRadius;
                    _frontHydArmFsm.SendEvent("DECREASE");
                }
                else if (_frontHydArmKeybindAft.GetKeybind())
                {
                    _frontHydArmCapsuleCollider.radius = CapsuleNewRadius;
                    _frontHydArmFsm.SendEvent("INCREASE");
                }
            }

            if (_frontHydLoaderFsm)
            {
                // Holding both buttons should do nothing
                if (_frontHydLoaderKeybindFore.GetKeybind() && _frontHydLoaderKeybindAft.GetKeybind() ||
                    _frontHydLoaderKeybindFore.GetKeybindUp() || _frontHydLoaderKeybindAft.GetKeybindUp())
                {
                    _frontHydLoaderCapsuleCollider.radius = _frontHydLoaderDefaultCapsuleRadius;
                    _frontHydLoaderFsm.SendEvent("FINISHED");
                }
                else if (_frontHydLoaderKeybindFore.GetKeybind())
                {
                    _frontHydLoaderCapsuleCollider.radius = CapsuleNewRadius;
                    _frontHydLoaderFsm.SendEvent("DECREASE");
                }
                else if (_frontHydLoaderKeybindAft.GetKeybind())
                {
                    _frontHydLoaderCapsuleCollider.radius = CapsuleNewRadius;
                    _frontHydLoaderFsm.SendEvent("INCREASE");
                }
            }
        }

        private void UpdateFsm()
        {
            if (_playerLastVehicle == _playerCurrentVehicle.Value) return;
            _playerLastVehicle = _playerCurrentVehicle.Value;
            ModConsole.Print("New Vehicle: " + _playerLastVehicle);


            switch (_playerCurrentVehicle.Value)
            {
                default:
                {
                    _frontHydArmFsm = null;
                    _frontHydLoaderFsm = null;
                    break;
                }
                case "Kekmet":
                {
                    _frontHydArmFsm = GameObject.Find("KEKMET(350-400psi)").transform
                        .Find("Dashboard/FrontHydArm").gameObject.GetComponent<PlayMakerFSM>();
                    _frontHydLoaderFsm = GameObject.Find("KEKMET(350-400psi)").transform
                        .Find("Dashboard/FrontHydLoader").gameObject.GetComponent<PlayMakerFSM>();
                    break;
                }
            }

            if (_frontHydArmFsm != null)
            {
                _frontHydArmCapsuleCollider = _frontHydArmFsm.gameObject.GetComponent<CapsuleCollider>();
                _frontHydArmDefaultCapsuleRadius = _frontHydArmCapsuleCollider.radius;
            }

            if (_frontHydLoaderFsm != null)
            {
                _frontHydLoaderCapsuleCollider = _frontHydLoaderFsm.gameObject.GetComponent<CapsuleCollider>();
                _frontHydLoaderDefaultCapsuleRadius = _frontHydLoaderCapsuleCollider.radius;
            }
        }
    }
}
