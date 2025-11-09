using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VientoScript : MonoBehaviour
{
    [Header("═══ CONFIGURACIÓN DEL VIENTO ═══")]
    [Tooltip("Dirección del viento (normalizada automáticamente).")]
    public Vector2 windDirection = new Vector2(-1f, 0f);

    [Tooltip("Intensidad de la fuerza del viento.")]
    public float windStrength = 15f;

    [Tooltip("Método de aplicación de fuerza")]
    public ForceMode2D forceMode = ForceMode2D.Force;

    [Header("═══ VISUALIZACIÓN ═══")]
    [Tooltip("Mostrar la dirección del viento en la escena.")]
    public bool showGizmo = false;

    [Tooltip("Color del Gizmo")]
    public Color gizmoColor = new Color(0.3f, 0.8f, 1f, 0.6f);

    private Collider2D windCollider;
    private Vector2 normalizedWindDirection;

    private void Start()
    {
        windCollider = GetComponent<Collider2D>();
        normalizedWindDirection = windDirection.normalized;

        if (windCollider == null)
        {
        }
        else if (!windCollider.isTrigger)
        {
            windCollider.isTrigger = true;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
        {
            return;
        }


        Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            return;
        }

        if (rb.isKinematic)
        {
            return;
        }

        Vector2 appliedForce = normalizedWindDirection * windStrength;

        rb.AddForce(appliedForce, forceMode);

    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
        }
    }

    private void OnDrawGizmos()
    {
        if (!showGizmo) return;

        Collider2D col = GetComponent<Collider2D>();
        if (col == null) return;

        Gizmos.color = gizmoColor;
        Gizmos.DrawCube(transform.position, col.bounds.size);

        Vector3 start = transform.position;
        Vector3 end = start + (Vector3)(windDirection.normalized * 3f);
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(start, end);
        Gizmos.DrawSphere(end, 0.15f);

        Gizmos.color = new Color(0, 1, 1, 0.3f);
        for (float i = 0; i < 3f; i += 0.3f)
        {
            Vector3 pos = start + (Vector3)(windDirection.normalized * i);
            Gizmos.DrawSphere(pos, 0.05f);
        }
    }
}
