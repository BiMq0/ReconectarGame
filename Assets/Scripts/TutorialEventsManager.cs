using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialEventsManager : MonoBehaviour
{
    private bool eventTriggered = false;

    [Header("Ajustes")]
    public float Duracion = 3.0f;
    public string MensajeTexto = "¡Cuidado con el perro!";
    public GameObject PerroObjeto;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            CambioDeEscena();
        }
    }

    public void CambioDeEscena()
    {
        Debug.Log("Cambio de escena activado");
        GameManager.Instance.SetCinematicMode(true);
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
        Debug.Log(MensajeTexto);

        yield return new WaitForSeconds(Duracion);

        PerroObjeto.SetActive(false);

        Debug.Log("debug: fin del evento");


        GameManager.Instance.SetCinematicMode(false);
    }
}
