using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DogEvent : MonoBehaviour
{
    [Header("Configuración de Evento")]
    [Tooltip("ID único para guardar el progreso:")]
    public string eventID = "PERRO_GETUP_EVENT";

    [Tooltip("Duración de la cinemática o espera.")]
    public float eventDuration = 3.0f;

    [Header("Referencias de la Escena")]
    [Tooltip("El objeto del perro que desaparecerá/se moverá.")]
    public GameObject dogObject;

    [Tooltip("Animator del Trigger (si el área de trigger tiene una animación).")]
    public Animator dogAnimator;

    private Collider2D triggerCollider;

    void Awake()
    {
        triggerCollider = GetComponent<Collider2D>();
    }

    void Start()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsEventCompleted(eventID))
        {
            if (dogObject != null)
            {
                dogObject.SetActive(false);
            }
            enabled = false;
            return;
        }

        if (dogObject != null)
        {
            dogObject.SetActive(true);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !GameManager.IsEventActive && enabled)
        {
            if (triggerCollider != null)
            {
                triggerCollider.enabled = false;
            }

            ExecuteEventSequence();
        }
    }

    private void ExecuteEventSequence()
    {
        if (GameManager.Instance == null) return;

        GameManager.Instance.SetCinematicMode(true);

        if (dogAnimator != null)
        {
            dogAnimator.SetBool("isActive", true);
        }

        StartCoroutine(DogEventSecuence());
    }

    private IEnumerator DogEventSecuence()
    {
        Debug.Log("DEBUG: Inicio de la cinemática del perro.");

        yield return new WaitForSeconds(eventDuration);

        if (dogObject != null)
        {
            dogObject.SetActive(false);
        }

        GameManager.Instance.MarkEventCompleted(eventID);

        GameManager.Instance.SetCinematicMode(false);

        enabled = false;

        Debug.Log("DEBUG: Evento de perro finalizado y guardado.");
    }
}
