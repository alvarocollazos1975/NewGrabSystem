using UnityEngine;

namespace BIMOS
{
    public class GrabHandler : MonoBehaviour
    {
        [HideInInspector]
        public Hand _hand;

        [SerializeField]
        private Transform _grabBounds;

        [SerializeField]
        private HandPose _hoverHandPose, _defaultGrabHandPose;

        [SerializeField]
        private AudioClip[] _grabSounds, _releaseSounds;

        public Grab _chosenGrab;
        private AudioSource _audioSource;

        // Configuración del Gizmo
        [Header("Gizmo Settings")]
        [SerializeField]
        private GameObject grabPointPrefab; // Prefab del círculo
        private GameObject grabPointInstance; // Instancia del círculo

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
        }

        private void Update()
        {
            if (_hand.CurrentGrab) // Si la mano está sosteniendo algo
            {
                if (grabPointInstance != null)
                {
                    Destroy(grabPointInstance);
                }
                return;
            }

            _chosenGrab = GetChosenGrab(); // Obtén el mejor agarre cercano

            if (_chosenGrab != null)
            {
                if (grabPointInstance == null)
                {
                    grabPointInstance = Instantiate(grabPointPrefab);
                }
                grabPointInstance.transform.position = _chosenGrab.GrabPositionOffset;
                grabPointInstance.transform.rotation = _chosenGrab.transform.rotation;

               // Debug.Log($"Grab detectado: {_chosenGrab.name}");
            }
            else
            {
                if (grabPointInstance != null)
                {
                    Destroy(grabPointInstance);
                }
            }

            bool grabInRange = _chosenGrab && _hand.HandInputReader.Grip < 0.5f;
            _hand.HandAnimator.HandPose = grabInRange ? _hoverHandPose : _hand.HandAnimator.HandPose;
        }

        private void FixedUpdate()
        {
            // if (_hand.CurrentGrab && _hand.CurrentGrab.GrabMechanic == HandType.Slider)
            // {
            //     // Forzar la rotación de la mano
            //     Quaternion targetRotation = Quaternion.Euler(0f, _hand.PalmTransform.rotation.eulerAngles.y, 0f); // Mantén el eje Y
            //     _hand.PhysicsHandTransform.rotation = targetRotation;
            // }
        }

        public void ApplyGrabPose(HandPose handPose)
        {
            if (!handPose)
                handPose = _defaultGrabHandPose;

            _hand.HandAnimator.HandPose = handPose;
        }

      private Grab GetChosenGrab()
        {
            Collider[] grabColliders = Physics.OverlapBox(
                _grabBounds.position,
                _grabBounds.localScale / 2,
                _grabBounds.rotation,
                Physics.AllLayers,
                QueryTriggerInteraction.Collide
            );

            float highestRank = 0;
            Grab highestRankGrab = null;

            foreach (Collider grabCollider in grabColliders)
            {
                Grab grab = grabCollider.GetComponent<Grab>() ?? grabCollider.GetComponentInParent<Grab>();

                if (grab == null)
                    continue;

                // Obtén el GrabPoint más cercano que sea válido para la mano
                Transform chosenGrabPoint = grab.GetClosestGrabPoint(this);
                grab.ActualizarprimaryGrabOffset(grab.GetClosestGrabPoint(this));

                if (chosenGrabPoint == null)
                    continue;

                GrabPoint grabPoint = chosenGrabPoint.GetComponent<GrabPoint>();
                if (grabPoint == null || !grabPoint.IsAvailableFor(_hand))
                {
                    // Si el GrabPoint no está disponible, ignóralo
                    continue;
                }

                float grabRank = grab.CalculateRank(_hand.PalmTransform);
                if (grabRank <= highestRank || grabRank <= 0f)
                    continue;

                highestRank = grabRank;
                highestRankGrab = grab;
            }

            return highestRankGrab;
        }


        public void AttemptGrab()
        {
            if (!_chosenGrab)
                return;

            _chosenGrab.OnGrab(this); // Pasa el GrabHandler en lugar de la mano
            _audioSource.PlayOneShot(Utilities.RandomAudioClip(_grabSounds));
        }

       public void AttemptRelease()
        {
            if (!_hand.CurrentGrab)
                return;

            _hand.CurrentGrab.OnRelease(this, true); // Pasa el GrabHandler
            _audioSource.PlayOneShot(Utilities.RandomAudioClip(_releaseSounds));
        }


        private void OnDisable()
        {
            AttemptRelease();
        }
    }
}
