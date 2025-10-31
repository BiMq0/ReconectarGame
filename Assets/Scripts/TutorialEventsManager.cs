using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialEventsManager : MonoBehaviour
{
    private bool eventTriggered = false;

    [Header("Ajustes")]
    public float Duracion = 3.0f;
    public GameObject PerroObjeto;
    private bool triggered = false;

    public Transition_Manager transitionManager;
    public Transform playerStartPoint;

    [Header("Carpetas de Fondos")]
    public GameObject currentRoomBackground;
    public GameObject nextRoomBackground;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            triggered = true;
            GetComponent<Collider2D>().enabled = false;
            StartCoroutine(TransitionSequence(other.transform));
        }
    }

    private IEnumerator TransitionSequence(Transform playerTransform)
    {
        GameManager.Instance.SetCinematicMode(true);

        yield return transitionManager.StartCoroutine(transitionManager.FadeIn());

        playerTransform.position = playerStartPoint.position;

        if (currentRoomBackground != null)
        {
            currentRoomBackground.SetActive(false);
        }
        if (nextRoomBackground != null)
        {
            nextRoomBackground.SetActive(true);
        }

        // Opcional: Si el PlayerController tiene una lógica de reinicio, puedes llamarla aquí.

        yield return transitionManager.StartCoroutine(transitionManager.FadeOut());

        GameManager.Instance.SetCinematicMode(false);

        // Destroy(gameObject); 
    }
    public void ActivarEvento()
    {
        if (eventTriggered) return;

        eventTriggered = true;

        GameManager.Instance.SetCinematicMode(true);

        StartCoroutine(PerroSecuencia());
    }

    private IEnumerator PerroSecuencia()
    {
        Debug.Log("debug: inicio del evento");
                

        yield return new WaitForSeconds(Duracion);

        PerroObjeto.SetActive(false);

        Debug.Log("debug: fin del evento");


        GameManager.Instance.SetCinematicMode(false);
    }
}
