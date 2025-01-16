using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class CollisionProximityWithColliders : MonoBehaviour
{
    public GameObject referenceObject; // Objeto 1 (con collider externo)
    public GameObject targetObject;    // Objeto 2 (donde se generarán los colliders)

    public Transform[] referencePoints = new Transform[4]; // Puntos de referencia cardinales
    private readonly string[] directions = { "Up", "Down", "Left", "Right" };

    [ContextMenu("Create Cardinal Colliders")]
    public void CreateCardinalColliders()
    {
        if (referenceObject == null || targetObject == null)
        {
            Debug.LogError("Por favor, asigna los objetos en el inspector.");
            return;
        }

        Collider referenceCollider = referenceObject.GetComponent<Collider>();
        if (referenceCollider == null)
        {
            Debug.LogError("El Objeto 1 debe tener un Collider.");
            return;
        }

        Vector3[] cardinalDirections = {
            Vector3.up,    // Arriba
            Vector3.down,  // Abajo
            Vector3.left,  // Izquierda
            Vector3.right  // Derecha
        };

        for (int i = 0; i < cardinalDirections.Length; i++)
        {
            Vector3 direction = cardinalDirections[i];
            Vector3 localOffset = direction * 0.01f; // Ajuste milimétrico

            // Calcular posición cardinal relativa al Objeto 2
            Vector3 referencePosition = CalculateClosestPoint(referenceCollider, targetObject, direction, localOffset);

            // Crear o actualizar punto de referencia
            if (referencePoints[i] == null)
            {
                GameObject point = new GameObject($"ReferencePoint_{directions[i]}");
                point.transform.parent = targetObject.transform;
                referencePoints[i] = point.transform;

                // Agregar collider en el punto
                Collider newCollider = referenceCollider is BoxCollider ?
                    point.AddComponent<BoxCollider>() :
                    referenceCollider is SphereCollider ? point.AddComponent<SphereCollider>() :
                    referenceCollider is CapsuleCollider ? point.AddComponent<CapsuleCollider>() :
                    null;

                if (newCollider != null)
                {
                    CopyColliderSettings(referenceCollider, newCollider);
                }
            }

            // Ajustar posición y rotación del punto de referencia
            referencePoints[i].position = referencePosition;
            referencePoints[i].rotation = targetObject.transform.rotation; // Basado en el Objeto 2
        }
    }

  private Vector3 CalculateClosestPoint(Collider referenceCollider, GameObject target, Vector3 direction, Vector3 offset)
{
    Collider targetCollider = target.GetComponent<Collider>();
    if (targetCollider == null)
    {
        Debug.LogError("El Objeto 2 debe tener un Collider.");
        return Vector3.zero;
    }

    // Obtener los límites del collider del Objeto 2
    Bounds targetBounds = targetCollider.bounds;

    // Calcular la posición en base a los límites del target
    Vector3 center = targetBounds.center;
    Vector3 extents = targetBounds.extents;

    // Ajustar la posición según la dirección cardinal
    Vector3 closestPoint = center;

    if (direction == Vector3.up)
    {
        closestPoint += new Vector3(0, extents.y, 0);
    }
    else if (direction == Vector3.down)
    {
        closestPoint -= new Vector3(0, extents.y, 0);
    }
    else if (direction == Vector3.left)
    {
        closestPoint -= new Vector3(extents.x, 0, 0);
    }
    else if (direction == Vector3.right)
    {
        closestPoint += new Vector3(extents.x, 0, 0);
    }

    // Aplicar el ajuste milimétrico
    closestPoint += offset;

    return closestPoint;
}

    private void CopyColliderSettings(Collider source, Collider destination)
    {
        if (source is BoxCollider srcBox && destination is BoxCollider destBox)
        {
            destBox.size = srcBox.size;
            destBox.center = srcBox.center;
        }
        else if (source is SphereCollider srcSphere && destination is SphereCollider destSphere)
        {
            destSphere.radius = srcSphere.radius;
            destSphere.center = srcSphere.center;
        }
        else if (source is CapsuleCollider srcCapsule && destination is CapsuleCollider destCapsule)
        {
            destCapsule.radius = srcCapsule.radius;
            destCapsule.height = srcCapsule.height;
            destCapsule.direction = srcCapsule.direction;
            destCapsule.center = srcCapsule.center;
        }
        else
        {
            Debug.LogWarning("Tipo de collider no soportado.");
        }
    }

    [ContextMenu("Clear Cardinal Colliders")]
    public void ClearCardinalColliders()
    {
        foreach (var point in referencePoints)
        {
            if (point != null)
            {
                DestroyImmediate(point.gameObject);
            }
        }

        referencePoints = new Transform[4];
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(CollisionProximityWithColliders))]
public class CollisionProximityWithCollidersEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CollisionProximityWithColliders script = (CollisionProximityWithColliders)target;

        if (GUILayout.Button("Crear Colliders Cardinales"))
        {
            script.CreateCardinalColliders();
        }

        if (GUILayout.Button("Borrar Colliders Cardinales"))
        {
            script.ClearCardinalColliders();
        }
    }
}
#endif
