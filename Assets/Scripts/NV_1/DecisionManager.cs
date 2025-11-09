using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DecisionManager : MonoBehaviour
{
    // =============================================================
    // SINGLETON (CORRECCIÓN CLAVE)
    // =============================================================
    public static DecisionManager Instance { get; private set; }

    private void Awake()
    {
        // Implementación de Singleton para acceso estático en la escena
        if (Instance != null && Instance != this)
        {
            // Destruye si ya existe otra instancia
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            // NOTA: Se ha quitado DontDestroyOnLoad() según tu petición.
        }
    }

    // =============================================================
    // REFERENCIAS
    // =============================================================
    [Tooltip("Panel principal que contiene los botones.")]
    public GameObject decisionPanel;

    [Tooltip("Los botones de opción (deben estar en orden: Opción 1, Opción 2, Opción 3).")]
    public Button[] choiceButtons = new Button[3];

    private Action<int> onDecisionMade;

    void Start()
    {
        // Ocultar el panel de inicio
        if (decisionPanel != null) decisionPanel.SetActive(false);

        // Asignar listeners a los botones
        for (int i = 0; i < choiceButtons.Length; i++)
        {
            if (choiceButtons[i] != null)
            {
                int choice = i + 1; // El valor de elección será 1, 2, 3

                // Asegura que no se añadan listeners múltiples si se llama varias veces (aunque no debería pasar en Start)
                choiceButtons[i].onClick.RemoveAllListeners();

                // Asignación funcional del listener
                choiceButtons[i].onClick.AddListener(() => OnChoiceSelected(choice));
            }
            else
            {
                Debug.LogError($"DecisionManager: Botón en el índice {i} no asignado en el Inspector.");
            }
        }
    }

    // =============================================================
    // LÓGICA DE DECISIÓN
    // =============================================================
    public IEnumerator WaitForDecision(string[] dialogueOptions, Action<int> callback)
    {
        if (decisionPanel == null)
        {
            Debug.LogError("DecisionPanel es nulo. Asigna el panel contenedor de botones en el Inspector.");
            yield break;
        }

        onDecisionMade = callback;

        // Configurar los botones
        for (int i = 0; i < choiceButtons.Length; i++)
        {
            if (choiceButtons[i] == null) continue;

            if (i < dialogueOptions.Length)
            {
                // Busca el componente TextMeshProUGUI en el botón o sus hijos
                TextMeshProUGUI buttonText = choiceButtons[i].GetComponentInChildren<TextMeshProUGUI>(true);
                if (buttonText != null)
                {
                    buttonText.text = dialogueOptions[i];
                }
                choiceButtons[i].gameObject.SetActive(true);
            }
            else
            {
                choiceButtons[i].gameObject.SetActive(false);
            }
        }

        // Muestra el panel y espera a que la decisión sea tomada
        decisionPanel.SetActive(true);

        // Espera a que el delegado onDecisionMade sea limpiado por OnChoiceSelected
        yield return new WaitUntil(() => onDecisionMade == null);

        // Oculta el panel al terminar
        decisionPanel.SetActive(false);
    }

    private void OnChoiceSelected(int choice)
    {
        // Se llama cuando se hace clic en un botón
        if (onDecisionMade != null)
        {
            onDecisionMade.Invoke(choice); // Ejecuta el callback en KidEvent.cs
            onDecisionMade = null;        // Limpia el delegado para que WaitUntil en WaitForDecision() termine
        }
    }
}