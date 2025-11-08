using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class DialogosEventDogAttack : MonoBehaviour
{
    public enum Speaker { Doggo = 0, Raymi = 1 }

    [System.Serializable]
    public class DialogueLine
    {
        public Speaker speaker = Speaker.Doggo;
        [TextArea(3, 5)]
        public string text;
        public int iconIndex = 0;
    }

    [Header("Referencias de UI")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public Image icon;
    public Sprite[] caras;

    [Header("Diálogos")]
    public DialogueLine[] dialogueLinesBeforeAttack;
    public DialogueLine[] dialogueLinesAfterAttack;

    [Header("Ajustes")]
    public float typeSpeed = 0.05f;
    public float linePauseDuration = 1.5f;

    [Header("Referencias")]
    private DogAttackEvent dogAttackEvent;
    private bool isTyping = false;
    private bool dialogueCompleted = false;

    private void Start()
    {
        dogAttackEvent = GetComponentInParent<DogAttackEvent>();
    }

    public void StartDogAttackEvent()
    {
        StartCoroutine(DogAttackEventSequence());
    }

    private IEnumerator DogAttackEventSequence()
    {
        // Congelar al jugador con GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetCinematicMode(true);
        }

        // Mostrar diálogos antes del ataque
        yield return StartCoroutine(RunDialogue(dialogueLinesBeforeAttack));

        // Iniciar ataque del perro
        if (dogAttackEvent != null)
        {
            dogAttackEvent.animator.SetBool("isPlayerInScene", true);
            dogAttackEvent.StartAttackSequence();
        }

        // Esperar a que se completen los 3 ataques
        yield return new WaitUntil(() => dogAttackEvent.attackCount >= dogAttackEvent.maxAttackRepetitions);

        // Mostrar diálogos después del ataque
        yield return StartCoroutine(RunDialogue(dialogueLinesAfterAttack));

        // Desbloquear controles
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetCinematicMode(false);
        }

        Debug.Log("Evento de perro completado");
    }

    private IEnumerator RunDialogue(DialogueLine[] lines)
    {
        if (lines == null || lines.Length == 0)
        {
            Debug.LogWarning("Array de diálogo vacío");
            yield break;
        }

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        foreach (DialogueLine line in lines)
        {
            if (caras != null && line.iconIndex >= 0 && line.iconIndex < caras.Length)
            {
                icon.sprite = caras[line.iconIndex];
            }

            isTyping = true;
            dialogueText.text = "";

            // Escribir el texto letra por letra
            for (int i = 0; i < line.text.Length; i++)
            {
                dialogueText.text += line.text[i];
                yield return new WaitForSeconds(typeSpeed);
            }

            isTyping = false;

            // Esperar a que el jugador haga click para continuar
            dialogueCompleted = false;
            yield return new WaitUntil(() => dialogueCompleted);

            yield return new WaitForSeconds(0.2f);
        }

        dialogueText.text = "";
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        Debug.Log("Diálogos completados");
    }

    private void Update()
    {
        // Detectar click para avanzar diálogo
        if (!isTyping && Input.GetMouseButtonDown(0))
        {
            dialogueCompleted = true;
        }
    }
}
