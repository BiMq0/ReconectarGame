using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class YatiriEvent : MonoBehaviour
{
    [Header("Configuración del Evento")]
    public string eventID = "YATIRI_MAIN_EVENT";

    [Header("Referencias")]
    public GameObject interactionIndicator;
    public GameObject panelYatiri; 
    public YatiriDialogueManager dialogueManager; 

    [Header("Flujos de Diálogo")]
    public YatiriDialogueManager.DialogueLine[] introductionDialogue; 
    public YatiriDialogueManager.DialogueLine[] interactionDialogue;

    // --- Variables de Estado y Input ---
    private InputAction interactAction;
    private InputAction dialogueAdvanceAction;

    private bool playerIsInRange = false; 
    private bool introDialogueStarted = false; 
    private bool introDialogueFinished = false;
    private bool eventCompleted = false;

    void Start()
    {
        if (interactionIndicator != null)
        {
            interactionIndicator.SetActive(false); 
        }

        eventCompleted = GameManager.Instance != null && GameManager.Instance.IsEventCompleted(eventID);

        if (eventCompleted)
        {
            enabled = false;
            return;
        }

        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null && player.playerActions != null)
        {
            InputActionMap playerMap = player.playerActions.FindActionMap("Player");

            interactAction = playerMap.FindAction("Interactuar");

            dialogueAdvanceAction = playerMap.FindAction("SaltarDialogo");

            if (dialogueAdvanceAction == null)
            {
                Debug.LogError("La acción 'SaltarDialogo' no se encontró en la Action Map 'Player'.");
            }

            if (dialogueManager != null)
            {
                dialogueManager.Initialize(dialogueAdvanceAction); 
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !GameManager.IsEventActive && enabled)
        {
            playerIsInRange = true;
            if (interactionIndicator != null && introDialogueFinished)
            {
                interactionIndicator.SetActive(true);
            }
            if (!introDialogueStarted)
            {
                StartIntroDialogue();
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
    private void StartIntroDialogue()
    {
        if (dialogueManager == null || introDialogueStarted) return;

        introDialogueStarted = true;

        GameManager.Instance.SetCinematicMode(true);

        dialogueManager.StartDialogue(introductionDialogue, false, OnIntroDialogueFinished);
    }
    private void OnIntroDialogueFinished()
    {
        GameManager.Instance.SetCinematicMode(false);

        introDialogueFinished = true;

        if (playerIsInRange && interactionIndicator != null)
        {
            interactionIndicator.SetActive(true);
        }
    }
    void Update()
    {
        if (playerIsInRange && introDialogueFinished && interactAction != null && interactAction.WasPressedThisFrame())
        {
            ExecuteInteractionEvent();
        }
    }
    private void ExecuteInteractionEvent()
    {
        if (GameManager.Instance == null || GameManager.IsEventActive) return;

        GameManager.Instance.SetCinematicMode(true);
        if (interactionIndicator != null) interactionIndicator.SetActive(false);
        if (panelYatiri != null) panelYatiri.SetActive(true);

        if (dialogueManager != null)
        {
            dialogueManager.StartDialogue(interactionDialogue, true, OnInteractionDialogueFinished);
        }
    }

    private void OnInteractionDialogueFinished()
    {
        Debug.Log("DEBUG: El diálogo de interacción ha finalizado. Ejecutando cierre del evento.");

        if (panelYatiri != null) panelYatiri.SetActive(false);

        GameManager.Instance.MarkEventCompleted(eventID);

        GameManager.Instance.SetCinematicMode(false);
        enabled = false;

        RoomsManager.Instance.ChangeLevel("Lvl_1"); 
    }
}
