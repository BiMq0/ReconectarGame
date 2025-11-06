using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro; 
public class DecisionManager : MonoBehaviour
{
    public static DecisionManager Instance { get; private set; }

    [Tooltip("Panel principal que contiene los botones.")]
    public GameObject decisionPanel;

    [Tooltip("Los botones de opción (deben estar en orden: Opción 1, Opción 2, Opción 3).")]
    public Button[] choiceButtons = new Button[3];

    private Action<int> onDecisionMade;

    void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            if (decisionPanel != null)
            {
                decisionPanel.SetActive(false);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Asignar listeners a los botones
        for (int i = 0; i < choiceButtons.Length; i++)
        {
            if (choiceButtons[i] != null)
            {
                int choice = i + 1; // 1, 2, 3
                choiceButtons[i].onClick.AddListener(() => OnChoiceSelected(choice));
            }
            else
            {
                Debug.LogError($"DecisionManager: Botón en el índice {i} no asignado en el Inspector.");
            }
        }
    }

    public IEnumerator WaitForDecision(string[] dialogueOptions, Action<int> callback)
    {
        if (decisionPanel == null)
        {
            Debug.LogError("DecisionPanel es nulo. Asigna el panel contenedor de botones en el Inspector.");
            yield break;
        }

        onDecisionMade = callback;

        for (int i = 0; i < choiceButtons.Length; i++)
        {
            if (choiceButtons[i] == null) continue;

            if (i < dialogueOptions.Length)
            {
                TextMeshProUGUI buttonText = choiceButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = dialogueOptions[i];
                }
                else
                {
                    Debug.LogWarning($"El botón {i} no tiene un componente TextMeshProUGUI hijo.");
                }
                choiceButtons[i].gameObject.SetActive(true);
            }
            else
            {
                choiceButtons[i].gameObject.SetActive(false);
            }
        }

        decisionPanel.SetActive(true);

        yield return new WaitUntil(() => onDecisionMade == null);

        decisionPanel.SetActive(false);
    }

    private void OnChoiceSelected(int choice)
    {
        if (onDecisionMade != null)
        {
            onDecisionMade.Invoke(choice);
            onDecisionMade = null;
        }
    }
}