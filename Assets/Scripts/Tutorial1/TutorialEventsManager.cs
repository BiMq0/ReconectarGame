using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TutorialEventsManager : MonoBehaviour
{
    public enum Speaker { Doggo = 0, Raymi = 1 }

    [System.Serializable]
    public class DialogueLine
    {
        [Tooltip("Quién habla?")]
        public Speaker speaker = Speaker.Doggo;
        [TextArea(3, 5)]
        public string text;
        [Tooltip("Índice del sprite en 'icon'.")]
        public int iconIndex = 0;
    }
    private bool isTyping = false; 

    [Header("Referencias de UI")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public Image icon;
    public Sprite[] caras; 

    [Header("Flujo de Diálogo")]
    public DialogueLine[] introductionDialogue;

    [Header("Ajustes de Diálogo")]
    public float typeSpeed = 0.05f; 
    [Tooltip("Tiempo de pausa después de cada línea (en segundos).")]
    public float linePauseDuration = 1.5f;

    private void Awake()
    {
        Transition_Manager.Instance.FadeOut();
        MusicManager.PlayBGM("tutorial",false);
    }
    public void ActivarDialogo()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true); 
        }
        StartCoroutine(RunDialogue(introductionDialogue));
    }
    private IEnumerator RunDialogue(DialogueLine[] lines)
    {
        if (lines == null || lines.Length == 0)
        {
            Debug.LogWarning("El array de diálogo está vacío. Terminando diálogo.");
            dialoguePanel.SetActive(false);
            yield break;
        }
        foreach (DialogueLine line in lines)
        {
            if (caras != null && line.iconIndex >= 0 && line.iconIndex < caras.Length)
            {
                icon.sprite = caras[line.iconIndex];
            }
            else
            {
                Debug.LogWarning($"Ícono de personaje no encontrado para el índice {line.iconIndex}.");
                icon.sprite = null; 
            }

            isTyping = true;
            dialogueText.text = ""; 

            for (int i = 0; i < line.text.Length; i++)
            {
                dialogueText.text += line.text[i];
                yield return new WaitForSeconds(typeSpeed);
            }

            isTyping = false;
           
            yield return new WaitForSeconds(linePauseDuration);
        }

        dialogueText.text = ""; 
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false); 
        }

        Debug.Log("¡Diálogo completado automáticamente!");
    }
}
