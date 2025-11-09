using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class EventDialogScript : MonoBehaviour
{
    [System.Serializable]
    public class DialogueLine
    {
        [TextArea(2, 4)]
        public string text;
    }

    [Header("═══ CONFIGURACIÓN ═══")]
    public string eventID = "EVENT_DIALOG";

    [Header("═══ DIÁLOGOS CONDICIONALES ═══")]
    [Tooltip("Nombre del item a verificar en el inventario")]
    public string itemToCheck = "";

    [Tooltip("Diálogos si el jugador NO tiene el item")]
    public DialogueLine[] dialogueWithoutItem;

    [Tooltip("Diálogos si el jugador SÍ tiene el item")]
    public DialogueLine[] dialogueWithItem;

    [Header("═══ REFERENCIAS UI ═══")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public Image icon;

    [Header("═══ ICONO DEL NPC ═══")]
    public Sprite npcIcon;

    [Header("═══ INDICADOR DE INTERACCIÓN ═══")]
    [Tooltip("Prefab del ojo/indicador de 'Pulsa E' que aparece cuando te acercas")]
    public GameObject interactionIndicator;

    private GameObject interactionIndicatorInstance;

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
        Debug.Log($"[EventDialogScript] Inicializando {gameObject.name}");

        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (playerTransform == null)
        {
            Debug.LogError($"[EventDialogScript] Player no encontrado en {gameObject.name}");
        }

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
        else
            Debug.LogWarning($"[EventDialogScript] dialoguePanel no asignado en {gameObject.name}");

        // Verificar que el Collider es Trigger
        triggerCollider = GetComponent<Collider2D>();
        if (triggerCollider != null && !triggerCollider.isTrigger)
        {
            Debug.LogWarning($"[EventDialogScript] El Collider2D en {gameObject.name} no es Trigger, ajustando...");
            triggerCollider.isTrigger = true;
        }

        // Instanciar el indicador de interacción (el ojo)
        if (interactionIndicator != null)
        {
            interactionIndicatorInstance = Instantiate(interactionIndicator, transform);
            interactionIndicatorInstance.SetActive(false);
            Debug.Log($"[EventDialogScript] Indicador de interacción creado para {gameObject.name}");
        }
        else
        {
            Debug.LogWarning($"[EventDialogScript] interactionIndicator no asignado en {gameObject.name}");
        }

        Debug.Log($"[EventDialogScript] Item a verificar: '{itemToCheck}'");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !GameManager.IsEventActive && enabled)
        {
            Debug.Log($"[EventDialogScript] Jugador entró en rango de {gameObject.name}");
            playerNear = true;

            if (interactionIndicatorInstance != null)
            {
                interactionIndicatorInstance.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"[EventDialogScript] Jugador salió del rango de {gameObject.name}");
            playerNear = false;

            if (interactionIndicatorInstance != null)
            {
                interactionIndicatorInstance.SetActive(false);
            }
        }
    }

    private void Update()
    {
        // Detectar tecla E
        if (playerNear && Input.GetKeyDown(KeyCode.E) && !isDialoguing && !GameManager.IsEventActive)
        {
            Debug.Log($"[EventDialogScript] Interacción iniciada con {gameObject.name}");
            StartCoroutine(ExecuteConditionalInteraction());
        }

        // Avanzar diálogo con click
        if (!isTyping && isDialoguing && Input.GetMouseButtonDown(0))
        {
            dialogueCompleted = true;
        }
    }

    private IEnumerator ExecuteConditionalInteraction()
    {
        isDialoguing = true;

        // Ocultar indicador
        if (interactionIndicatorInstance != null)
        {
            interactionIndicatorInstance.SetActive(false);
        }

        // Congelar jugador
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetCinematicMode(true);
        }

        // Verificar si el jugador tiene el item
        bool hasItem = false;
        if (InventoryScript.Instance != null)
        {
            hasItem = InventoryScript.Instance.TieneItem(itemToCheck);
            Debug.Log($"[EventDialogScript] Verificando item '{itemToCheck}': {(hasItem ? "✓ TIENE" : "✗ NO TIENE")}");
        }
        else
        {
            Debug.LogError($"[EventDialogScript] InventoryScript.Instance no encontrado");
        }

        // Seleccionar diálogos según condición
        DialogueLine[] dialoguesToShow = hasItem ? dialogueWithItem : dialogueWithoutItem;

        if (dialoguesToShow == null || dialoguesToShow.Length == 0)
        {
            Debug.LogWarning($"[EventDialogScript] No hay diálogos asignados para la condición (hasItem: {hasItem})");
            isDialoguing = false;
            yield break;
        }

        // Mostrar diálogos
        yield return StartCoroutine(RunDialogue(dialoguesToShow));

        // Limpiar UI
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        // Descongelar jugador
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetCinematicMode(false);
        }

        isDialoguing = false;
        Debug.Log($"[EventDialogScript] Interacción completada con {gameObject.name}");
    }

    private IEnumerator RunDialogue(DialogueLine[] lines)
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        if (icon != null && npcIcon != null)
        {
            icon.sprite = npcIcon;
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
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}
