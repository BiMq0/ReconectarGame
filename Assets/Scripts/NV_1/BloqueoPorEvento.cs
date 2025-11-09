using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Desactiva un Collider (barrera) cuando se cumplen todos los IDs de evento especificados.
/// La verificación se realiza al inicio de la escena y al chocar contra la barrera.
/// </summary>
public class BloqueoPorEvento : MonoBehaviour
{
    // =============================================================
    // CONFIGURACIÓN DEL BLOQUEO
    // =============================================================
    [Header("Configuración del Bloqueo")]
    [Tooltip("Lista de IDs de eventos que deben estar COMPLETADOS para desactivar este bloqueo.")]
    public List<string> requiredEventIDs = new List<string>
    {
        "NIÑO_EVENT",
        "JUGO_EVENT",
        "TIENDA_ROPA_EVENT"
    };

    [Header("Referencias Opcionales")]
    [Tooltip("El Collider que será desactivado (suele ser el propio componente).")]
    public Collider2D requiredCollider;


    private bool isLocked = true;

    void Awake()
    {
        if (requiredCollider == null)
        {
            requiredCollider = GetComponent<Collider2D>();
        }

        if (requiredCollider != null)
        {
            requiredCollider.enabled = true;
            requiredCollider.isTrigger = false; // Asegurar que sea una barrera sólida.
        }
        // Verifica el estado del desbloqueo al cargar la escena (por si ya estaba desbloqueado).
        CheckUnlockCondition();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            CheckUnlockCondition();
        }
    }


    /// <summary>
    /// Comprueba el estado de los eventos en el GameManager y desactiva el bloqueo si se cumplen.
    /// </summary>
    public void CheckUnlockCondition()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("BloqueoPorEvento requiere que GameManager esté inicializado.");
            return;
        }

        if (!isLocked)
        {
            return; // Ya está desbloqueado.
        }

        bool allEventsCompleted = true;
        foreach (string eventID in requiredEventIDs)
        {
            if (!GameManager.Instance.IsEventCompleted(eventID))
            {
                allEventsCompleted = false;
                break;
            }
        }

        if (allEventsCompleted)
        {
            UnlockPassage();
        }
        else
        {
            SoundEffectManager.Play("Explosion");
        }
    }

    /// <summary>
    /// Desactiva el bloqueo (Collider y Renderer).
    /// </summary>
    private void UnlockPassage()
    {
        isLocked = false;

        if (requiredCollider != null)
        {
            requiredCollider.enabled = false;
        }

        // Opcional: Desactivar la representación visual
        if (GetComponent<Renderer>() != null)
        {
            GetComponent<Renderer>().enabled = false;
        }


        Debug.Log($"Bloqueo {gameObject.name}: ¡TODOS los eventos completos! Pasaje desbloqueado.");
    }
}