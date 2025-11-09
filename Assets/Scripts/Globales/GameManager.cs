using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public static event Action<bool> CambioEstadoControles;
    public static bool IsEventActive { get; private set; } = false;

    private HashSet<string> completedEvents = new HashSet<string>();

    // Constantes de Guardado
    private const string SceneSaveKey = "LastVisitedSceneName";
    private const string EventsSaveKey = "CompletedEventsIDs";

    // posición del jugador:
    private const string PosXSaveKey = "PlayerPosX";
    private const string PosYSaveKey = "PlayerPosY";
    private const string PosZSaveKey = "PlayerPosZ";

    // Variables de estado de carga
    public static bool IsLoadingGame { get; private set; } = false; // Indica si estamos en un proceso de carga de partida

    // NUEVA Referencia para usar en SaveGame()
    private Transform playerTransform;

    private const char Separator = ',';


    //Verificar si es la primera vez entrando al juego;
    private const string FirstTimePlayerKey = "IsFirstTimePlayer";



    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            //ClearSavedData(); // Descomentar solo si estás en desarrollo
            LoadGame();
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
    }

    public bool IsFirstTimePlayer()
    {
        // Si la clave NO existe, es la primera vez.
        return !PlayerPrefs.HasKey(FirstTimePlayerKey);
    }
    public void MarkGameStarted()
    {
        if (!PlayerPrefs.HasKey(FirstTimePlayerKey))
        {
            PlayerPrefs.SetInt(FirstTimePlayerKey, 1);
            PlayerPrefs.Save();
            Debug.Log("Juego marcado como iniciado. Los próximos lanzamientos no serán la primera vez.");
        }
    }
    public void SkipIntro()
    {
        SceneManager.LoadScene("MainMenuScene");
    }
    public void SetCinematicMode(bool isActive)
    {
        if (IsEventActive == isActive)
            return;

        IsEventActive = isActive;
        CambioEstadoControles?.Invoke(isActive);
    }

    // =============================================================
    // GESTIÓN DE EVENTOS
    // =============================================================

    public bool IsEventCompleted(string eventID)
    {
        return completedEvents.Contains(eventID);
    }

    public void MarkEventCompleted(string eventID)
    {
        if (!completedEvents.Contains(eventID))
        {
            completedEvents.Add(eventID);
            Debug.Log($"Evento marcado en memoria: {eventID}");
        }
    }

    // =============================================================
    // GESTIÓN DE DECISIONES
    // =============================================================
    public void SaveDecision(string eventID, int choiceValue)
    {
        string key = eventID + "_DECISION";
        PlayerPrefs.SetInt(key, choiceValue);
        Debug.Log($"Decisión '{choiceValue}' guardada en caché para el evento '{eventID}'.");
    }

    public int LoadDecision(string eventID)
    {
        string key = eventID + "_DECISION";
        return PlayerPrefs.GetInt(key, 0);
    }

    // =============================================================
    // GESTIÓN DE ESCENAS (PRIVADO)
    // =============================================================

    private void _SaveCurrentSceneData()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        PlayerPrefs.SetString(SceneSaveKey, currentSceneName);
        Debug.Log($"Escena '{currentSceneName}' guardada en caché.");
    }

    // =============================================================
    // GUARDADO Y CARGA CENTRALIZADA
    // =============================================================

    public void SaveGame()
    {
        Debug.Log("--- Iniciando guardado de partida ---");

        _SaveEventsData();
        _SavePlayerPosition();
        _SaveCurrentSceneData();

        //  _SaveInventoryData()

        PlayerPrefs.Save();
        Debug.Log("--- Guardado de Sesión completado y escrito en disco. ---");
    }
    private void _SavePlayerPosition()
    {
        if (playerTransform != null)
        {
            Vector3 pos = playerTransform.position;
            PlayerPrefs.SetFloat(PosXSaveKey, pos.x);
            PlayerPrefs.SetFloat(PosYSaveKey, pos.y);
            PlayerPrefs.SetFloat(PosZSaveKey, pos.z); 
            Debug.Log($"Posición ({pos.x:F2}, {pos.y:F2}) guardada.");
        }
    }
    private void _SaveEventsData()
    {
        string serializedEvents = string.Join(Separator.ToString(), completedEvents);
        PlayerPrefs.SetString(EventsSaveKey, serializedEvents);
        Debug.Log($"Eventos serializados y guardados en caché: {completedEvents.Count}");
    }

    public void LoadGame()
    {
        if (PlayerPrefs.HasKey(EventsSaveKey))
        {
            string serializedEvents = PlayerPrefs.GetString(EventsSaveKey);
            string[] eventArray = serializedEvents.Split(Separator);

            completedEvents.Clear();
            foreach (string id in eventArray)
            {
                if (!string.IsNullOrWhiteSpace(id))
                {
                    completedEvents.Add(id.Trim());
                }
            }
            Debug.Log($"Carga de Eventos exitosa: {completedEvents.Count}");
        }
    }

    public void LoadLastScene()
    {
        if (PlayerPrefs.HasKey(SceneSaveKey))
        {
            string sceneName = PlayerPrefs.GetString(SceneSaveKey);

            if (Application.CanStreamedLevelBeLoaded(sceneName))
            {
                IsLoadingGame = true;
                SceneManager.LoadScene(sceneName);
                Debug.Log($"Cargando última escena visitada: {sceneName}. Estado: IsLoadingGame=TRUE");
            }
            else
            {
                Debug.LogError($"La escena '{sceneName}' no se encontró o no está en Build Settings.");
            }
        }
    }
    public Vector3 GetSavedPlayerPosition()
    {
        float x = PlayerPrefs.GetFloat(PosXSaveKey, 0f); 
        float y = PlayerPrefs.GetFloat(PosYSaveKey, 0f);
        float z = PlayerPrefs.GetFloat(PosZSaveKey, 0f);
        return new Vector3(x, y, z);
    }

    public void ClearSavedData()
    {
        PlayerPrefs.DeleteKey(EventsSaveKey);
        PlayerPrefs.DeleteKey(SceneSaveKey);
        PlayerPrefs.DeleteKey(FirstTimePlayerKey); 
        PlayerPrefs.DeleteAll(); 
        completedEvents.Clear();
        PlayerPrefs.Save();
        Debug.Log("Datos de eventos, decisiones y escena borrados.");
    }


    // NUEVO MÉTODO para que RoomsManager pueda resetear el estado de carga.
    public static void SetIsLoadingGame(bool isLoading)
    {
        IsLoadingGame = isLoading;
    }
}