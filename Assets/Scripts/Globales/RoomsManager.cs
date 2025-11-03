using System; 
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;

[System.Serializable]
public class LevelConfig
{
    [Tooltip("ID único para este nivel.")]
    public string levelID;
    [Tooltip("Nombres de las escenas de este nivel, en orden.")]
    public string[] sceneNames;
    [Tooltip("El índice de la escena que se cargará al iniciar el nivel (ej: 0).")]
    public int startSceneIndex = 0;
}

public class RoomsManager : MonoBehaviour
{
    public static RoomsManager Instance { get; private set; }

    [Header("Referencias")]
    public Transition_Manager transitionManager;
    private Transform playerTransform;

    [Header("Configuración de Niveles")]
    [Tooltip("Define cada nivel con su ID y su lista de escenas.")]
    public LevelConfig[] allLevels;

    // Estado actual del Manager
    private LevelConfig currentLevelConfig;
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
        }
        string activeSceneName = SceneManager.GetActiveScene().name;
        currentLevelConfig = Array.Find(allLevels, level => Array.Exists(level.sceneNames, name => name == activeSceneName));

        if (currentLevelConfig != null)
        {
            currentSceneIndex = Array.IndexOf(currentLevelConfig.sceneNames, activeSceneName);
        }
        else if (allLevels.Length > 0)
        {
            currentLevelConfig = allLevels[0];
            currentSceneIndex = 0;
        }
        else
        {
            Debug.LogError("RoomsManager: No hay niveles configurados en el Inspector.");
        }
    }

    // =============================================================
    // MÉTODO NUEVO: CAMBIO DE NIVEL COMPLETO
    // =============================================================
    public void ChangeLevel(string newLevelID)
    {
        LevelConfig nextConfig = Array.Find(allLevels, level => level.levelID == newLevelID);

        if (nextConfig != null)
        {
            currentLevelConfig = nextConfig;

            spawnDirection = 1;
            StartCoroutine(LoadNewRoomSequence(currentLevelConfig.startSceneIndex));
        }
        else
        {
            Debug.LogError($"RoomsManager: Nivel ID '{newLevelID}' no encontrado en la configuración.");
        }
    }

    public void GoToRoom(int direction)
    {
        if (currentLevelConfig == null) { Debug.LogError("RoomsManager: No hay nivel activo."); return; }

        int nextIndex = currentSceneIndex + direction;

        if (nextIndex >= 0 && nextIndex < currentLevelConfig.sceneNames.Length)
        {
            spawnDirection = direction;
            StartCoroutine(LoadNewRoomSequence(nextIndex));
        }
        else
        {
            Debug.Log($"ALCANCE EL TOPE del Nivel '{currentLevelConfig.levelID}'.");
        }
    }

    private IEnumerator LoadNewRoomSequence(int newIndex)
    {
        if (GameManager.Instance != null) GameManager.Instance.SetCinematicMode(true);

        if (transitionManager != null) yield return transitionManager.StartCoroutine(transitionManager.FadeIn());

        currentSceneIndex = newIndex;
        string sceneName = currentLevelConfig.sceneNames[newIndex];

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

    // --- TeleportPlayerToEntrance (Se mantiene igual) ---
    private void TeleportPlayerToEntrance()
    {
        string spawnTag = spawnDirection == 1 ? "SpawnPointLeft" : "SpawnPointRight";

        GameObject spawnPoint = GameObject.FindGameObjectWithTag(spawnTag);

        if (playerTransform != null && spawnPoint != null)
        {
            playerTransform.position = spawnPoint.transform.position;
            Debug.Log($"Teleportado a: {spawnTag} en la escena {currentSceneIndex}.");
        }
        else
        {
            Debug.LogError($"No se pudo encontrar el punto de aparición con la etiqueta: {spawnTag}");
        }
    }

    private void ReEngageCinemachine()
    {
        CinemachineVirtualCamera vCam = FindObjectOfType<CinemachineVirtualCamera>();
        if (vCam != null && playerTransform != null)
        {
            vCam.Follow = playerTransform;
        }
    }

    public (int currentIndex, int totalScenes) GetMapState()
    {
        if (currentLevelConfig == null) return (0, 0);
        return (currentSceneIndex, currentLevelConfig.sceneNames.Length);
    }
}