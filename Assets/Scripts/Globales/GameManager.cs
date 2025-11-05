using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public static event Action<bool> CambioEstadoControles;
    public static bool IsEventActive { get; private set; } = false;

    private HashSet<string> completedEvents = new HashSet<string>();

    private const string EventsSaveKey = "CompletedEventsIDs";
    private const char Separator = ',';

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            ClearSavedData(); // Descomentar solo si estás en desarrollo
            LoadGame();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetCinematicMode(bool isActive)
    {
        if (IsEventActive == isActive)
            return;

        IsEventActive = isActive;
        // La clave: invocar el evento para que PlayerController actúe
        CambioEstadoControles?.Invoke(isActive);
    }

    // =============================================================
    // MÉTODOS DE PERSISTENCIA DE EVENTOS
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
            Debug.Log($"Evento marcado como completado: {eventID}");

            SaveGame();
        }
    }

    // =============================================================
    // MÉTODOS DE PERSISTENCIA DE DECISIONES (VALORES)
    // =============================================================

    public void SaveDecision(string eventID, int choiceValue)
    {
        string key = eventID + "_DECISION";
        PlayerPrefs.SetInt(key, choiceValue);
        PlayerPrefs.Save();
        Debug.Log($"Decisión '{choiceValue}' guardada para el evento '{eventID}'.");
    }

    public int LoadDecision(string eventID)
    {
        string key = eventID + "_DECISION";
        return PlayerPrefs.GetInt(key, 0);
    }

    // ... (Métodos de guardado y carga completos)
    public void SaveGame()
    {
        string serializedEvents = string.Join(Separator.ToString(), completedEvents);

        PlayerPrefs.SetString(EventsSaveKey, serializedEvents);
        PlayerPrefs.Save();
        Debug.Log($"Guardado de Sesión exitoso. Eventos guardados: {completedEvents.Count}");
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
            Debug.Log($"Carga de Sesión exitosa. Eventos cargados: {completedEvents.Count}");
        }
    }

    public void ClearSavedData()
    {
        PlayerPrefs.DeleteKey(EventsSaveKey);
        completedEvents.Clear();
        PlayerPrefs.Save();
        Debug.Log("Datos de eventos borrados.");
    }
}