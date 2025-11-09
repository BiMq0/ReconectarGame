using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NpcGirlDialogue : MonoBehaviour
{
    // =============================================================
    // ESTRUCTURAS DE DATOS
    // =============================================================
    [System.Serializable]
    public class DialogueLine
    {
        [TextArea(3, 5)]
        public string text;
        [Tooltip("Índice del sprite en 'carasPJ'.")]
        public int expressionIndex = 0;
    }

    // =============================================================
    // CONFIGURACIÓN Y REFERENCIAS
    // =============================================================
    [Header("Configuración del Evento")]
    public string eventID = "NPC_GIRL_EVENT";

    [Header("Referencias de UI")]
    public GameObject interactionIndicator;
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public Image icon;
    public Sprite[] carasPJ;
    public Animator animator;

    [Header("Flujo de Diálogo")]
    public DialogueLine[] dialogue;

    [Header("Ajustes de Diálogo")]
    [Tooltip("Velocidad de tipeo en CARACTERES POR SEGUNDO (ej: 20.0).")]
    public float typeSpeed = 20.0f;
    [Tooltip("Tiempo de espera entre líneas.")]
    public float autoPauseDuration = 1.5f;
    private Coroutine dialogueCoroutine = null;

    // =============================================================
    // CICLO DE VIDA Y TRIGGERS
    // =============================================================
    private void Start()
    {
        animator = GetComponent<Animator>();

        if (GameManager.Instance != null && GameManager.Instance.IsEventCompleted(eventID))
        {
            enabled = false;
            Collider2D col = gameObject.GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
            return;
        }

        if (interactionIndicator != null) interactionIndicator.SetActive(false);
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !GameManager.IsEventActive && enabled)
        {
            ExecuteInteractionEvent();
            Collider2D col = gameObject.GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
        }
    }

    // =============================================================
    // FLUJO DEL DIÁLOGO AUTOMÁTICO
    // =============================================================
    private void ExecuteInteractionEvent()
    {
        if (GameManager.Instance == null || dialogueCoroutine != null) return;

        if (dialoguePanel == null || dialogueText == null)
        {
            Debug.LogError("FATAL ERROR: diálogoPanel o dialogueText es NULL. Verifica el Inspector.");
            return;
        }

        GameManager.Instance.SetCinematicMode(true);
        if (interactionIndicator != null) interactionIndicator.SetActive(false);

        dialogueCoroutine = StartCoroutine(RunDialogue(dialogue));
    }

    private IEnumerator RunDialogue(DialogueLine[] lines)
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(true);

        int lineIndex = 0;

        float charDelay = 0.05f; 
        if (typeSpeed > 0.0f)
        {
            charDelay = 1f / typeSpeed;
        }

        foreach (DialogueLine line in lines)
        {
            if (lineIndex == 4)
            {
                if (animator != null) animator.SetBool("isActive", true);
            }

            if (icon != null && line.expressionIndex < carasPJ.Length)
            {
                icon.sprite = carasPJ[line.expressionIndex];
            }

            dialogueText.text = "";

            for (int i = 0; i < line.text.Length; i++)
            {
                dialogueText.text += line.text[i];
                yield return new WaitForSeconds(charDelay);
            }

            yield return new WaitForSeconds(autoPauseDuration);

            if (lineIndex == 7)
            {
                if (animator != null) animator.SetBool("isActive", false);
            }

            lineIndex++;
        }

        dialoguePanel.SetActive(false);

        dialogueText.text = "";

        if (!GameManager.Instance.IsEventCompleted(eventID))
        {
            GameManager.Instance.MarkEventCompleted(eventID);
        }

        GameManager.Instance.SetCinematicMode(false);

        dialogueCoroutine = null;
        enabled = false;
    }
}