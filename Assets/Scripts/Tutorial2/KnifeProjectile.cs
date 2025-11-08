using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnifeProjectile : MonoBehaviour
{
    [Header("Velocidad")]
    [SerializeField] private float projectileSpeed = 10f;

    private Transform playerTarget;
    private bool isMoving = false;

    public void LaunchTowards(Transform player)
    {
        playerTarget = player;
        isMoving = true;
        gameObject.SetActive(true);

        Debug.Log($"Cuchillo activado en posición {transform.position}, persiguiendo al jugador");
    }

    private void FixedUpdate()
    {
        if (!isMoving || playerTarget == null) return;

        // Mover hacia el jugador en tiempo real
        Vector3 direction = (playerTarget.position - transform.position).normalized;
        transform.position = Vector3.MoveTowards(
            transform.position,
            playerTarget.position,
            projectileSpeed * Time.fixedDeltaTime
        );

        // Rotar hacia el jugador
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log($"¡Cuchillo golpeó al jugador!");

            isMoving = false;
            gameObject.SetActive(false);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // Si el cuchillo sale del área sin golpear, desactivarlo
        if (!collision.CompareTag("Player"))
        {
            isMoving = false;
            gameObject.SetActive(false);
        }
    }
}
