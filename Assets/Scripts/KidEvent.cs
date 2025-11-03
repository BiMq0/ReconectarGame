using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class KidEvent : MonoBehaviour
{
    // =============================================================
    // ESTRUCTURAS DE DATOS
    // =============================================================
    public enum Speaker { NPC = 0, Player = 1, Decision = -1 }

    [System.Serializable]
    public class DialogueLine
    {
        [Tooltip("Quién habla (NPC: Niño, Player: Jugador, Decision: Pausar y mostrar opciones).")]
        public Speaker speaker = Speaker.NPC;
        [TextArea(3, 5)]
        public string text;
        [Tooltip("Índice del sprite en 'carasNiño' (solo se aplica si el Speaker es NPC).")]
        public int expressionIndex = 0;
    }

    // =============================================================
    // CONFIGURACIÓN Y REFERENCIAS
    // =============================================================
    [Header("Configuración del Evento")]
    public string eventID = "NIÑO_EVENT";

    [Header("Referencias de UI")]
    public GameObject interactionIndicator;
    public GameObject dialoguePanel; 
    public TextMeshProUGUI dialogueText;
    public Image kidExpressionImage;
    public Sprite[] carasNiño;

    [Header("Flujo de Diálogo")]
    public DialogueLine[] introductionDialogue;
    public string[] decisionOptions = new string[3];
    public DialogueLine[] route1Dialogue;
    public DialogueLine[] route2Dialogue;
    public DialogueLine[] route3Dialogue;

    [Header("Ajustes de Diálogo")]
    public float typeSpeed = 50f;

    private InputAction interactAction;
    private InputAction dialogueAdvanceAction;
    private bool playerIsInRange = false;
    private bool isTyping = false;
    private int playerChoice = 0;

    // =============================================================
    // CICLO DE VIDA (Sin cambios)
    // =============================================================
    void Start()
    {
        if (interactionIndicator != null) interactionIndicator.SetActive(false);
        if (dialoguePanel != null) dialoguePanel.SetActive(false);

        if (GameManager.Instance != null)
        {
            playerChoice = GameManager.Instance.LoadDecision(eventID);

            if (GameManager.Instance.IsEventCompleted(eventID))
            {
                enabled = false;
                return;
            }
        }

        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null && player.playerActions != null)
        {
            interactAction = player.playerActions.FindActionMap("Player").FindAction("Interactuar");
            dialogueAdvanceAction = player.playerActions.FindActionMap("Player").FindAction("SaltarDialogo");
            if (dialogueAdvanceAction == null)
            {
                Debug.LogError("La acción 'SaltarDialogo' no se encontró en la Action Map 'Player'.");
            }
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
        if (playerIsInRange && interactAction != null && interactAction.WasPressedThisFrame())
        {
            ExecuteInteractionEvent();
        }
    }

    private void ExecuteInteractionEvent()
    {
        if (GameManager.Instance == null || GameManager.IsEventActive) return;

        if (dialoguePanel.activeSelf) return;

        GameManager.Instance.SetCinematicMode(true);
        if (interactionIndicator != null) interactionIndicator.SetActive(false);
        StartCoroutine(HandleDecisionSequence());
    }

    // =============================================================
    // FLUJO PRINCIPAL Y DIÁLOGO (Sin cambios, solo el final)
    // =============================================================
    private IEnumerator HandleDecisionSequence()
    {
        yield return StartCoroutine(RunDialogue(introductionDialogue));

        if (playerChoice != 0)
        {
            Debug.Log($"Reanudando evento con elección: {playerChoice}");
        }

        if (playerChoice == 0)
        {
            yield return StartCoroutine(WaitForPlayerChoice());
        }

        DialogueLine[] finalRoute;
        switch (playerChoice)
        {
            case 1: finalRoute = route1Dialogue; break;
            case 2: finalRoute = route2Dialogue; break;
            case 3: finalRoute = route3Dialogue; break;
            default: finalRoute = null; break;
        }

        if (finalRoute != null)
        {
            yield return StartCoroutine(RunDialogue(finalRoute));
        }

        if (!GameManager.Instance.IsEventCompleted(eventID))
        {
            GameManager.Instance.MarkEventCompleted(eventID);
            GameManager.Instance.SaveDecision(eventID, playerChoice);
        }

        dialoguePanel.SetActive(false); 
        GameManager.Instance.SetCinematicMode(false);
        enabled = false;
    }

    private IEnumerator RunDialogue(DialogueLine[] lines)
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(true);

        foreach (DialogueLine line in lines)
        {
            if (line.speaker == Speaker.Decision)
            {
                yield return StartCoroutine(WaitForPlayerChoice());
                break;
            }

            if (line.speaker == Speaker.NPC && kidExpressionImage != null && line.expressionIndex < carasNiño.Length)
            {
                kidExpressionImage.sprite = carasNiño[line.expressionIndex];
            }

            yield return StartCoroutine(TypeAndAdvance(line));
        }
    }

    private IEnumerator TypeAndAdvance(DialogueLine line)
    {
        string speakerPrefix = line.speaker == Speaker.NPC ? "Niño: " : "Player: ";
        string fullText = speakerPrefix + line.text;

        if (dialogueAdvanceAction == null)
        {
            dialogueText.text = fullText;
            yield break;
        }

        isTyping = true;
        dialogueText.text = "";
        float charDelay = 1f / typeSpeed;

        foreach (char c in fullText)
        {
            dialogueText.text += c;

            if (dialogueAdvanceAction.WasPressedThisFrame())
            {
                dialogueText.text = fullText;
                isTyping = false;
                break;
            }
            yield return new WaitForSeconds(charDelay);
        }

        isTyping = false;

        yield return new WaitUntil(() => dialogueAdvanceAction.WasPressedThisFrame());

        yield return null;
    }

    // =============================================================
    // CORRECCIÓN CLAVE: MANTENER dialoguePanel ACTIVO
    // =============================================================
    private IEnumerator WaitForPlayerChoice()
    {
        if (DecisionManager.Instance == null)
        {
            Debug.LogError("DecisionManager no encontrado. Usando opción 1 por defecto.");
            playerChoice = 1;
            yield break;
        }

        yield return DecisionManager.Instance.StartCoroutine(
            DecisionManager.Instance.WaitForDecision(decisionOptions, (choice) =>
            {
                playerChoice = choice;
            })
        );
    }
}