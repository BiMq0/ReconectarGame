using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine; 
public class RoomsManager : MonoBehaviour
{
    public static RoomsManager Instance { get; private set; }

    [Header("Referencias")]
    public Transition_Manager transitionManager;

    [Header("Configuración del Mapa")]
    [Tooltip("Nombres de las escenas de la mazmorra, en orden de izquierda a derecha.")]
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
        return 0; // Por defecto.
    }

    // =========================================================================
    // MÉTODO DE CONTROL DE TRANSICIÓN
    // =========================================================================

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
        // 0. BLOQUEAR CONTROLES
        GameManager.Instance.SetCinematicMode(true);

        // 1. FADE-IN (Pantalla Negra)
        yield return transitionManager.StartCoroutine(transitionManager.FadeIn());

        // 2. CARGA ASÍNCRONA DE LA NUEVA ESCENA
        currentSceneIndex = newIndex;
        string sceneName = dungeonScenes[newIndex];

        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName);

        // Espera a que la nueva escena termine de cargar
        while (!loadOperation.isDone)
        {
            yield return null;
        }

        TeleportPlayerToEntrance();
        ReEngageCinemachine(); // <-- ¡Engancha la CM de la nueva escena!

        yield return transitionManager.StartCoroutine(transitionManager.FadeOut());

        // 5. DEVOLVER CONTROLES
        GameManager.Instance.SetCinematicMode(false);
    }

    // =========================================================================
    // LÓGICA DE CINEMACHINE
    // =========================================================================

    private void ReEngageCinemachine()
    {
        if (playerTransform == null) return;

        CinemachineVirtualCamera vCam = FindObjectOfType<CinemachineVirtualCamera>();

        if (vCam != null)
        {
            // Asignar el Transform del jugador persistente al Follow y Look At de la VCam.
            vCam.Follow = playerTransform;
            vCam.LookAt = playerTransform;
            Debug.Log("Cinemachine re-enganchada al jugador en la nueva escena.");
        }
        else
        {
            Debug.LogWarning("No se encontró una Cinemachine Virtual Camera en la nueva escena.");
        }
    }

    // =========================================================================
    // LÓGICA DE TELETRANSPORTE
    // =========================================================================

    private void TeleportPlayerToEntrance()
    {
        if (playerTransform == null) return;

        // Determinar el Tag del Spawn Point:
        // Si avanzamos (1), aparecemos en el punto IZQUIERDO.
        // Si retrocedemos (-1), aparecemos en el punto DERECHO.
        string spawnTag = (spawnDirection > 0) ? "SpawnPointLeft" : "SpawnPointRight";

        GameObject spawnPointGO = GameObject.FindGameObjectWithTag(spawnTag);

        if (spawnPointGO != null)
        {
            playerTransform.position = spawnPointGO.transform.position;
            Debug.Log($"Jugador teletransportado a {spawnTag} en la escena {dungeonScenes[currentSceneIndex]}.");
        }
        else
        {
            Debug.LogError($"ERROR EN TELETRANSPORTE: No se encontró el punto de aparición con el Tag '{spawnTag}' en la escena: {dungeonScenes[currentSceneIndex]}.");
        }
    }
}