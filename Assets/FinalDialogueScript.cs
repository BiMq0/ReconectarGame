using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class FinalDialogueScript : MonoBehaviour
{
    [System.Serializable]
    public class DialogueLine
    {
        [TextArea(2, 4)]
        public string text;
    }

    [Header("═══ REFERENCIAS UI ═══")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public Image icon;
    public Image fadeImage;

    [Header("═══ DIÁLOGOS ═══")]
    public DialogueLine[] dialogueLines;

    [Header("═══ TRANSICIONES ═══")]
    public float fadeDuration = 0.5f;
    public float typeSpeed = 0.01f;

    private bool isDisplaying = false;

    private void Start()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
        fadeImage.color = new Color(0, 0, 0, 1f);
        StartDialogue();
    }

    public void StartDialogue()
    {
        StartCoroutine(DisplayDialogueSequence());
    }

    private IEnumerator DisplayDialogueSequence()
    {
        isDisplaying = true;
        Debug.Log("[FinalDialogueScript] Iniciando secuencia de diálogos");

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
            Debug.Log("[FinalDialogueScript] Panel activado");
        }
        else
        {
            Debug.LogError("[FinalDialogueScript] dialoguePanel es null");
            yield break;
        }

        // Fade in una sola vez al inicio
        Debug.Log("[FinalDialogueScript] Iniciando fade in");
        yield return StartCoroutine(FadeImage(true, fadeDuration));

        // Mostrar cada línea de texto por click
        foreach (DialogueLine line in dialogueLines)
        {
            Debug.Log($"[FinalDialogueScript] Mostrando línea: {line.text.Substring(0, Mathf.Min(30, line.text.Length))}...");

            yield return StartCoroutine(TypeAndAdvance(line));
        }

        // Fade out al final
        Debug.Log("[FinalDialogueScript] Iniciando fade out");
        yield return StartCoroutine(FadeImage(false, fadeDuration));

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        isDisplaying = false;
        Debug.Log("[FinalDialogueScript] Diálogos completados");
    }

    private IEnumerator FadeImage(bool fadeIn, float duration)
    {
        if (fadeImage == null)
        {
            Debug.LogWarning("[FinalDialogueScript] fadeImage no asignado");
            yield break;
        }

        // Fade IN: Negro opaco → Blanco visible (se ve la imagen)
        // Fade OUT: Blanco visible → Negro opaco (desaparece)
        Color startColor = fadeIn ? new Color(0, 0, 0, 1f) : new Color(1, 1, 1, 1f);
        Color endColor = fadeIn ? new Color(1, 1, 1, 1f) : new Color(0, 0, 0, 1f);

        fadeImage.color = startColor;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            fadeImage.color = Color.Lerp(startColor, endColor, t);
            yield return null;
        }

        fadeImage.color = endColor;
        Debug.Log($"[FinalDialogueScript] Fade {(fadeIn ? "IN" : "OUT")} completado");
    }
    private IEnumerator TypeText(string text)
    {
        if (dialogueText == null) yield break;

        dialogueText.text = "";
        for (int i = 0; i < text.Length; i++)
        {
            dialogueText.text += text[i];
            yield return new WaitForSeconds(typeSpeed);
        }
    }

    private IEnumerator TypeAndAdvance(DialogueLine line)
    {
        // Escribir texto
        yield return StartCoroutine(TypeText(line.text));

        // Esperar click
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));

        yield return new WaitForSeconds(0.2f);
    }
}

