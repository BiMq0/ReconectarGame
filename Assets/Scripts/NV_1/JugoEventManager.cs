using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class JugoEventManager : MonoBehaviour
{
    [Header("Configuración del Evento")]
    public string eventID = "JUGO_EVENT";
    [Tooltip("Frase exacta que el jugador debe escribir.")]
    public string specialCode = "chicha";

    // =============================================================
    // REFERENCIAS DE UI Y COMPONENTES
    // =============================================================
    [Header("Referencias")]
    public GameObject interactionIndicator;
    public GameObject panelJugo; 
    public JugoDialogueManager dialogueManager;
    public Animator crowdAnimator; 
    public Animator nayeliAnimator; 

    [Header("Flujos de Diálogo")]
    public JugoDialogueManager.DialogueLine[] introductionDialogue;
    public JugoDialogueManager.DialogueLine[] approachDialogue;
    public JugoDialogueManager.DialogueLine[] successDialogue;
    public JugoDialogueManager.DialogueLine[] failureDialogue;

    [Header("Ajustes de Empuje y QTE")]
    [Tooltip("Clicks necesarios para pasar de fase (Total: 15).")]
    public int clicksPerPhase = 5;
    [Tooltip("Tiempo en segundos antes de que el contador de clicks baje 1.")]
    public float decayTimerDuration = 2.0f;
    [Tooltip("Tiempo límite para escribir la frase especial (Fase 3).")]
    public float inputTimeLimit = 15.0f;

    // =============================================================
    // VARIABLES DE ESTADO Y INPUT (Mantenido)
    // =============================================================
    private InputAction interactAction;
    private InputAction pushAction;

    private bool playerIsInRange = false;
    private bool eventCompleted = false;
    private int currentProgressClicks = 0;
    private bool isAwaitingInput = false;
    private bool hasMadeChoice = false;
    private Coroutine decayCoroutine = null;

    // =============================================================
    // CICLO DE VIDA Y TRIGGERS (Mantenido)
    // =============================================================
    private void Awake()
    {
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null && player.playerActions != null)
        {
            interactAction = player.playerActions.FindActionMap("Player").FindAction("Interactuar");
            // Usamos "SaltarDialogo" para evitar el bloqueo del Cinemático.
            pushAction = player.playerActions.FindActionMap("Player").FindAction("SaltarDialogo");
        }
        else
        {
            Debug.LogError("No se encontró PlayerController o sus InputActions.");
        }
    }

    private void Start()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsEventCompleted(eventID))
        {
            enabled = false;
            if (panelJugo != null) panelJugo.SetActive(false);
            if (interactionIndicator != null) interactionIndicator.SetActive(false);
            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
            return;
        }

        if (panelJugo != null) panelJugo.SetActive(false);
        if (interactionIndicator != null) interactionIndicator.SetActive(false);

        if (dialogueManager != null && dialogueManager.inputField != null)
        {
            dialogueManager.inputField.onEndEdit.AddListener(CheckInputResult);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !GameManager.IsEventActive && enabled)
        {
            playerIsInRange = true;
            if (interactionIndicator != null) interactionIndicator.SetActive(true);
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
            if (GetComponent<Collider2D>() != null) GetComponent<Collider2D>().enabled = false;
            StartEventSequence();
        }
        if (GameManager.IsEventActive && !isAwaitingInput && pushAction != null && pushAction.WasPressedThisFrame())
        {
            HandlePushInput();
        }
    }

    // =============================================================
    // FLUJO PRINCIPAL DEL EVENTO (Mantenido)
    // =============================================================
    private void StartEventSequence()
    {
        if (GameManager.Instance == null || dialogueManager == null) return;

        GameManager.Instance.SetCinematicMode(true);

        if (interactionIndicator != null) interactionIndicator.SetActive(false);

        panelJugo.SetActive(true);
        currentProgressClicks = 0;
        isAwaitingInput = false;
        UpdateCrowdAnimator();

        if (decayCoroutine == null)
        {
            StartCoroutine(EventFlowCoroutine());
        }
        else
        {
            decayCoroutine = StartCoroutine(DecayProgressCoroutine());
        }
    }

    private IEnumerator EventFlowCoroutine()
    {
        yield return StartCoroutine(RunAndAwaitDialogue(introductionDialogue));

        decayCoroutine = StartCoroutine(DecayProgressCoroutine());

        yield return new WaitUntil(() => currentProgressClicks >= 15 || !GameManager.IsEventActive);

        if (!GameManager.IsEventActive) yield break;

        if (decayCoroutine != null) StopCoroutine(decayCoroutine);
        UpdateCrowdAnimator();

        if (nayeliAnimator != null)
        {
            nayeliAnimator.SetBool("IsPlayerClose", true);
        }

        yield return StartCoroutine(RunAndAwaitDialogue(approachDialogue));

        yield return StartCoroutine(InputPhaseCoroutine());
    }

    // =============================================================
    // LÓGICA DE EMPUJE (QTE)
    // =============================================================
    private void HandlePushInput()
    {
        if (currentProgressClicks < 15)
        {
            currentProgressClicks = Mathf.Min(currentProgressClicks + 1, 15);
            UpdateCrowdAnimator();
            // DEBUG: Mostrar el aumento de clicks
            Debug.Log($"DEBUG INPUT: Empuje registrado. Clicks: {currentProgressClicks}");
        }
    }

    private IEnumerator DecayProgressCoroutine()
    {
        while (currentProgressClicks < 15)
        {
            yield return new WaitForSeconds(decayTimerDuration);

            if (currentProgressClicks > 0)
            {
                currentProgressClicks = Mathf.Max(currentProgressClicks - 1, 0);
                UpdateCrowdAnimator();
                Debug.Log($"DEBUG DECAY: Decaimiento. Clicks: {currentProgressClicks}");
            }
        }
    }

    private void UpdateCrowdAnimator()
    {
        if (crowdAnimator == null) return;

        int phase = 0;

        if (currentProgressClicks >= 15)
        {
            phase = 3;
        }
        else
        {
            phase = Mathf.FloorToInt(currentProgressClicks / (float)clicksPerPhase);
            phase = Mathf.Min(phase, 2);
        }

        crowdAnimator.SetInteger("CrowdPhase", phase);
    }

    // =============================================================
    // LÓGICA DE INPUT (FASE 3) (Mantenido)
    // =============================================================
    private IEnumerator InputPhaseCoroutine()
    {
        dialogueManager.ActivateInputPanel(true);
        isAwaitingInput = true;
        hasMadeChoice = false;

        float timer = inputTimeLimit;

        while (timer > 0 && !hasMadeChoice)
        {
            timer -= Time.deltaTime;
            yield return null;
        }

        dialogueManager.ActivateInputPanel(false);
        isAwaitingInput = false;

        if (!hasMadeChoice)
        {
            CheckInputResult(dialogueManager.inputField.text);
        }
    }

    public void CheckInputResult(string submittedText)
    {
        if (!isAwaitingInput) return;

        hasMadeChoice = true;
        string cleanedText = submittedText.Trim().ToUpper();

        dialogueManager.inputField.text = "";

        if (cleanedText == specialCode.Trim().ToUpper())
        {
            Debug.Log($"DEBUG: Respuesta correcta recibida: {cleanedText}.");
            StartCoroutine(EndEvent(true));
        }
        else
        {
            Debug.Log($"DEBUG: Respuesta incorrecta recibida: {cleanedText}.");
            StartCoroutine(EndEvent(false));
        }
    }

    // =============================================================
    // GESTIÓN DE DIÁLOGO Y EVENTO (Mantenido)
    // =============================================================

    private IEnumerator RunAndAwaitDialogue(JugoDialogueManager.DialogueLine[] lines)
    {
        bool dialogueFinished = false;
        dialogueManager.StartAutomaticDialogue(lines, () => dialogueFinished = true);
        yield return new WaitUntil(() => dialogueFinished);
    }

    private IEnumerator EndEvent(bool success)
    {
        dialogueManager.StopCurrentDialogue();

        // Detenemos el Animator de Nayeli (que estaba en Static/Atención)
        if (success)
        {
            if (nayeliAnimator != null)
            {
                nayeliAnimator.SetBool("IsPlayerClose", false);
            }
            if (dialogueManager.nayeliImage != null && dialogueManager.nayeliExpressions.Length > 0)
            {
                dialogueManager.nayeliImage.sprite = dialogueManager.nayeliExpressions[0];
            }

            yield return StartCoroutine(RunAndAwaitDialogue(successDialogue));

            GameManager.Instance.MarkEventCompleted(eventID);
            eventCompleted = true;

            if (panelJugo != null) panelJugo.SetActive(false);
            dialogueManager.dialoguePanel.SetActive(false);
            
            GameManager.Instance.SetCinematicMode(false);
            enabled = false;
        }
        else
        {
            if (dialogueManager.nayeliImage != null && dialogueManager.nayeliExpressions.Length > 1)
            {
                dialogueManager.nayeliImage.sprite = dialogueManager.nayeliExpressions[1];
            }

            yield return StartCoroutine(RunAndAwaitDialogue(failureDialogue));

            if (dialogueManager.nayeliImage != null && dialogueManager.nayeliExpressions.Length > 0)
            {
                dialogueManager.nayeliImage.sprite = dialogueManager.nayeliExpressions[0];
            }
            if (nayeliAnimator != null)
            {
                nayeliAnimator.SetBool("IsPlayerClose", false);
            }
            StartEventSequence();
            yield break;
        }
    }
}