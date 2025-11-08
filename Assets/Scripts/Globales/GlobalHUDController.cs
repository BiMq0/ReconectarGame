using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GlobalHUDController : MonoBehaviour
{
    [Header("Canvases")]
    [SerializeField] private Canvas pauseMenuCanvas;
    [SerializeField] private Canvas inventoryCanvas;

    private bool isPaused = false;

    private void Awake()
    {
        if (FindObjectsOfType<GlobalHUDController>().Length > 1)
        {
            Destroy(gameObject);
        }
        else
        {

            DontDestroyOnLoad(gameObject);
        }

        if (pauseMenuCanvas != null)
            pauseMenuCanvas.gameObject.SetActive(false);
        if (inventoryCanvas != null)
            inventoryCanvas.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseMenu();
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            ToggleInventory();
        }
    }

    public void TogglePauseMenu()
    {
        isPaused = !isPaused;

        if (pauseMenuCanvas != null)
        {
            pauseMenuCanvas.gameObject.SetActive(isPaused);
        }

        Time.timeScale = isPaused ? 0f : 1f;
        Debug.Log(isPaused ? "Juego pausado" : "Juego reanudado");
    }

    public void ContinueGame()
    {
        isPaused = false;
        if (pauseMenuCanvas != null)
        {
            pauseMenuCanvas.gameObject.SetActive(false);
        }
        Time.timeScale = 1f;
        Debug.Log("Continuando juego");
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenuScene");
        Debug.Log("Ir al menú principal");
    }

    public void ToggleInventory()
    {
        if (inventoryCanvas != null)
        {
            bool isActive = inventoryCanvas.gameObject.activeSelf;
            inventoryCanvas.gameObject.SetActive(!isActive);
            Debug.Log(isActive ? "Inventario cerrado" : "Inventario abierto");
        }
    }
}
