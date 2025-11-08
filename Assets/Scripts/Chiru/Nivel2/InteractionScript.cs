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

    [Header(" DIÁLOGOS ")]
    public DialogueLine[] dialogueLines;

    [Header(" REFERENCIAS UI ")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public Image icon;

    [Header(" ICONO DEL NPC ")]
    public Sprite npcIcon;

    [Header(" AJUSTES ")]
    public float typeSpeed = 0.05f;
    public float interactionDistance = 2f;

    private bool playerNear = false;
    private bool isDialoguing = false;
    private bool isTyping = false;
    private bool dialogueCompleted = false;
    private Transform playerTransform;

    private void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (playerTransform == null)
        {
        }

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

    }

    private void Update()
    {
        if (playerTransform != null)
        {
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            playerNear = distance <= interactionDistance;
        }

        if (playerNear && Input.GetKeyDown(KeyCode.E) && !isDialoguing)
        {
            StartCoroutine(ShowDialogue());
        }

        if (!isTyping && isDialoguing && Input.GetMouseButtonDown(0))
        {
            dialogueCompleted = true;
        }
    }

    private IEnumerator ShowDialogue()
    {
        isDialoguing = true;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        if (icon != null && npcIcon != null)
        {
            icon.sprite = npcIcon;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetCinematicMode(true);
        }

        foreach (DialogueLine line in dialogueLines)
        {
            isTyping = true;
            dialogueText.text = "";

            for (int i = 0; i < line.text.Length; i++)
            {
                dialogueText.text += line.text[i];
                yield return new WaitForSeconds(typeSpeed);
            }

            isTyping = false;

            dialogueCompleted = false;
            yield return new WaitUntil(() => dialogueCompleted);

            yield return new WaitForSeconds(0.2f);
        }

        dialogueText.text = "";
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetCinematicMode(false);
        }

        isDialoguing = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}