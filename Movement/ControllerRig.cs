using UnityEngine;

namespace BIMOS
{
    public class ControllerRig : MonoBehaviour
    {
        [Header("Transforms")]
        public Transform CameraOffsetTransform;
        public Transform CameraTransform;
        public Transform MenuCameraTransform;
        public Transform LeftControllerTransform;
        public Transform RightControllerTransform;
        public Transform FloorOffsetTransform;

        private void Awake()
        {
            CameraTransform.GetComponent<Camera>().cullingMask = ~LayerMask.GetMask("BIMOSMenu");
            MenuCameraTransform.GetComponent<Camera>().cullingMask = LayerMask.GetMask("BIMOSMenu");
        }
    }
}
