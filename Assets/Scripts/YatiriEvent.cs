using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class YatiriEvent : MonoBehaviour
{
    [Header("Configuración del Evento")]
    [Tooltip("ID único para este evento (ej: 'NPC_Jefe_Sala1').")]
    public string eventID;

    [Header("Referencias")]
    [Tooltip("El sprite/icono de interacción (ej: una 'E' o un círculo).")]
    public GameObject interactionIndicator;
    public GameObject panelYatiri;

    [Header("Ajustes de Prueba")]
    [Tooltip("Tiempo de simulación del evento (ej: duración del diálogo).")]
    public float eventDuration = 3.0f;

    private InputAction interactAction;
    private bool playerIsInRange = false;

    void Start()
    {
        if (interactionIndicator != null)
        {
            interactionIndicator.SetActive(false);
        }
        if (GameManager.Instance != null && GameManager.Instance.IsEventCompleted(eventID))
        {
            Debug.Log($"Evento {eventID} ya completado. Desactivando interacción.");
            enabled = false;
            return;

        }

        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null && player.playerActions != null)
        {
            interactAction = player.playerActions.FindActionMap("Player").FindAction("Interactuar");
        }
        else
        {
            Debug.LogError("No se pudo encontrar el PlayerController o la ActionMap.");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !GameManager.IsEventActive && enabled)
        {
            playerIsInRange = true;
            if (interactionIndicator != null)
            {
                interactionIndicator.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsInRange = false;
            if (interactionIndicator != null)
            {
                interactionIndicator.SetActive(false);
            }
        }
    }

    void Update()
    {
        if (playerIsInRange && interactAction != null && interactAction.WasPressedThisFrame())
        {
            ExecuteInteractionEvent();
        }
    }

    // =============================================================
    // EJECUCIÓN Y FINALIZACIÓN DEL EVENTO
    // =============================================================
    private void ExecuteInteractionEvent()
    {
        if (GameManager.Instance == null) return;

        GameManager.Instance.SetCinematicMode(true);
        if (interactionIndicator != null) interactionIndicator.SetActive(false);

        StartCoroutine(HandleEventCompletion());
    }

    private IEnumerator HandleEventCompletion()
    {
        Debug.Log($"INICIANDO EVENTO: {eventID}. Duración: {eventDuration}s");

        panelYatiri.SetActive(true);

        yield return new WaitForSeconds(eventDuration);
        panelYatiri.SetActive(false);


        // 2. MARCAR Y GUARDAR
        GameManager.Instance.MarkEventCompleted(eventID); // Marca y guarda en PlayerPrefs
        RoomsManager.Instance.ChangeLevel("Lvl_1");
        // 3. DESBLOQUEAR Y DESACTIVAR
        GameManager.Instance.SetCinematicMode(false);
        enabled = false; // Desactiva este script para que no se active de nuevo.
        Debug.Log("DEBUG: Tutorial finalizado y transición a Nivel 1 iniciada.");
    }
}
