using UnityEngine;
using UnityEngine.Events;

public class ConfigurableJointLimitEvent : MonoBehaviour
{
    public ConfigurableJoint configurableJoint;
    public UnityEvent OnActivate;   // Evento cuando se alcanza el límite inferior
    public UnityEvent OnDeactivate; // Evento cuando se sale del límite inferior

    private bool isAtLowerLimit = false; // Estado interno para controlar la transición
    private bool IsholdingSlider = false;

    private const float lowerLimit = -0.04f; // Límite inferior en Z
    private const float upperLimit = 0.0f;   // Límite superior en Z

    private void Start()
    {
        // Configurar el ConfigurableJoint al inicio
        if (configurableJoint != null)
        {
            SoftJointLimit softLimit = new SoftJointLimit
            {
                limit = Mathf.Abs(lowerLimit) // Configuramos el límite como absoluto
            };

            configurableJoint.linearLimit = softLimit;

            // Configurar el ancla inicial para mantener el rango
            configurableJoint.anchor = new Vector3(configurableJoint.anchor.x, configurableJoint.anchor.y, 0);
            configurableJoint.connectedAnchor = new Vector3(configurableJoint.connectedAnchor.x, configurableJoint.connectedAnchor.y, 0);
        }
    }

    void Update()
    {
        if (configurableJoint != null)
        {
            // Obtener la posición local actual en Z
            Vector3 localPosition = configurableJoint.transform.localPosition;

            // Limitar la posición en Z dentro del rango permitido
            if (localPosition.z < lowerLimit)
                localPosition.z = lowerLimit;
            else if (localPosition.z > upperLimit)
                localPosition.z = upperLimit;

            // Aplicar la posición restringida
            configurableJoint.transform.localPosition = localPosition;

            // Lógica de activación/desactivación de eventos
            if (localPosition.z <= lowerLimit + 0.002f && !isAtLowerLimit && IsholdingSlider)
            {
                isAtLowerLimit = true; // Actualiza el estado
                OnActivate?.Invoke();  // Llama al evento
            }
            else if (localPosition.z >= upperLimit - 0.002f && isAtLowerLimit)
            {
                isAtLowerLimit = false; // Actualiza el estado
                OnDeactivate?.Invoke(); // Llama al evento
            }
        }
    }

    public void ValidarIsGrabbingSlider(bool isgrabbing)
    {
        IsholdingSlider = isgrabbing;
    }
}
