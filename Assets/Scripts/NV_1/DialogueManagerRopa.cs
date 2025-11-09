using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class DialogueManagerRopa : MonoBehaviour
{
    // =============================================================
    // ESTRUCTURAS DE DATOS
    // =============================================================
    public enum Speaker { Vendedora = 0, Raymi = 1}

    [System.Serializable]
    public class DialogueLine
    {
        public Speaker speaker = Speaker.Vendedora;
        [TextArea(3, 5)]
        public string text;
        [Tooltip("Índice LITERAL del sprite de carita en el array 'carasSprites'.")]
        public int expressionIndex = 0;
    }

    // =============================================================
    // REFERENCIAS DE UI Y DIÁLOGOS
    // =============================================================
    [Header("Referencias de UI Principal")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public Image icon; 
    public Sprite[] carasSprites;

    [Header("Ajustes de Diálogo")]
    public float typeSpeed = 20.0f;
    public float autoPauseDuration = 1.5f;

    [Header("Flujos de Diálogo del Evento (Asignar en Inspector)")]
    public DialogueLine[] introductionDialogue;
    public DialogueLine[] interactionDialogue; 
    public DialogueLine[] eventDialogue;       
    public DialogueLine[] victoryDialogue;  

    // =============================================================
    // ESTADO
    // =============================================================
    private Coroutine currentDialogueCoroutine = null;
    private Action onDialogueComplete = null;
    private bool isAwaitingManualInput = false;
    private Queue<DialogueLine> manualDialogueQueue;

    // =============================================================
    // CONTROL PÚBLICO
    // =============================================================

    public void SetPanelActive(bool activate)
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(activate);
            if (!activate && icon != null) icon.gameObject.SetActive(false);
        }
    }

    public void StopCurrentDialogue()
    {
        if (currentDialogueCoroutine != null)
        {
            StopCoroutine(currentDialogueCoroutine);
            currentDialogueCoroutine = null;
        }
        SetPanelActive(false);
        isAwaitingManualInput = false;
        manualDialogueQueue = null;
    }

    // Inicia diálogo automático (pausa por tiempo)
    public void StartAutomaticDialogue(DialogueLine[] lines, Action onComplete)
    {
        StopCurrentDialogue();
        onDialogueComplete = onComplete;
        isAwaitingManualInput = false;
        currentDialogueCoroutine = StartCoroutine(RunDialogue(lines));
    }

    // Inicia diálogo manual (pausa por input del jugador)
    public void StartManualDialogue(DialogueLine[] lines, Action onComplete)
    {
        StopCurrentDialogue();
        onDialogueComplete = onComplete;
        isAwaitingManualInput = true;

        manualDialogueQueue = new Queue<DialogueLine>(lines);
        AdvanceManualDialogue(); 
    }

    // Llamado por el script principal (TiendaRopaEvent) al presionar "Interactuar"
    public void AdvanceManualDialogue()
    {
        if (!isAwaitingManualInput) return;

        if (currentDialogueCoroutine != null && dialogueText.maxVisibleCharacters < dialogueText.text.Length)
        {
            StopCoroutine(currentDialogueCoroutine);
            currentDialogueCoroutine = null;
            dialogueText.maxVisibleCharacters = dialogueText.text.Length;
            return;
        }

        if (manualDialogueQueue != null && manualDialogueQueue.Count > 0)
        {
            DialogueLine nextLine = manualDialogueQueue.Dequeue();
            currentDialogueCoroutine = StartCoroutine(TypeLine(nextLine));
        }
        else
        {
            StopCurrentDialogue();
            onDialogueComplete?.Invoke();
        }
    }

    // =============================================================
    // LÓGICA INTERNA DE DIÁLOGO
    // =============================================================
    private IEnumerator RunDialogue(DialogueLine[] lines)
    {
        SetPanelActive(true);

        foreach (DialogueLine line in lines)
        {
            yield return StartCoroutine(TypeLine(line));
            if (!isAwaitingManualInput)
            {
                yield return new WaitForSeconds(autoPauseDuration);
            }
        }
        StopCurrentDialogue();
        onDialogueComplete?.Invoke();
    }
    private IEnumerator TypeLine(DialogueLine line)
    {
        SetPanelActive(true);
        float charDelay = (typeSpeed > 0.0f) ? 1f / typeSpeed : 0.05f;

        if (icon != null && carasSprites != null && line.expressionIndex >= 0 && line.expressionIndex < carasSprites.Length)
        {
            icon.gameObject.SetActive(true);
            icon.sprite = carasSprites[line.expressionIndex];
        }
        else if (icon != null)
        {
            icon.gameObject.SetActive(false);
        }
        dialogueText.text = line.text;
        dialogueText.maxVisibleCharacters = 0;

        while (dialogueText.maxVisibleCharacters < dialogueText.text.Length)
        {
            dialogueText.maxVisibleCharacters++;
            yield return new WaitForSeconds(charDelay);
        }
    }
}