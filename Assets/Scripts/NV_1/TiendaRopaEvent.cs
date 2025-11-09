using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class TiendaRopaEvent : MonoBehaviour
{
    // =============================================================
    // CONFIGURACIÓN Y REFERENCIAS
    // =============================================================
    [Header("Configuración del Evento")]
    public string eventID = "TIENDA_ROPA_EVENT";

    [Header("Referencias de UI y Diálogo")]
    public GameObject interactionIndicator;
    public DialogueManagerRopa dialogueManager;
    public GameObject tiendaPanel; // Panel que contiene la vista de la dueña (diálogo manual)
    public GameObject minijuegoPanel; // Panel que contiene la mesa y las capas de ropa
    public GameObject ropaObjetivoGO; // La ropa que debe ser encontrada (debe tener el RopaItem.cs)

    [Header("Referencias de Brazos del Jugador")]
    public GameObject playerArmsParent; // Se mantiene, pero solo para activar/desactivar el GO

    // ¡CORRECCIÓN AQUÍ! Referencias a los Animators individuales
    public Animator leftArmAnimator;
    public Animator rightArmAnimator;
    private bool nextArmIsRight = true;

    [Header("Ajustes del Minijuego")]
    [Tooltip("Número de prendas que deben moverse antes de que se active un diálogo especial (Opcional).")]
    public int ropaClearedThreshold = 3;

    // =============================================================
    // INPUT Y ESTADO
    // =============================================================
    private InputAction interactAction;
    private InputAction advanceDialogueAction;
    private bool playerIsInRange = false;
    private bool isMinigameActive = false;
    private int currentRopaCleared = 0;

    // =============================================================
    // CICLO DE VIDA E INPUT
    // =============================================================

    private void Awake()
    {
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null && player.playerActions != null)
        {
            interactAction = player.playerActions.FindActionMap("Player").FindAction("Interactuar");
            advanceDialogueAction = player.playerActions.FindActionMap("Player").FindAction("SaltarDialogo");
        }
        else
        {
            Debug.LogError("No se encontró PlayerController o sus InputActions.");
        }
    }

    private void Start()
    {
        tiendaPanel.SetActive(false);
        minijuegoPanel.SetActive(false);
        if (playerArmsParent != null) playerArmsParent.SetActive(false);

        if (GameManager.Instance != null && GameManager.Instance.IsEventCompleted(eventID))
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !GameManager.IsEventActive && enabled)
        {
            playerIsInRange = true;
            if (interactionIndicator != null) interactionIndicator.SetActive(true);

            if (!GameManager.IsEventActive)
            {
                StartCoroutine(StartEventFlow());
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsInRange = false;
            if (interactionIndicator != null) interactionIndicator.SetActive(false);
        }
    }

    void Update()
    {
        if (playerIsInRange && interactAction != null && interactAction.WasPressedThisFrame() && !GameManager.IsEventActive)
        {
            StartCoroutine(StartEventFlow());
        }

        if (GameManager.IsEventActive && advanceDialogueAction != null && advanceDialogueAction.WasPressedThisFrame())
        {
            if (dialogueManager != null && dialogueManager.dialoguePanel.activeSelf)
            {
                dialogueManager.AdvanceManualDialogue();
            }
        }
    }

    // =============================================================
    // FLUJO DEL EVENTO
    // =============================================================

    private IEnumerator StartEventFlow()
    {
        GameManager.Instance.SetCinematicMode(true);
        if (interactionIndicator != null) interactionIndicator.SetActive(false);

        yield return StartCoroutine(RunAndAwaitDialogue(dialogueManager.introductionDialogue, false));

        tiendaPanel.SetActive(true);
        yield return StartCoroutine(RunAndAwaitDialogue(dialogueManager.interactionDialogue, true));


        StartMinijuego();
    }

    private void StartMinijuego()
    {
        minijuegoPanel.SetActive(true);
        isMinigameActive = true;
        currentRopaCleared = 0;

        // Mostrar los brazos del jugador
        if (playerArmsParent != null) playerArmsParent.SetActive(true);

        // Iniciar Diálogo Evento (Automático durante la búsqueda, se detendrá al encontrar)
        StartCoroutine(RunAndAwaitDialogue(dialogueManager.eventDialogue, false));
    }

    // Llamado por RopaItem.cs
    public void NotifyRopaCleared(GameObject ropa)
    {
        if (!isMinigameActive) return;

        // Si es la ropa objetivo, ¡VICTORIA!
        if (ropa == ropaObjetivoGO)
        {
            StartCoroutine(VictoryFlow());
        }
        else
        {
            currentRopaCleared++;
            // Lógica opcional para pistas o diálogos intermedios aquí.
        }
    }
    public Animator GetCurrentGrabArm()
    {
        // Devuelve el Animator del brazo que tiene el turno
        return nextArmIsRight ? rightArmAnimator : leftArmAnimator;
    }
    public void ToggleNextGrabArm()
    {
        // Cambia el estado para el siguiente agarre
        nextArmIsRight = !nextArmIsRight;
    }

    private IEnumerator VictoryFlow()
    {
        isMinigameActive = false;

        // Ocultar los brazos y el panel de minijuego
        minijuegoPanel.SetActive(false);
        if (playerArmsParent != null) playerArmsParent.SetActive(false);

        // Detener el diálogo automático del evento
        dialogueManager.StopCurrentDialogue();

        // --- FASE 4: DIÁLOGO DE VICTORIA (Manual) ---
        yield return StartCoroutine(RunAndAwaitDialogue(dialogueManager.victoryDialogue, true));
        nextArmIsRight = true;
        tiendaPanel.SetActive(false);
        // --- CIERRE FINAL DEL EVENTO ---
        GameManager.Instance.MarkEventCompleted(eventID);
        GameManager.Instance.SetCinematicMode(false);

        // Limpieza final
        Destroy(gameObject);
    }


    // =============================================================
    // UTILIDADES DE DIÁLOGO
    // =============================================================

    private IEnumerator RunAndAwaitDialogue(DialogueManagerRopa.DialogueLine[] lines, bool isManual)
    {
        bool dialogueFinished = false;

        if (dialogueManager == null || lines == null || lines.Length == 0)
        {
            dialogueFinished = true;
        }
        else if (isManual)
        {
            dialogueManager.StartManualDialogue(lines, () => dialogueFinished = true);
        }
        else
        {
            dialogueManager.StartAutomaticDialogue(lines, () => dialogueFinished = true);
        }

        yield return new WaitUntil(() => dialogueFinished);
    }

   
}