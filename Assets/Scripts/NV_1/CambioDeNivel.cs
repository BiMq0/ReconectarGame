using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class CambioDeNivel : MonoBehaviour
{
    // =============================================================
    // CONFIGURACIÓN Y REFERENCIAS (CORREGIDO)
    // =============================================================
    [Header("Configuración de Transición")]
    public string nextSceneName = "Nivel2_Plaza";

    [Header("Referencias de Diálogo")]
    public FinalDialogueManager finalDialogueManager; // ¡NUEVO TIPO!
    public FinalDialogueManager.DialogueLine[] finalDialogue;

    [Header("Ajustes de Limpieza")]
    public GameObject[] persistentObjectsToDestroy;

    // =============================================================
    // INPUT Y ESTADO
    // =============================================================
    private InputAction advanceDialogueAction;
    private bool playerIsInRange = false;

    private void Start()
    {
        // ... (Verificaciones de Collider y obtención de advanceDialogueAction)
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null && player.playerActions != null)
        {
            advanceDialogueAction = player.playerActions.FindActionMap("Player").FindAction("SaltarDialogo");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !GameManager.IsEventActive)
        {
            playerIsInRange = true;
            StartCoroutine(TransitionSequence());
        }
    }

    void Update()
    {
        // Permite avanzar el diálogo manual durante la secuencia
        if (GameManager.IsEventActive && advanceDialogueAction != null && advanceDialogueAction.WasPressedThisFrame())
        {
            if (finalDialogueManager != null && finalDialogueManager.dialoguePanel.activeSelf)
            {
                finalDialogueManager.AdvanceManualDialogue(); // Llamada al nuevo Manager
            }
        }
    }

    // =============================================================
    // FLUJO DE TRANSICIÓN
    // =============================================================
    private IEnumerator TransitionSequence()
    {
        // 1. INICIO CINEMÁTICO
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetCinematicMode(true);
        }

        // 2. DIÁLOGO DE TRANSICIÓN (Manual)
        bool dialogueFinished = false;
        if (finalDialogueManager != null && finalDialogue.Length > 0)
        {
            // Inicia el diálogo manual usando el nuevo Manager
            finalDialogueManager.StartManualDialogue(finalDialogue, () => dialogueFinished = true);
            yield return new WaitUntil(() => dialogueFinished);
        }

        // 3. CIERRE DEL DIÁLOGO
        if (finalDialogueManager != null) finalDialogueManager.SetPanelActive(false);

        // 4. LIMPIEZA DE OBJETOS PERSISTENTES (CRÍTICO)
        CleanUpPersistentObjects();

        // 5. CARGA DE NIVEL
        SceneManager.LoadScene(nextSceneName);
    }

    // =============================================================
    // FUNCIÓN DE LIMPIEZA
    // =============================================================
    private void CleanUpPersistentObjects()
    {
        Debug.Log("Iniciando limpieza de Singletons persistentes...");

        // 1. Limpieza de Singletons ESPECÍFICOS (DDOL)

        // Limpieza de GameManager (asumiendo que tiene .Instance)
        if (PlayerController.Instance != null)
        {
            // La destrucción se realiza en el GameObject del Singleton
            Destroy(PlayerController.Instance.gameObject);
            Debug.Log("[CleanUp] Destruido: Player.");
        }

        // Limpieza de DecisionManager (asumiendo que tiene .Instance)
        if (DecisionManager.Instance != null)
        {
            Destroy(DecisionManager.Instance.gameObject);
            Debug.Log("[CleanUp] Destruido: DecisionManager.");
        }

        // Limpieza del SoundEffectManager (¡TUS OBJETOS!)
        // Asumiendo que ahora es público estático:
        

        // NOTA: No es necesario destruir SoundEffectLibrary por separado
        // si está en el mismo GameObject que SoundEffectManager.

        // 2. Limpieza de objetos LOCALES arrastrados al Inspector (Si se usaran

        // Importante: No llamar a GameManager.SetEventActive(false) después de destruirlo.
    }
}
