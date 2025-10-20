using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DogEvent : MonoBehaviour
{
    public TutorialEventsManager eventController;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (eventController != null)
            {
                eventController.ActivarEvento();
            }
            enabled = false;
        }
    }
}
