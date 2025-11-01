using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneEntrance : MonoBehaviour
{
    [Header("Configuración de Dirección")]
    [Tooltip("1 para ir a la siguiente sala (derecha), -1 para ir a la sala anterior (izquierda).")]
    public int directionToMove = 1;

    private bool triggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !triggered)
        {
            triggered = true;

            GetComponent<Collider2D>().enabled = false;

            RoomsManager.Instance.GoToRoom(directionToMove);
        }
    }
}
