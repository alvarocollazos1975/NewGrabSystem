using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BIMOS
{
    [AddComponentMenu("BIMOS/Grabs/Grab")]
    public abstract class Grab : MonoBehaviour
    {
        
       
         /// <summary>
        /// Snap to a location or grab anywhere on the object
        /// </summary>
        [Tooltip("Snap to a location or grab anywhere on the object")]
        public HandType GrabMechanic = HandType.Grip;
        [Header("Grab Points")]
        [Tooltip("Lista de puntos de agarre disponibles para este objeto.")]
        public List<Transform> grabPoints = new List<Transform>();

        [Header("Grab Settings")]
        [Tooltip("Permitir agarre con la mano izquierda.")]
        public bool IsLeftHanded = true;
        [SerializeField] private float maxGrabDistance = 0.5f;

        [Tooltip("Permitir agarre con la mano derecha.")]
        public bool IsRightHanded = true;
        public Grab[] EnableGrabs, DisableGrabs;

         [HideInInspector]
        public Hand LeftHand, RightHand;

        public HandPose HandPose;
        public delegate void Release();
        public event Release ReleaseEvent;

         private Rigidbody _rigidBody;
        private ArticulationBody _articulationBody;
        private Transform _body;

        private List<GrabHandler> grabbingHandlers = new List<GrabHandler>(); // GrabHandlers que están agarrando el objeto
        private Dictionary<GrabHandler, ConfigurableJoint> handlerJoints = new Dictionary<GrabHandler, ConfigurableJoint>(); // Joints creados por GrabHandler
        private Dictionary<GrabHandler, HandType> handlerTypes = new Dictionary<GrabHandler, HandType>(); // Tipos de agarre por GrabHandler


        [HideInInspector]
        public Collider Collider;

        [HideInInspector]
        Vector3 grabPosition
        {
            get
            {
                if (primaryGrabOffset != null)
                {
                    return primaryGrabOffset.position;
                }
                else
                {
                    return transform.position;
                }
            }
        }      

         [HideInInspector]
        public Vector3 GrabPositionOffset
        {
            get
            {
                if (primaryGrabOffset)
                {
                    return primaryGrabOffset.transform.localPosition;
                }

                return Vector3.zero;
            }
        }


        [HideInInspector]
        public Vector3 GrabRotationOffset
        {
            get
            {
                if (primaryGrabOffset)
                {
                    return primaryGrabOffset.transform.localEulerAngles;
                }
                return Vector3.zero;
            }
        }

        protected Transform primaryGrabOffset;

        [HideInInspector]
        public Vector3 SecondaryLookOffset;

        [HideInInspector]
        public Transform SecondaryLookAtTransform;

        [HideInInspector]
        public Transform LocalOffsetTransform;
        Transform getGrabPoint;

        protected List<GrabHandler> heldByGrabbers = new List<GrabHandler>();

        public List<GrabHandler> HeldByGrabbers
        {
            get
            {
                return heldByGrabbers;
            }
        }

        [SerializeField] private List<Collider> colliders = new List<Collider>();

        private void Awake()
        {
            // Si no se asignaron colliders, intenta encontrarlos automáticamente
            if (colliders.Count == 0)
            {
                colliders.AddRange(GetComponentsInChildren<Collider>());
            }

            if (colliders.Count == 0)
            {
                Debug.LogError("No se encontraron colliders en el objeto. Por favor, asigne al menos uno.");
            }
        }

       private void LateUpdate()
        {
            // Si no hay handlers agarrando, salir
            if (grabbingHandlers.Count == 0) return;

            foreach (var handler in grabbingHandlers)
            {
                if (!handlerJoints.TryGetValue(handler, out ConfigurableJoint joint)) continue;

                // Si el tipo de agarre es un slider, actualiza la posición y rotación
                if (handlerTypes[handler] == HandType.Slider)
                {
                    joint.connectedAnchor = _body.InverseTransformPoint(primaryGrabOffset.position);

                    // Sincronizar rotación (opcional, según tus necesidades)
                    Quaternion targetRotation = Quaternion.Inverse(_rigidBody.transform.rotation) * handler._hand.PhysicsHandTransform.rotation;
                    joint.targetRotation = targetRotation;

                    Debug.Log($"Actualizando slider para {handler.name}. ConnectedAnchor: {joint.connectedAnchor}");
                }
            }

             foreach (var hand in grabbingHandlers)
            {
                if (handlerTypes[hand] == HandType.Slider)
                {
                     SyncHandRotationWithSlider(hand);

                }
               
            }
        }

        private void SyncHandRotationWithSlider(GrabHandler grabHandler)
        {
            if (grabHandler == null || grabHandler._hand.CurrentGrab == null) return;

            

            // Asegurarse de que la mano siga la rotación del slider
            grabHandler._hand.PalmTransform.rotation = primaryGrabOffset.rotation;
        }




        /// <summary>
        /// Calcula la idoneidad de un agarre para una mano.
        /// Este método puede ser sobrescrito en clases derivadas.
        /// </summary>
        /// <param name="handTransform">Transform de la mano que intenta agarrar.</param>
        /// <returns>Puntuación del agarre (valores mayores indican mejor ajuste).</returns>
        public virtual float CalculateRank(Transform handTransform)
        {
             if (Collider is MeshCollider)
                return 1f / 1000f;

            return 1f / Vector3.Distance(handTransform.position, Collider.ClosestPoint(handTransform.position));
        }

        /// <summary>
        /// Alinea la mano al objeto agarrado.
        /// Este método debe ser sobrescrito en clases derivadas para definir la lógica de alineación.
        /// </summary>
        /// <param name="hand">La mano que realiza el agarre.</param>
        public virtual void AlignHand(Hand hand)
        {
            // Implementación base vacía.
        }

/// <summary>
/// Obtiene el punto de agarre más cercano compatible con la mano controlada por el GrabHandler.
/// </summary>
/// <param name="grabHandler">Instancia del GrabHandler asociado a la mano.</param>
/// <returns>Transform del punto de agarre más cercano, o null si no se encuentra uno compatible.</returns>
public virtual Transform GetClosestGrabPoint(GrabHandler grabHandler)
{
    if (grabHandler == null || grabHandler._hand == null)
    {
        Debug.LogWarning("GrabHandler o su Hand asociada son nulos.");
        return null;
    }

    Hand hand = grabHandler._hand; // Obtiene la mano asociada al GrabHandler
    Transform closestPoint = null;
    float closestDistance = float.MaxValue;

    foreach (Transform grabPoint in grabPoints)
    {
        if (grabPoint == null) 
            continue;

        GrabPoint grabPointComponent = grabPoint.GetComponent<GrabPoint>();

        if (grabPointComponent == null || !grabPointComponent.enabled)
        {
            continue;
        }

        // Verifica si el punto de agarre es compatible con la mano
        if (!grabPointComponent.IsAvailableFor(hand))
        {
            continue;
        }

        // Verifica si el GrabPoint ya está ocupado por otro GrabHandler
        if (grabPointComponent.CurrentHand != null && grabPointComponent.CurrentHand.GrabHandler != grabHandler)
        {
            continue;
        }

        // Calcula la distancia entre la mano y el punto de agarre
        float distance = Vector3.Distance(hand.PalmTransform.position, grabPoint.position);

        if (distance < closestDistance)
        {
            closestDistance = distance;
            closestPoint = grabPoint;

            // Actualiza la pose de la mano asociada al punto de agarre más cercano
            HandPose = grabPointComponent.HandPose;
        }
    }

    return closestPoint;
}


        /// <summary>
        /// Método que selecciona el punto de agarre según la mano y lógica personalizada.
        /// </summary>
        /// <param name="hand">La mano que intenta agarrar.</param>
        /// <returns>El punto de agarre elegido.</returns>
        public virtual Transform GetChosenGrab(GrabHandler hand)
        {
            return GetClosestGrabPoint(hand); // Utiliza el punto más cercano por defecto.
        }

         public virtual Transform ActualizarprimaryGrabOffset(Transform transform)
        {
            return primaryGrabOffset=transform; // Utiliza el punto más cercano por defecto.
        }




        /// <summary>
        /// Comprueba si el agarre es compatible con una mano dada.
        /// </summary>
        /// <param name="hand">La mano que intenta agarrar.</param>
        /// <returns>True si el agarre es compatible, de lo contrario false.</returns>
        public bool IsCompatibleWithHand(Hand hand)
        {
            if (hand.IsLeftHand && !IsLeftHanded)
                return false;

            if (!hand.IsLeftHand && !IsRightHanded)
                return false;

            return true;
        }
         private void OnEnable()
        {
            _body = Utilities.GetBody(transform, out _rigidBody, out _articulationBody);
            if (!_body)
            {
                Rigidbody rigidbody = gameObject.AddComponent<Rigidbody>();
                rigidbody.isKinematic = true;
                _rigidBody = rigidbody;
                _body = _rigidBody.transform;
            }

            Collider = GetComponent<Collider>();
            if (Collider == null)
            {
                CreateCollider();
            }

            CreateGrabPoint();
        }

        /// <summary>
        /// Returns the Grabber that first grabbed this item. Return null if not being held.
        /// </summary>
        /// <returns></returns>
       public virtual GrabHandler GetPrimaryGrabber(Hand hand)
        {
            if (heldByGrabbers != null)
            {
                foreach (var grabber in heldByGrabbers)
                {
                    if (grabber != null && grabber._chosenGrab == this)
                    {
                        return grabber;
                    }
                }
            }

            return null;
        }
        

        /// <summary>
        /// Método que se ejecuta cuando el objeto comienza a ser agarrado.
        /// Este método puede ser sobrescrito en clases derivadas.
        /// </summary>
        /// <param name="hand">La mano que realiza el agarre.</param>
       public virtual void OnGrab(GrabHandler grabHandler)
        {
            if (grabHandler == null)
            {
                Debug.LogWarning("GrabHandler no proporcionado.");
                return;
            }

            Hand hand = grabHandler._hand; // Obtenemos la mano desde el GrabHandler
            hand.CurrentGrab = this;

            if (!heldByGrabbers.Contains(grabHandler))
            {
                heldByGrabbers.Add(grabHandler);
            }

            // Encuentra y ocupa un GrabPoint válido
            GrabPoint chosenGrabPoint = GetClosestGrabPoint(grabHandler)?.GetComponent<GrabPoint>();
            if (chosenGrabPoint != null)
            {
                chosenGrabPoint.Occupy(hand);
                hand.CurrentGrabPoint = chosenGrabPoint;

                // Deshabilitar los GrabPoints relacionados
                foreach (GrabPoint relatedGrabPoint in chosenGrabPoint.DisableGrabPoint ?? new GrabPoint[0])
                {
                    relatedGrabPoint.enabled = false;
                }
            }

            // Configurar alineación, pose y Join según el tipo de agarre
            if (GrabMechanic == HandType.Slider)
            {
                //StartCoroutine(AlignHandToGrabPointSmooth(grabHandler));
                StartCoroutine(AlignHandToGrabPoint(grabHandler));
            }
            else
            {
                StartCoroutine(AlignHandToGrabPoint(grabHandler));
            }

            AttachHandToGrabPoint(hand);
            grabHandler.ApplyGrabPose(HandPose);
            AlignHand(hand);

            IgnoreCollision(hand, true);

             foreach (Grab grab in EnableGrabs)
            {
                if (grab)
                    grab.enabled = true;
            }
            foreach (Grab grab in DisableGrabs)
            {
                if (grab)
                    grab.enabled = false;
            }

            GetComponent<Interactable>()?.OnGrab();

            Debug.Log($"{gameObject.name} ha sido agarrado por {grabHandler.name}.");
        }


         public virtual void IgnoreCollision(Hand hand, bool ignore)
        {
            foreach (Collider collider in _body.GetComponentsInChildren<Collider>())
            {
                Physics.IgnoreCollision(collider, hand.PhysicsHandCollider, ignore);
            }
        }

        

    public void CreateGrabJoint(GrabHandler grabHandler, HandType grabMechanic)
    {
        if (grabHandler == null || grabbingHandlers.Contains(grabHandler))
        {
            Debug.LogWarning("El GrabHandler es nulo o ya está agarrando el objeto.");
            return;
        }

        // Crear un ConfigurableJoint
        ConfigurableJoint joint = grabHandler._hand.PhysicsHandTransform.gameObject.AddComponent<ConfigurableJoint>();

        // Configurar el cuerpo conectado
        if (_rigidBody != null)
            joint.connectedBody = _rigidBody;

          // Configurar restricciones basadas en el GrabMechanic
        if (grabMechanic == HandType.Slider)
        {
            joint.autoConfigureConnectedAnchor = false;
            joint.anchor = Vector3.zero;
            joint.connectedAnchor = _body.InverseTransformPoint(primaryGrabOffset.position);
            //joint.connectedAnchor = _body.InverseTransformPoint(primaryGrabOffset.position);

            // Permitir movimiento solo en el eje Z
            joint.xMotion = ConfigurableJointMotion.Locked;
            joint.yMotion = ConfigurableJointMotion.Locked;
            joint.zMotion = ConfigurableJointMotion.Limited;
            joint.angularXMotion = ConfigurableJointMotion.Locked;
            joint.angularYMotion = ConfigurableJointMotion.Locked;
            joint.angularZMotion = ConfigurableJointMotion.Locked;

            // Configurar límites en el eje Z
            SoftJointLimit zLimit = new SoftJointLimit
            {
                limit = 0.1f // Ajusta según el rango de movimiento permitido
            };
            joint.linearLimit = zLimit;

            joint.angularXDrive = new JointDrive
            {
                positionSpring = 1000f,
                positionDamper = 100f,
                maximumForce = Mathf.Infinity
            };
            joint.angularYZDrive = joint.angularXDrive;
            joint.slerpDrive = joint.angularXDrive;

            // Bloquear todas las rotaciones
            joint.angularXMotion = ConfigurableJointMotion.Locked;
            joint.angularYMotion = ConfigurableJointMotion.Locked;
            joint.angularZMotion = ConfigurableJointMotion.Locked;
        }
        else if (grabMechanic == HandType.Grip)
        {
            joint.autoConfigureConnectedAnchor = false;

            // Bloquear todos los movimientos y rotaciones
            joint.xMotion = ConfigurableJointMotion.Locked;
            joint.yMotion = ConfigurableJointMotion.Locked;
            joint.zMotion = ConfigurableJointMotion.Locked;
            joint.angularXMotion = ConfigurableJointMotion.Locked;
            joint.angularYMotion = ConfigurableJointMotion.Locked;
            joint.angularZMotion = ConfigurableJointMotion.Locked;

            // Ajustar el offset del punto de agarre
            joint.anchor = Vector3.zero;
            joint.connectedAnchor = _body.InverseTransformPoint(primaryGrabOffset.position);
        }

        // Registrar el handler y el joint
        grabbingHandlers.Add(grabHandler);
        handlerJoints[grabHandler] = joint;
        handlerTypes[grabHandler] = grabMechanic;

        Debug.Log($"Joint creado para {grabMechanic}. Offset: {primaryGrabOffset.position}");
    }




        /// <summary>
        /// Método que se ejecuta cuando el objeto deja de ser agarrado.
        /// Este método puede ser sobrescrito en clases derivadas.
        /// </summary>
        /// <param name="hand">La mano que suelta el objeto.</param>
        /// <param name="toggleGrabs">Indica si se deben habilitar o deshabilitar otros GrabPoints asociados.</param>
        public virtual void OnRelease(GrabHandler grabHandler, bool toggleGrabs)
        {
             if (grabHandler == null || !grabbingHandlers.Contains(grabHandler)) return;

            if (handlerJoints.TryGetValue(grabHandler, out ConfigurableJoint joint))
            {
                Destroy(joint);
            }

            grabbingHandlers.Remove(grabHandler);
            handlerJoints.Remove(grabHandler);
            handlerTypes.Remove(grabHandler);

              // Si la mano tiene un GrabPoint ocupado, libera ese punto
            GrabPoint occupiedGrabPoint = grabHandler._hand.CurrentGrabPoint; // Asegúrate de que este campo esté configurado en Hand
            if (occupiedGrabPoint != null)
            {
                occupiedGrabPoint.Release();
                Debug.Log($"GrabPoint {occupiedGrabPoint.name} liberado por {grabHandler.name}.");
            }

            PhysicsHand physicsHand = grabHandler._hand.GetComponent<PhysicsHand>();
            if (physicsHand != null)
            {
                physicsHand.ResetOffsets();
            }

            DetachHandFromGrabPoint(grabHandler);
           // ReleaseGrabJoint(hand);

            if (toggleGrabs)
            {
                foreach (Grab grab in EnableGrabs)
                {
                    if (grab)
                        grab.enabled = false;
                }
                foreach (Grab grab in DisableGrabs)
                {
                    if (grab)
                        grab.enabled = true;
                }
            }

             // Restaurar la rotación inicial de la mano
            grabHandler._hand.PalmTransform.rotation = grabHandler._hand.PhysicsHandTransform.rotation;          

            GetComponent<Interactable>()?.OnRelease();
            grabHandler._hand.CurrentGrab = null;
            grabHandler._hand.CurrentGrabPoint?.Release();
            grabHandler._hand.CurrentGrabPoint = null;

            Debug.Log($"{gameObject.name} ha sido soltado por {grabHandler.name}.");
        }



         public virtual void DestroyGrabJoint(Hand hand)
        {
            if (hand == null)
                return;
                 FixedJoint grabJoint = hand.PhysicsHandTransform.GetComponent<FixedJoint>();

            if (GrabMechanic == HandType.Slider) {               
                grabJoint.anchor = Vector3.zero;  
            }

            Destroy(grabJoint);           

            IgnoreCollision(hand, false);
        }

       

      private void OnDisable()
        {
            // Si hay manos sujetando el objeto, libera cada una usando su GrabHandler
            foreach (GrabHandler grabHandler in heldByGrabbers.ToArray()) // Itera sobre una copia para evitar modificaciones concurrentes
            {
                OnRelease(grabHandler, false); // Pasa el GrabHandler directamente
            }
            
            // Limpia las referencias internas
            heldByGrabbers.Clear();
            LeftHand = null;
            RightHand = null;
        }

       private void AttachHandToGrabPoint(Hand hand)
        {
            hand.transform.SetParent(primaryGrabOffset); // Asignar al grab point
            hand.transform.localPosition = Vector3.zero; // Sin offset de posición
            hand.transform.localRotation = Quaternion.identity; // Sin offset de rotación
        }

        private IEnumerator AlignHandToGrabPoint(GrabHandler hand)
        {
            // Alineación suave antes de fijar la posición
            float duration = 0.1f; // Duración de la interpolación
            float elapsed = 0f;

            Vector3 startPosition = hand.transform.position;
            Quaternion startRotation = hand.transform.rotation;

            Vector3 targetPosition = primaryGrabOffset.position;
            Quaternion targetRotation = primaryGrabOffset.rotation;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                hand.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
                hand.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);

                yield return null;
            }

            // Asegurar alineación final
            hand.transform.position = targetPosition;
            hand.transform.rotation = targetRotation;
            CreateGrabJoint(hand, HandType.Grip); // Para el grip

          
        }
        private IEnumerator AlignHandToGrabPointSmooth(GrabHandler hand)
        {
            Transform grabPoint = primaryGrabOffset; // Obtén el punto de agarre correspondiente
            float duration = 0.2f; // Tiempo para completar la alineación
            float elapsedTime = 0f;

            Vector3 startPosition = hand._hand.PalmTransform.position;
            Quaternion startRotation = hand._hand.PalmTransform.rotation;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / duration;

                // Movimiento y rotación suaves hacia el punto de agarre
                hand._hand.PalmTransform.position = Vector3.Lerp(startPosition, grabPoint.position, t);

                // Mantener la rotación restringida al eje Y y Z
                Quaternion targetRotation = grabPoint.rotation;
                targetRotation = Quaternion.Euler(
                    startRotation.eulerAngles.x, // Mantén el eje X original
                    targetRotation.eulerAngles.y,
                    targetRotation.eulerAngles.z
                );

                hand._hand.PalmTransform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);

                yield return null;
            }

            // Asegura que la mano esté exactamente en el punto final
            hand._hand.PalmTransform.position = grabPoint.position;
            hand._hand.PalmTransform.rotation = grabPoint.rotation;

            CreateGrabJoint(hand, HandType.Slider); // Para el slider
        }



        private void DetachHandFromGrabPoint(GrabHandler hand)
        {
            hand.transform.SetParent(null);
        }

        public virtual void CreateCollider()
        {
            SphereCollider collider = gameObject.AddComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = 0.01f;
            Collider = collider;
        }
      
          private void CreateGrabPoint()
        {
            if (primaryGrabOffset == null)
            {
                primaryGrabOffset = new GameObject("Primary Grab Point").transform;
                primaryGrabOffset.SetParent(this.transform);
                primaryGrabOffset.localPosition = Vector3.zero;
            }

        }

       private void AlignObjectToHand(Hand hand)
        {
            if (primaryGrabOffset != null)
            {
                // Mover el objeto para alinear su posición con la mano
                transform.position = hand.PhysicsHandTransform.position;

                // Ajustar la rotación del objeto para que coincida con la mano
                transform.rotation = hand.PhysicsHandTransform.rotation * Quaternion.Inverse(primaryGrabOffset.localRotation);
            }
        }



        /// <summary>
        /// Visualiza los puntos de agarre en el editor para facilitar ajustes.
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;

            foreach (Transform grabPoint in grabPoints)
            {
                if (grabPoint != null)
                {
                    Gizmos.DrawWireSphere(grabPoint.position, 0.01f);
                }
            }

             Gizmos.color = Color.red;

            // Dibuja una esfera en la posición de primaryGrabOffset si existe
            if (primaryGrabOffset != null)
            {
                Gizmos.DrawWireSphere(primaryGrabOffset.position, 0.02f);

                // También puedes dibujar una línea desde el objeto al primaryGrabOffset para mayor claridad
                Gizmos.DrawLine(transform.position, primaryGrabOffset.position);

                // Opcional: Añadir un label (requiere usar Handles)
                #if UNITY_EDITOR
                UnityEditor.Handles.Label(primaryGrabOffset.position, "Primary Grab Offset");
               
                #endif
            }          
        }
    }
}
public enum HandType
{
    Grip,
    Slider
}