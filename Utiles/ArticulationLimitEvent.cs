using UnityEngine;
using UnityEngine.Events;

public class ArticulationLimitEvent : MonoBehaviour
{
    public ArticulationBody articulationBody;
    public UnityEvent OnActivate;   // Evento cuando se alcanza el límite
    public UnityEvent OnDeactivate; // Evento cuando se sale del límite

    private bool isAtLowerLimit = false; // Estado interno para controlar la transición
    private bool IsholdingSlider=false;

    void Update()
    {
        if (articulationBody != null)
        {
            // Obtén la posición actual en el eje Z
            float currentPosition = articulationBody.jointPosition[0];

            // Si está en el límite inferior y no se ha activado antes
            if (currentPosition <= -0.038f && !isAtLowerLimit && IsholdingSlider)
            {
                isAtLowerLimit = true; // Actualiza el estado
                OnActivate?.Invoke();  // Llama al evento
            }
            // Si sale del límite inferior
            else if (currentPosition >= -0.02f && isAtLowerLimit)
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
