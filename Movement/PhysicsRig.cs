using UnityEngine;

namespace BIMOS
{
    public class PhysicsRig : MonoBehaviour
    {
        public LocomotionSphere LocomotionSphere;

        public Rigidbody
            LocomotionSphereRigidbody,
            FenderRigidbody,
            PelvisRigidbody,
            HeadRigidbody,
            LeftHandRigidbody,
            RightHandRigidbody;

        public ConfigurableJoint
            FenderPelvisJoint,
            PelvisHeadJoint,
            PelvisHeadColliderJoint,
            LeftHandJoint,
            RightHandJoint;

        public float
            AirAcceleration = 3f,
            FenderPelvisOffset = 0.55f,
            RealFenderPelvisOffset = 0.55f; //The multiplier for air acceleration

        public enum JumpStates
        {
            NotJumping,
            Anticipation,
            PushingGround,
            Ascending,
            Descending
        }
        public JumpStates JumpState; //What state of a jump the player is in

        private void Awake()
        {
            int playerLayer = LayerMask.NameToLayer("BIMOSRig");
            Physics.IgnoreLayerCollision(playerLayer, playerLayer);

            SetLayerRecursive(gameObject, LayerMask.NameToLayer("BIMOSRig"));
        }

        private void SetLayerRecursive(GameObject gameObject, int layer)
        {
            gameObject.layer = layer;
            foreach (Transform child in gameObject.transform)
            {
                child.gameObject.layer = layer;

                Transform hasChildren = child.GetComponentInChildren<Transform>();
                if (!hasChildren)
                    continue;

                SetLayerRecursive(child.gameObject, layer);
            }
        }
    }
}