using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System; // Necesario para usar 'Action'

public class JugoDialogueManager : MonoBehaviour
{
    // =============================================================
    // ESTRUCTURAS DE DATOS
    // =============================================================
    public enum Speaker { Nayeli = 0, Raymi = 1, NPC = 2 }

    [System.Serializable]
    public class DialogueLine
    {
        public Speaker speaker = Speaker.NPC;
        [TextArea(3, 5)]
        public string text;
        [Tooltip("Índice del sprite en el array correspondiente.")]
        public int expressionIndex = 0;
    }
    // =============================================================
    // REFERENCIAS DE UI Y PERSONAJES
    // =============================================================
    [Header("Referencias de UI Principal")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public Image icon; // Para Raymi y NPC
    public Sprite[] carasSprites;

    [Header("Referencias de Nayeli (Dueña)")]
    public Image nayeliImage;
    public Sprite[] nayeliExpressions;

    [Header("Panel de Input (Fase 3)")]
    public GameObject inputPanel;
    public TMP_InputField inputField;

    // =============================================================
    // AJUSTES DE DIÁLOGO
    // =============================================================
    [Header("Ajustes de Diálogo")]
    [Tooltip("Velocidad de tipeo en CARACTERES POR SEGUNDO (ej: 20.0).")]
    public float typeSpeed = 20.0f;
    public float autoPauseDuration = 1.5f;

    private Coroutine currentDialogueCoroutine = null;
    private Action onDialogueComplete = null;

    // =============================================================
    // MÉTODOS PÚBLICOS DE CONTROL
    // =============================================================

    public Coroutine StartAutomaticDialogue(DialogueLine[] lines, Action onComplete = null)
    {
        if (lines == null || lines.Length == 0)
        {
            onComplete?.Invoke();
            return null;
        }

        StopCurrentDialogue();
        onDialogueComplete = onComplete;
        currentDialogueCoroutine = StartCoroutine(RunDialogue(lines));
        return currentDialogueCoroutine;
    }

    public void StopCurrentDialogue()
    {
        if (currentDialogueCoroutine != null)
        {
            StopCoroutine(currentDialogueCoroutine);
            currentDialogueCoroutine = null;
        }
        SetPanelActive(false);
        ActivateInputPanel(false);

        if (icon != null) icon.gameObject.SetActive(false);
    }

    public void SetPanelActive(bool active)
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(active);
        }
    }

    public void ActivateInputPanel(bool activate)
    {
        if (inputPanel != null)
        {
            inputPanel.SetActive(activate);
            if (activate && inputField != null)
            {
                inputField.text = "";
                inputField.ActivateInputField();
            }
        }
    }

    // =============================================================
    // LÓGICA DE LA MÁQUINA DE ESCRIBIR
    // =============================================================
    private IEnumerator RunDialogue(DialogueLine[] lines)
    {
        SetPanelActive(true);

        float charDelay = 0.05f;
        if (typeSpeed > 0.0f)
        {
            charDelay = 1f / typeSpeed;
        }

        foreach (DialogueLine line in lines)
        {
            if (icon != null && carasSprites != null && line.expressionIndex >= 0)
            {
                int spriteIndex = line.expressionIndex;

                if (spriteIndex < carasSprites.Length)
                {
                    icon.gameObject.SetActive(true);
                    icon.sprite = carasSprites[spriteIndex];
                }
                else
                {
                    // Si el índice configurado es inválido
                    icon.gameObject.SetActive(false);
                    Debug.LogWarning($"El índice de expresión {line.expressionIndex} está fuera de rango para el array carasSprites.");
                }
            }
            else if (icon != null)
            {
                icon.gameObject.SetActive(false);
            }
            dialogueText.text = "";

            for (int i = 0; i < line.text.Length; i++)
            {
                dialogueText.text += line.text[i];
                yield return new WaitForSeconds(charDelay);
            }
            yield return new WaitForSeconds(autoPauseDuration);
        }

        onDialogueComplete?.Invoke();
        currentDialogueCoroutine = null;
    }
}