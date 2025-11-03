using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            decisionPanel.SetActive(false);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        for (int i = 0; i < choiceButtons.Length; i++)
        {
            int choice = i + 1; 
            choiceButtons[i].onClick.AddListener(() => OnChoiceSelected(choice));
        }
    }

    public IEnumerator WaitForDecision(string[] dialogueOptions, Action<int> callback)
    {
        onDecisionMade = callback;

        for (int i = 0; i < choiceButtons.Length; i++)
        {
            if (i < dialogueOptions.Length)
            {
                choiceButtons[i].GetComponentInChildren<Text>().text = dialogueOptions[i];
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
