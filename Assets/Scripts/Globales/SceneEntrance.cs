using UnityEngine;

// Este script va en los GameObjects SpawnPointLeft y SpawnPointRight.
public class SceneEntranceTrigger : MonoBehaviour
{
    [Header("Configuración de Dirección")]
    [Tooltip("1 para avanzar, -1 para regresar.")]
    public int directionToMove = 1;

    private Collider2D triggerCollider;
    // Eliminamos la variable privada 'roomsManager'

    private void Awake()
    {
        triggerCollider = GetComponent<Collider2D>();

        if (triggerCollider == null)
        {
            Debug.LogError($"El Spawn Point '{gameObject.name}' no tiene un Collider2D (Is Trigger).");
        }
    }

    private void OnEnable()
    {
        // Usamos RoomsManager.Instance directamente.
        if (RoomsManager.Instance == null || triggerCollider == null) return;

        (int currentIndex, int totalScenes) = RoomsManager.Instance.GetMapState();
        bool shouldBeActive = true;

        // Lógica de Tope:
        if (directionToMove == -1) // Es el SpawnPointLeft (para ir hacia atrás)
        {
            // Solo debe desactivarse si está en la PRIMERA escena (índice 0)
            if (currentIndex == 0)
            {
                shouldBeActive = false;
            }
        }
        else if (directionToMove == 1) // Es el SpawnPointRight (para ir hacia adelante)
        {
            // Solo debe desactivarse si está en la ÚLTIMA escena
            if (currentIndex == totalScenes - 1)
            {
                shouldBeActive = false;
            }
        }

        // Establecer el estado del Collider basado en la lógica de tope.
        triggerCollider.enabled = shouldBeActive;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && triggerCollider != null && triggerCollider.enabled)
        {
            triggerCollider.enabled = false;
            if (RoomsManager.Instance != null)
            {
                RoomsManager.Instance.GoToRoom(directionToMove);
            }
            else
            {
                Debug.LogError("Error FATAL: RoomsManager.Instance es NULL al iniciar una transición.");
                triggerCollider.enabled = true; // Reactivar para no quedar atascado
            }
        }
    }
}