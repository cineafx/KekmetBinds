using System.Net.Configuration;
using UnityEngine;

namespace KekmetBinds
{
    public class LeverHandler
    {
        public LeverHandler(PlayMakerFSM fsm, float capsuleNewRadius, Vector3 leverOffset)
        {
            _fsm = fsm;
            _capsuleNewRadius = capsuleNewRadius;
            _leverOffset = leverOffset;
            
            _capsuleCollider = _fsm.gameObject.GetComponent<CapsuleCollider>();
            _defaultCapsuleRadius = _capsuleCollider.radius;
            _localPosColl = _capsuleCollider.transform.localPosition;

            _lever = _fsm.gameObject.transform.Find("Lever");
            _localPosLever = _lever.localPosition;
        }

        private bool _isInVehicle;
        public bool IsInVehicle
        {
            get => _isInVehicle;
            set
            {
                _isInVehicle = value;
                if (!_isInVehicle)
                {
                    _fsm.SendEvent("FINISHED");
                }
            }
        }

        private PlayMakerFSM _fsm;
        private readonly CapsuleCollider _capsuleCollider;
        private readonly Transform _lever;
        private readonly float _defaultCapsuleRadius;
        private readonly Vector3 _localPosColl;
        private readonly Vector3 _localPosLever;
        private readonly float _capsuleNewRadius;
        private readonly Vector3 _leverOffset;
        private Vector3 LeverOffsetDown => new Vector3(_leverOffset.x, _leverOffset.y, -_leverOffset.z);

        public void SetLeverHandler(string fsmEvent)
        {
            _capsuleCollider.radius = _capsuleNewRadius;
            _capsuleCollider.transform.localPosition = _localPosColl + _leverOffset;
            _lever.localPosition = _localPosLever + LeverOffsetDown;
            _fsm.SendEvent(fsmEvent);
        }

        public void ResetLeverHandler(bool stillInVehicle)
        {
            if (_capsuleCollider)
            {
                _capsuleCollider.radius = _defaultCapsuleRadius;
                _capsuleCollider.transform.localPosition = _localPosColl;
                _lever.localPosition = _localPosLever;
            }

            _fsm.SendEvent("FINISHED");
            if (!stillInVehicle)
                _fsm = null;
        }
    }
}
