using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroManager : MonoBehaviour
{
    [Header("Referencias de UI/Flujo")]
    [Tooltip("El botón de 'Saltar Cinemática' que solo aparece en la segunda o más veces.")]
    public GameObject skipButton;

    [Tooltip("La escena de tutorial a la que va el jugador la primera vez.")]
    public string tutorialSceneName = "Tutorial";

    [Tooltip("La escena del menú principal a donde va tras la intro o skip.")]
    public string mainMenuSceneName = "MainMenuScene";
    [Tooltip("Duración TOTAL de la cinemática sin el skip.")]
    public float cinematicDuration = 58f; 

    private bool cinematicActive = true;

    void Awake()
    {
        if (skipButton != null)
        {
            skipButton.SetActive(false);
        }
    }

    void Start()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("ERROR FATAL: GameManager no está inicializado. ¡Asegúrate de que está en la escena anterior o de que su Awake() es lo primero en ejecutarse!");
            SceneManager.LoadScene(mainMenuSceneName); // Ruta de emergencia
            return;
        }

        bool isFirstTime = GameManager.Instance.IsFirstTimePlayer();

        if (isFirstTime)
        {
            Debug.Log("Jugador nuevo detectado. Ejecutando cinemática completa.");
            GameManager.Instance.SetCinematicMode(true);
            StartCoroutine(StartFullCinematic());
        }
        else
        {
            Debug.Log("Jugador veterano. Botón de skip visible.");
            if (skipButton != null)
            {
                skipButton.SetActive(true);
            }
            GameManager.Instance.SetCinematicMode(true);
            StartCoroutine(StartFullCinematic());
        }
    }
    private IEnumerator StartFullCinematic()
    {
        float timer = 0f;
        while (timer < cinematicDuration && cinematicActive)
        {
            yield return null;
            timer += Time.deltaTime;
        }

        if (cinematicActive)
        {
            OnCinematicFinished();
        }
    }

    public void SkipCinematic()
    {
        if (!cinematicActive) return;

        Debug.Log("Cinemática saltada por el jugador.");
        cinematicActive = false; // Detiene el bucle de StartFullCinematic

        // Usar RoomsManager si estuviera disponible, si no, carga directa:
        GameManager.Instance.SetCinematicMode(false);
        GameManager.Instance.SkipIntro(); // Carga MainMenuScene
    }

    private void OnCinematicFinished()
    {
        cinematicActive = false;

        // Desactivar el modo cinemático
        GameManager.Instance.SetCinematicMode(false);

        // Decidir la siguiente escena (Tutorial o Main Menu)
        if (GameManager.Instance.IsFirstTimePlayer())
        {
            GameManager.Instance.MarkGameStarted();

            // 2. Cargamos el tutorial.
            SceneManager.LoadScene(tutorialSceneName);
        }
        else
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}