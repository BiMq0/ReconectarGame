using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InteractionScript : MonoBehaviour
{
    [System.Serializable]
    public class DialogueLine
    {
        [TextArea(2, 4)]
        public string text;
    }

    [Header("═══ CONFIGURACIÓN ═══")]
    public string eventID = "NPC_EVENT";

    [Header("═══ DIÁLOGOS ═══")]
    public DialogueLine[] dialogueLines;

    [Header("═══ REFERENCIAS UI ═══")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public Image icon;

    [Header("═══ ICONO DEL NPC ═══")]
    public Sprite npcIcon;

    [Header("═══ AJUSTES ═══")]
    public float typeSpeed = 0.05f;
    public float interactionDistance = 2f;

    private bool playerNear = false;
    private bool isDialoguing = false;
    private bool isTyping = false;
    private bool dialogueCompleted = false;
    private Transform playerTransform;
    private Collider2D triggerCollider;

    private void Start()
    {
        Debug.Log($"[InteractionScript] Inicializando {gameObject.name}");

        // Encontrar el jugador
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (playerTransform == null)
        {
            Debug.LogError($"[InteractionScript] Player no encontrado en {gameObject.name}");
        }

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
        else
            Debug.LogWarning($"[InteractionScript] dialoguePanel no asignado en {gameObject.name}");

        triggerCollider = GetComponent<Collider2D>();
        if (triggerCollider != null && !triggerCollider.isTrigger)
        {
            Debug.LogWarning($"[InteractionScript] El Collider2D en {gameObject.name} no es Trigger, ajustando...");
            triggerCollider.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !GameManager.IsEventActive && enabled)
        {
            Debug.Log($"[InteractionScript] Jugador entró en rango de {gameObject.name}");
            playerNear = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"[InteractionScript] Jugador salió del rango de {gameObject.name}");
            playerNear = false;
        }
    }

    private void Update()
    {
        if (playerNear && Input.GetKeyDown(KeyCode.E) && !isDialoguing && !GameManager.IsEventActive)
        {
            Debug.Log($"[InteractionScript] Interacción iniciada con {gameObject.name}");
            StartCoroutine(ExecuteInteraction());
        }

        if (!isTyping && isDialoguing && Input.GetMouseButtonDown(0))
        {
            dialogueCompleted = true;
        }
    }

    private IEnumerator ExecuteInteraction()
    {
        isDialoguing = true;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetCinematicMode(true);
        }

        yield return StartCoroutine(RunDialogue(dialogueLines));

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetCinematicMode(false);
        }

        isDialoguing = false;
        Debug.Log($"[InteractionScript] Interacción completada con {gameObject.name}");
    }

    private IEnumerator RunDialogue(DialogueLine[] lines)
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }

        if (icon != null && npcIcon != null)
        {
            icon.sprite = npcIcon;
        }

        if (lines == null || lines.Length == 0)
        {
            Debug.LogWarning($"[InteractionScript] No hay líneas de diálogo en {gameObject.name}");
            yield break;
        }

        foreach (DialogueLine line in lines)
        {
            yield return StartCoroutine(TypeAndAdvance(line));
        }
    }

    private IEnumerator TypeAndAdvance(DialogueLine line)
    {
        isTyping = true;
        dialogueText.text = "";

        // Escribir letra por letra
        for (int i = 0; i < line.text.Length; i++)
        {
            dialogueText.text += line.text[i];
            yield return new WaitForSeconds(typeSpeed);
        }

        isTyping = false;

        // Esperar click para avanzar
        dialogueCompleted = false;
        yield return new WaitUntil(() => dialogueCompleted);

        yield return new WaitForSeconds(0.2f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}