using UnityEngine;

namespace BIMOS
{
    public class Hand : MonoBehaviour
    {
        public HandPoseAnimator HandAnimator;
        public HandPose Default_handPose;
        public Grab CurrentGrab;
        public HandInputReader HandInputReader;
        public Transform PalmTransform;
        public PhysicsHand PhysicsHand;
        public Transform PhysicsHandTransform;
        public GrabHandler GrabHandler;
        public bool IsLeftHand;
        public Hand otherHand;
        public Collider PhysicsHandCollider;
        public GrabPoint CurrentGrabPoint { get; set; }
    }
}
