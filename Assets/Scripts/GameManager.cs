using System; // Necesario para Action
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public static event Action<bool> CambioEstadoControles;
    public static bool IsEventActive { get; private set; } = false;

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

    public void SetCinematicMode(bool isActive)
    {
        if (IsEventActive == isActive)
            return;

        IsEventActive = isActive;

        CambioEstadoControles?.Invoke(isActive);
    }
}