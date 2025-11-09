using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManagerScript : MonoBehaviour
{
    [Header("Configuración de Escenas")]
    [Tooltip("El nombre de la primera escena de juego (ej. Tutorial o Level_1).")]
    public string firstGameSceneName = "Introduccion";

    [Header("Referencias de UI")]
    [Tooltip("El panel o contenedor que tiene los botones 'Nueva Partida' y 'Cargar Partida'.")]
    public GameObject gameModePanel;
    public GameObject mainmenuPanel;

    private void Awake()
    {
        MusicManager.PlayBGM("mainmenu", false);

        if (gameModePanel != null)
        {
            gameModePanel.SetActive(false);
        }
    }
    public void ToggleGameModeSelection()
    {
        if (gameModePanel != null)
        {
            bool currentState = gameModePanel.activeSelf;
            gameModePanel.SetActive(!currentState);
        }
        mainmenuPanel.SetActive(false);
    }

    public void Volver()
    {
        mainmenuPanel.SetActive(true);
        gameModePanel.SetActive(false);
    }
    public void NewGame()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ClearSavedData();
            GameManager.Instance.MarkGameStarted();
        }

        SceneManager.LoadScene(firstGameSceneName);
        Debug.Log($"Iniciando Nueva Partida en la escena: {firstGameSceneName}");
    }

    /// <summary>
    /// Carga la última partida guardada si existe. Muestra una advertencia si no hay guardado.
    /// Asignar al botón 'Cargar Partida'.
    /// </summary>
    public void LoadGame()
    {
        if (GameManager.Instance != null && GameManager.Instance.DoesSaveGameExist())
        {
            // 1. Carga la última escena guardada (el GameManager se encarga de posicionar al jugador).
            GameManager.Instance.LoadLastScene();
            Debug.Log("Cargando partida guardada.");
        }
        else
        {
            Debug.LogWarning("¡Advertencia! No existe partida guardada para cargar. El botón debería estar deshabilitado o mostrar un mensaje.");
            // Opcional: Mostrar un mensaje temporal en la UI al jugador.
        }
    }

    public void ExitGame()
    {
        Application.Quit();
        Debug.Log("Saliendo del juego.");
    }
}