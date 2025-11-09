using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic; 

public class RopaItemUI : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    // =============================================================
    // REFERENCIAS
    // =============================================================
    private TiendaRopaEvent eventManager;
    private RectTransform rectTransform;
    private Canvas canvas;
    private Animator currentActiveArmAnimator;
    // =============================================================
    // CONFIGURACIÓN DE MINIJUEGO
    // =============================================================
    [Header("Configuración de Arrastre")]
    [Tooltip("Distancia mínima (en unidades de Canvas) para que se considere 'limpiado'.")]
    public float minClearingDistance = 150f; // Unidades típicas de Canvas
    [Tooltip("El índice de jerarquía (Sibling Index) al que se mueve el ítem al arrastrarlo (debe ser el ÚLTIMO).")]
    public int dragSiblingIndex = -1; // -1 moverá al final de la lista (más arriba en el renderizado)

    private Vector2 originalPosition;
    private int originalSiblingIndex;
    private bool isCleared = false;

    // =============================================================
    // CICLO DE VIDA
    // =============================================================
    void Start()
    {
        eventManager = FindObjectOfType<TiendaRopaEvent>();
        if (eventManager == null)
        {
            Debug.LogError("RopaItemUI no encontró TiendaRopaEvent en la escena.");
            enabled = false;
            return;
        }

        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();

        if (rectTransform == null || canvas == null)
        {
            Debug.LogError($"El GameObject {gameObject.name} necesita un RectTransform y estar dentro de un Canvas.");
            enabled = false;
            return;
        }

        originalPosition = rectTransform.anchoredPosition;
        originalSiblingIndex = rectTransform.GetSiblingIndex();
    }

    // =============================================================
    // INTERACCIÓN DE UI (EventSystem)
    // =============================================================

    // Al presionar el mouse/toque
    public void OnPointerDown(PointerEventData eventData)
    {
        if (isCleared) return;

        // 1. ¡NUEVO! Obtener y almacenar el Animator del brazo que tiene el turno
        currentActiveArmAnimator = eventManager.GetCurrentGrabArm();

        transform.SetAsLastSibling();

        if (currentActiveArmAnimator != null)
        {
            currentActiveArmAnimator.SetTrigger("Grab");
        }
    }

    // Mientras arrastra el mouse/toque
    public void OnDrag(PointerEventData eventData)
    {
        if (isCleared) return;

        // Mover el RectTransform
        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay || canvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (isCleared) return;

        if (currentActiveArmAnimator != null)
        {
            currentActiveArmAnimator.SetTrigger("Release");
        }

        float distance = Vector2.Distance(rectTransform.anchoredPosition, originalPosition);

        if (distance >= minClearingDistance)
        {
            isCleared = true;
            eventManager.NotifyRopaCleared(gameObject);
            eventManager.ToggleNextGrabArm();
        }
        else
        {
            rectTransform.anchoredPosition = originalPosition;
            transform.SetSiblingIndex(originalSiblingIndex);

            eventManager.ToggleNextGrabArm();
        }
        currentActiveArmAnimator = null;
    }
}