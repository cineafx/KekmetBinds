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
        private PlayMakerFSM _frontHydArmFsm;
        private CapsuleCollider _frontHydArmCapsuleCollider;
        private Transform _frontHydArmLever;
        private float _frontHydArmDefaultCapsuleRadius;
        private Vector3 _frontHydArmLocalPosColl;
        private Vector3 _frontHydArmLocalPosLever;

        //Frontloader Loader (the fork thing)
        private PlayMakerFSM _frontHydLoaderFsm;
        private CapsuleCollider _frontHydLoaderCapsuleCollider;
        private Transform _frontHydLoaderLever;
        private float _frontHydLoaderDefaultCapsuleRadius;
        private Vector3 _frontHydLoaderLocalPosColl;
        private Vector3 _frontHydLoaderLocalPosLever;


        private static readonly float CapsuleNewRadius = 0.7f;
        private static readonly Vector3 LeverOffset = new Vector3(0.35f, 1.25f, 0.5f);
        private static readonly Vector3 LeverOffsetDown = new Vector3(LeverOffset.x, LeverOffset.y, -LeverOffset.z);


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
        /// Update is called once per frame
        /// </summary>
        public override void Update()
        {
            //Reset if player leaves vehicle
            if (_playerCurrentVehicle.Value.Length == 0)
            {
                if (_playerLastVehicle == _playerCurrentVehicle.Value) return;

                if (_frontHydArmCapsuleCollider) ResetFrontHydArm(false);
                if (_frontHydLoaderCapsuleCollider) ResetFrontHydLoader(false);

                _playerLastVehicle = _playerCurrentVehicle.Value;
                //ModConsole.Print("New vehicle: None");
                return;
            }

            UpdateFsm();

            if (_frontHydArmFsm)
            {
                // Holding both buttons should do nothing
                if (KeybindBothHoldOrEitherUp(_frontHydArmKeybindFore, _frontHydArmKeybindAft))
                    ResetFrontHydArm(true);
                else if (_frontHydArmKeybindFore.GetKeybind())
                    SetFrontHydArm("DECREASE");
                else if (_frontHydArmKeybindAft.GetKeybind())
                    SetFrontHydArm("INCREASE");
            }

            if (_frontHydLoaderFsm)
            {
                // Holding both buttons should do nothing
                if (KeybindBothHoldOrEitherUp(_frontHydLoaderKeybindFore, _frontHydLoaderKeybindAft))
                    ResetFrontHydLoader(true);
                else if (_frontHydLoaderKeybindFore.GetKeybind())
                    SetFrontHydLoader("DECREASE");
                else if (_frontHydLoaderKeybindAft.GetKeybind())
                    SetFrontHydLoader("INCREASE");
            }
        }

        /// <summary>
        /// Returns true if both Keybinds are held at the same time or either Keybind is got released this frames.
        /// Use case: Holding both buttons should do nothing. --> Reset the capsule colliders.
        /// </summary>
        /// <param name="kb1"></param>
        /// <param name="kb2"></param>
        /// <returns></returns>
        private static bool KeybindBothHoldOrEitherUp(Keybind kb1, Keybind kb2)
        {
            return kb1.GetKeybind() && kb2.GetKeybind() || kb1.GetKeybindUp() || kb2.GetKeybindUp();
        }

        private void SetFrontHydArm(string fsmEvent)
        {
            _frontHydArmCapsuleCollider.radius = CapsuleNewRadius;
            _frontHydArmCapsuleCollider.transform.localPosition = _frontHydArmLocalPosColl + LeverOffset;
            //_frontHydArmLever.localPosition = _frontHydArmLocalPosLever + LeverOffsetDown;
            _frontHydArmFsm.SendEvent(fsmEvent);
        }

        private void SetFrontHydLoader(string fsmEvent)
        {
            _frontHydLoaderCapsuleCollider.radius = CapsuleNewRadius;
            _frontHydLoaderCapsuleCollider.transform.localPosition = _frontHydLoaderLocalPosColl + LeverOffset;
            //_frontHydLoaderLever.localPosition = _frontHydLoaderLocalPosLever + LeverOffsetDown;
            _frontHydLoaderFsm.SendEvent(fsmEvent);
        }

        private void ResetFrontHydArm(bool stillInVehicle)
        {
            _frontHydArmCapsuleCollider.radius = _frontHydArmDefaultCapsuleRadius;
            _frontHydArmCapsuleCollider.transform.localPosition = _frontHydArmLocalPosColl;
            _frontHydArmLever.localPosition = _frontHydArmLocalPosLever;
            if (stillInVehicle)
                _frontHydArmFsm.SendEvent("FINISHED");
            else
                _frontHydArmCapsuleCollider = null;
        }

        private void ResetFrontHydLoader(bool stillInVehicle)
        {
            _frontHydLoaderCapsuleCollider.radius = _frontHydLoaderDefaultCapsuleRadius;
            _frontHydLoaderCapsuleCollider.transform.localPosition = _frontHydLoaderLocalPosColl;
            _frontHydLoaderLever.localPosition = _frontHydLoaderLocalPosLever;
            if (stillInVehicle)
                _frontHydLoaderFsm.SendEvent("FINISHED");
            else
                _frontHydLoaderCapsuleCollider = null;
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
                _frontHydArmLocalPosColl = _frontHydArmCapsuleCollider.transform.localPosition;

                _frontHydArmLever = _frontHydArmFsm.gameObject.transform.Find("Lever");
                _frontHydArmLocalPosLever = _frontHydArmLever.localPosition;
            }

            if (_frontHydLoaderFsm != null)
            {
                _frontHydLoaderCapsuleCollider = _frontHydLoaderFsm.gameObject.GetComponent<CapsuleCollider>();
                _frontHydLoaderDefaultCapsuleRadius = _frontHydLoaderCapsuleCollider.radius;
                _frontHydLoaderLocalPosColl = _frontHydLoaderCapsuleCollider.transform.localPosition;

                _frontHydLoaderLever = _frontHydLoaderFsm.gameObject.transform.Find("Lever");
                _frontHydLoaderLocalPosLever = _frontHydLoaderLever.localPosition;
            }
        }
    }
}
