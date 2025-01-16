using UnityEngine;

namespace BIMOS
{
    [AddComponentMenu("BIMOS/Grabs/Grab (Snap)")]
    public class SnapGrab : Grab
    {
        public override float CalculateRank(Transform handTransform)
        {
            return base.CalculateRank(handTransform) * 3f;
        }

       public override void AlignHand(Hand hand)
    {
        // Alinear posición de la mano al punto de agarre
        hand.PhysicsHandTransform.position = primaryGrabOffset.position;

        // Alinear rotación de la mano al punto de agarre
        hand.PhysicsHandTransform.rotation = primaryGrabOffset.rotation;  }

       

        private void FixedUpdate()
        {
           
        }
    }
}
