using UnityEngine;

namespace KekmetBinds
{
    public class LeverHandler
    {
        public LeverHandler(float capsuleNewRadius, Vector3 leverOffset)
        {
            CapsuleNewRadius = capsuleNewRadius;
            LeverOffset = leverOffset;
        }

        public PlayMakerFSM Fsm;
        public CapsuleCollider CapsuleCollider;
        public Transform Lever;
        public float DefaultCapsuleRadius;
        public Vector3 LocalPosColl;
        public Vector3 LocalPosLever;
        public readonly float CapsuleNewRadius;
        public readonly Vector3 LeverOffset;
        public Vector3 LeverOffsetDown => new Vector3(LeverOffset.x, LeverOffset.y, -LeverOffset.z);

        public void Update()
        {
            if (Fsm == null) return;

            CapsuleCollider = Fsm.gameObject.GetComponent<CapsuleCollider>();
            DefaultCapsuleRadius = CapsuleCollider.radius;
            LocalPosColl = CapsuleCollider.transform.localPosition;

            Lever = Fsm.gameObject.transform.Find("Lever");
            LocalPosLever = Lever.localPosition;
        }
    }
}
