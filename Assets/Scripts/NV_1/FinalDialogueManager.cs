using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FinalDialogueManager : MonoBehaviour
{
    // =============================================================
    // ESTRUCTURAS DE DATOS
    // =============================================================
    [System.Serializable]
    public class DialogueLine
    {
        [TextArea(3, 5)]
        public string text;
    }

    // =============================================================
    // REFERENCIAS DE UI
    // =============================================================
    [Header("Referencias de UI")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;

    [Header("Ajustes de Diálogo")]
    public float typeSpeed = 50.0f; // Caracteres por segundo

    // =============================================================
    // ESTADO
    // =============================================================
    private Coroutine currentDialogueCoroutine = null;
    private Action onDialogueComplete = null;
    private Queue<DialogueLine> dialogueQueue;

    // =============================================================
    // CONTROL PÚBLICO
    // =============================================================

    private void Awake()
    {
        MusicManager.PlayBGM("final");
        SoundEffectManager.Play("Viento");
    }
    public void SetPanelActive(bool activate)
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(activate);
        }
    }

    public void StartManualDialogue(DialogueLine[] lines, Action onComplete)
    {
        StopAllCoroutines();
        currentDialogueCoroutine = null;
        onDialogueComplete = onComplete;

        dialogueQueue = new Queue<DialogueLine>(lines);
        AdvanceManualDialogue(); // Inicia la primera línea
    }

    public void AdvanceManualDialogue()
    {
        SoundEffectManager.Play("Click");
        if (currentDialogueCoroutine != null)
        {
            // Si el texto se está tipeando, muestra el texto completo de inmediato.
            StopCoroutine(currentDialogueCoroutine);
            currentDialogueCoroutine = null;
            if (dialogueText != null)
            {
                dialogueText.maxVisibleCharacters = dialogueText.text.Length;
            }
            return;
        }

        if (dialogueQueue != null && dialogueQueue.Count > 0)
        {
            DialogueLine nextLine = dialogueQueue.Dequeue();
            currentDialogueCoroutine = StartCoroutine(TypeLine(nextLine));
        }
        else
        {
            // Fin del diálogo manual
            SetPanelActive(false);
            onDialogueComplete?.Invoke();
        }
    }

    // =============================================================
    // LÓGICA INTERNA DE DIÁLOGO
    // =============================================================

    private IEnumerator TypeLine(DialogueLine line)
    {
        SetPanelActive(true);
        float charDelay = (typeSpeed > 0.0f) ? 1f / typeSpeed : 0.05f;

        dialogueText.text = line.text;
        dialogueText.maxVisibleCharacters = 0;

        while (dialogueText.maxVisibleCharacters < dialogueText.text.Length)
        {
            dialogueText.maxVisibleCharacters++;
            yield return new WaitForSeconds(charDelay);
        }

        currentDialogueCoroutine = null;
    }
}