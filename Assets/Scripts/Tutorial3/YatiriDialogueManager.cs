using System; 
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class YatiriDialogueManager : MonoBehaviour
{
    public enum Speaker { Yatiri = 0, Raymi = 1 }

    [System.Serializable]
    public class DialogueLine
    {
        public Speaker speaker = Speaker.Yatiri;
        [TextArea(3, 5)]
        public string text;
        public int expressionIndex = 0;
    }

    [Header("Referencias de UI")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public Image icon;
    public Sprite[] carasNPC;

    [Header("Ajustes de Diálogo")]
    public float typeSpeed = 0.05f;
    public float autoPauseDuration = 1.5f;

    private InputAction dialogueAdvanceAction;
    private bool dialogueIsRunning = false;
    private bool isTyping = false;
    private System.Action onDialogueCompleteCallback; 

    public void Initialize(InputAction advanceAction)
    {
        dialogueAdvanceAction = advanceAction;
    }

    public void StartDialogue(DialogueLine[] lines, bool isManualAdvance, System.Action onComplete = null)
    {
        if (dialogueIsRunning) return;

        onDialogueCompleteCallback = onComplete;

        dialogueIsRunning = true;
        dialoguePanel.SetActive(true);
        StartCoroutine(RunDialogue(lines, isManualAdvance));
    }

    private IEnumerator RunDialogue(DialogueLine[] lines, bool isManualAdvance)
    {
        if (lines == null || lines.Length == 0)
        {
            EndDialogue();
            yield break;
        }

        foreach (DialogueLine line in lines)
        {
            if (icon != null && carasNPC != null && line.expressionIndex < carasNPC.Length)
            {
                icon.sprite = carasNPC[line.expressionIndex];
            }

            isTyping = true;
            dialogueText.text = "";
            for (int i = 0; i < line.text.Length; i++)
            {
                dialogueText.text += line.text[i];
                yield return new WaitForSeconds(typeSpeed);
            }
            isTyping = false;

            if (isManualAdvance)
            {
                if (dialogueAdvanceAction == null)
                {
                    Debug.LogError("Error: Acción de avance no bindeada en el Dialogue Manager.");
                    break;
                }
                yield return new WaitUntil(() => dialogueAdvanceAction.WasPressedThisFrame());
            }
            else
            {
                // FLUJO 1 (AUTOMÁTICO): Avance por tiempo.
                yield return new WaitForSeconds(autoPauseDuration);
            }
        }
        EndDialogue();
    }

    private void EndDialogue()
    {
        dialogueText.text = "";
        dialoguePanel.SetActive(false);
        dialogueIsRunning = false;

        if (onDialogueCompleteCallback != null)
        {
            onDialogueCompleteCallback.Invoke();
            onDialogueCompleteCallback = null;
        }
    }

    public void SaveTuto3()
    {
        GameManager.Instance.SaveGame();
    }
}