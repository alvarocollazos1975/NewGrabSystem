using UnityEngine;

namespace BIMOS
{
    [RequireComponent(typeof(Rigidbody))]
    public class PhysicsHand : MonoBehaviour
    {
        [SerializeField]
        public Transform Target, Controller;

        [SerializeField]
        private ConfigurableJoint handJoint;

        [SerializeField]
        public Vector3 TargetOffsetPosition;
        [SerializeField]
        public Quaternion TargetOffsetRotation;

        private Rigidbody handRigidbody;

        private void Start()
        {
            handRigidbody = GetComponent<Rigidbody>();
            handRigidbody.solverIterations = 60; // Puedes probar valores mayores
            handRigidbody.solverVelocityIterations = 20; // Ajustar según la respuesta



            ConfigureHandJoint();
        }

        private void FixedUpdate()
        {
            SynchronizeHandWithController(handJoint, Controller);
        }

        private void SynchronizeHandWithController(ConfigurableJoint handJoint, Transform controller)
        {
            // Set target position with offsets
            handJoint.targetPosition = TargetOffsetPosition;
            
            // Calculate the target rotation in the correct reference frame
            Quaternion targetRotation = Quaternion.Inverse(controller.rotation) * (controller.rotation * TargetOffsetRotation);
            targetRotation = targetRotation.normalized;

            // Set target rotation
            handJoint.targetRotation = targetRotation;

            // Optional: Adjust the joint's target velocity to reduce lag
            Vector3 velocity = (controller.position - handJoint.transform.position) / Time.fixedDeltaTime;
            Vector3 angularVelocity = (controller.rotation.eulerAngles - handJoint.transform.rotation.eulerAngles) / Time.fixedDeltaTime;

            handRigidbody.linearVelocity  = velocity;
            handRigidbody.angularVelocity = angularVelocity;
        }

        // private void SynchronizeHandWithController(ConfigurableJoint handJoint, Transform controller)
        // {
        //     // Calcula la posición objetivo aplicando el offset
        //     Vector3 targetPosition = controller.TransformPoint(TargetOffsetPosition);
        //     handJoint.targetPosition = transform.InverseTransformPoint(targetPosition);

        //     // Calcula la rotación objetivo aplicando el offset
        //     Quaternion targetRotation = controller.rotation * TargetOffsetRotation;
        //     handJoint.targetRotation = Quaternion.Inverse(targetRotation);

        //     // Elimina el cálculo manual de velocidades angulares
        //     handRigidbody.linearVelocity = Vector3.zero;
        //     handRigidbody.angularVelocity = Vector3.zero;
        // }


        private void ConfigureHandJoint()
        {
            // Set connected body to the controller's Rigidbody
            handJoint.connectedBody = Controller.GetComponent<Rigidbody>();
            
            // Free motion in all axes
            handJoint.angularXMotion = ConfigurableJointMotion.Free;
            handJoint.angularYMotion = ConfigurableJointMotion.Free;
            handJoint.angularZMotion = ConfigurableJointMotion.Free;
            handJoint.breakForce=Mathf.Infinity;
            handJoint.breakTorque=Mathf.Infinity;

            // Set drive mode and configure drives
            handJoint.rotationDriveMode = RotationDriveMode.Slerp;

            JointDrive rotationDrive = new JointDrive
            {
                positionSpring = 1000f, // Adjust as needed
                positionDamper = 100f, // Adjust as needed
                maximumForce = Mathf.Infinity
            };
            handJoint.slerpDrive = rotationDrive;
        }
// private void ConfigureHandJoint()
// {
//     handJoint.angularXMotion = ConfigurableJointMotion.Limited;
//     handJoint.angularYMotion = ConfigurableJointMotion.Limited;
//     handJoint.angularZMotion = ConfigurableJointMotion.Limited;

//     SoftJointLimit limit = new SoftJointLimit
//     {
//         limit = 45f // Limita la rotación a 45 grados en cualquier eje
//     };
//     handJoint.highAngularXLimit = limit;
//     handJoint.lowAngularXLimit = limit;
//     handJoint.angularYLimit = limit;
//     handJoint.angularZLimit = limit;

//     // Configura los drives de rotación
//     JointDrive rotationDrive = new JointDrive
//     {
//         positionSpring = 1500f,
//         positionDamper = 200f,
//         maximumForce = Mathf.Infinity
//     };
//     handJoint.slerpDrive = rotationDrive;
// }


        public void ResetOffsets()
        {
            TargetOffsetPosition = Vector3.zero;
            TargetOffsetRotation = Quaternion.identity;
        }


        private void OnDrawGizmos()
        {
    //         Gizmos.color = Color.yellow;
    //         Gizmos.DrawWireSphere(Target.position, 0.02f);

    //         Gizmos.color = Color.blue;
    //         Gizmos.DrawWireSphere(transform.position, 0.02f);

    //         if (Controller != null)
    // {
    //     Gizmos.color = Color.red;
    //     Gizmos.DrawLine(transform.position, transform.position + transform.right * 0.1f); // Eje X de la mano
    //     Gizmos.color = Color.green;
    //     Gizmos.DrawLine(transform.position, transform.position + transform.up * 0.1f);   // Eje Y de la mano
    //     Gizmos.color = Color.blue;
    //     Gizmos.DrawLine(transform.position, transform.position + transform.forward * 0.1f); // Eje Z de la mano

    //     Gizmos.color = Color.yellow;
    //     Gizmos.DrawLine(Controller.position, Controller.position + Controller.forward * 0.1f); // Eje Z del controlador
    // }
        }
    }
}
