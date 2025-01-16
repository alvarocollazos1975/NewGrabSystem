using UnityEngine;

namespace BIMOS
{
    [AddComponentMenu("BIMOS/Grabs/Grab Point")]
    public class GrabPoint : MonoBehaviour
    {
        public HandPose HandPose;
        public bool IsLeftHanded = true;
        public bool IsRightHanded = true;
        public GrabPoint[] EnableGrabPoint, DisableGrabPoint;

        [HideInInspector] public Hand CurrentHand = null; // Mano que está usando el GrabPoint

        /// <summary>
        /// Verifica si el GrabPoint está disponible para una mano.
        /// </summary>
        public bool IsAvailableFor(Hand hand)
        {
            return CurrentHand == null &&
                   ((hand.IsLeftHand && IsLeftHanded) || (!hand.IsLeftHand && IsRightHanded));
        }

        /// <summary>
        /// Ocupa el GrabPoint y actualiza los estados de otros GrabPoints.
        /// </summary>
        public void Occupy(Hand hand)
        {
            if (!IsAvailableFor(hand))
            {
                Debug.LogWarning($"{name} no está disponible para {hand.name}");
                return;
            }

            CurrentHand = hand;

            // Actualizar estados de GrabPoints relacionados
            InicializarGrabPoints();

            Debug.Log($"{name} ocupado por {hand.name}");
        }

        /// <summary>
        /// Libera el GrabPoint y actualiza los estados de otros GrabPoints.
        /// </summary>
        public void Release()
        {
            if (CurrentHand == null)
            {
                Debug.LogWarning($"{name} ya está libre.");
                return;
            }

            Debug.Log($"{name} liberado por {CurrentHand.name}");

            CurrentHand = null;

            // Restaurar estados de GrabPoints relacionados
            DesInicializarGrabPoints();
        }

        /// <summary>
        /// Habilita o deshabilita los GrabPoints relacionados.
        /// </summary>
        public void InicializarGrabPoints()
        {
            foreach (GrabPoint grabpoint in EnableGrabPoint)
            {
                if (grabpoint)
                    grabpoint.enabled = true;
            }
            foreach (GrabPoint grabpoint in DisableGrabPoint)
            {
                if (grabpoint)
                    grabpoint.enabled = false;
            }
        }

        /// <summary>
        /// Restaura los estados originales de los GrabPoints relacionados.
        /// </summary>
        public void DesInicializarGrabPoints()
        {
            foreach (GrabPoint grabpoint in EnableGrabPoint)
            {
                if (grabpoint)
                    grabpoint.enabled = false;
            }
            foreach (GrabPoint grabpoint in DisableGrabPoint)
            {
                if (grabpoint)
                    grabpoint.enabled = true;
            }
        }

        private void OnEnable()
        {
            if (CurrentHand != null)
                InicializarGrabPoints();
        }

        private void OnDisable()
        {
            DesInicializarGrabPoints();
        }
    }
}
