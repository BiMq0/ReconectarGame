using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialEventsManager : MonoBehaviour
{
    private bool eventTriggered = false;

    [Header("Ajustes")]
    public float Duracion = 3.0f;
    public GameObject PerroObjeto;
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
