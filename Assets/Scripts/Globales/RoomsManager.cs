using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine; // Importante para controlar las cámaras virtuales

public class RoomsManager : MonoBehaviour
{
    public static RoomsManager Instance { get; private set; }

    [Header("Referencias")]
    public Transition_Manager transitionManager;

    [Header("Configuración del Mapa")]
    [Tooltip("Nombres de las scene del nivel, en orden de izquierda a derecha.")]
    public string[] dungeonScenes;

    private Transform playerTransform;
    private int currentSceneIndex = 0;
    private int spawnDirection = 1;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null)
        {
            playerTransform = playerGO.transform;
            currentSceneIndex = GetCurrentSceneIndex();
        }
        else
        {
            Debug.LogError("FATAL: El jugador no está taggeado como 'Player' o no existe al inicio.");
        }
    }
    private int GetCurrentSceneIndex()
    {
        string currentName = SceneManager.GetActiveScene().name;
        for (int i = 0; i < dungeonScenes.Length; i++)
        {
            if (dungeonScenes[i] == currentName)
            {
                return i;
            }
        }
        return 0; // Por defecto
    }
    public (int currentIndex, int totalScenes) GetMapState()
    {
        return (currentSceneIndex, dungeonScenes.Length);
    }
    public void GoToRoom(int direction)
    {
        int nextIndex = currentSceneIndex + direction;

        if (nextIndex >= 0 && nextIndex < dungeonScenes.Length)
        {
            spawnDirection = direction;
            StartCoroutine(LoadNewRoomSequence(nextIndex));
        }
        else
        {
            Debug.Log($"ALCANCE EL TOPE. Dirección: {direction}. Índice: {nextIndex}");
        }
    }

    private IEnumerator LoadNewRoomSequence(int newIndex)
    {
        if (GameManager.Instance != null) GameManager.Instance.SetCinematicMode(true);

        if (transitionManager != null) yield return transitionManager.StartCoroutine(transitionManager.FadeIn());

        currentSceneIndex = newIndex;
        string sceneName = dungeonScenes[newIndex];
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName);

        while (!loadOperation.isDone)
        {
            yield return null;
        }

        TeleportPlayerToEntrance();
        ReEngageCinemachine();

        if (transitionManager != null) yield return transitionManager.StartCoroutine(transitionManager.FadeOut());

        if (GameManager.Instance != null) GameManager.Instance.SetCinematicMode(false);
    }

    private void ReEngageCinemachine()
    {
        if (playerTransform == null) return;

        // Busca la Virtual Camera en la escena que acaba de cargarse.
        CinemachineVirtualCamera vCam = FindObjectOfType<CinemachineVirtualCamera>();

        if (vCam != null)
        {
            // Asigna el Transform del jugador persistente.
            vCam.Follow = playerTransform;
            vCam.LookAt = playerTransform;
        }
    }

    private void TeleportPlayerToEntrance()
    {
        if (playerTransform == null) return;

        string spawnTag = (spawnDirection > 0) ? "SpawnPointLeft" : "SpawnPointRight";

        GameObject spawnPointGO = GameObject.FindGameObjectWithTag(spawnTag);

        if (spawnPointGO != null)
        {
            playerTransform.position = spawnPointGO.transform.position;
        }
        else
        {
            Debug.LogError($"ERROR: No se encontró el punto de aparición con el Tag '{spawnTag}'.");
        }
    }
}