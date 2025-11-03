using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnifeProjectile : MonoBehaviour
{
    [Header("Velocidad")]
    [SerializeField] private float projectileSpeed = 10f;

    private Vector3 targetPosition;
    private bool isMoving = false;

    public void LaunchTowards(Vector3 target)
    {
        targetPosition = target;
        isMoving = true;
        gameObject.SetActive(true);

        // Calcular dirección
        Vector3 direction = (targetPosition - transform.position).normalized;

        // Rotar el cuchillo hacia el objetivo
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        Debug.Log($"Cuchillo activado en posición {transform.position}, apuntando a {target}");
    }

    private void FixedUpdate()
    {
        if (!isMoving) return;

        // Mover hacia el objetivo
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            projectileSpeed * Time.fixedDeltaTime
        );

        // Si llegó al objetivo, desactivar
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            isMoving = false;
            gameObject.SetActive(false);
        }
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
}
