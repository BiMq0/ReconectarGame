using UnityEngine;

public class ParallaxController : MonoBehaviour
{
    // ASIGNAR EN EL INSPECTOR: El Transform que controla tu CamaraManager (el jugador).
    [Tooltip("Arrastra aqu� el Transform del jugador o target que sigue la c�mara.")]
    public Transform parallaxTarget;

    // Posici�n inicial del target (jugador) al inicio del juego.
    private Vector3 startTargetPos;

    // Arrays para las referencias y c�lculos
    private GameObject[] backgrounds;
    private Material[] materials;
    private float[] backSpeeds;
    private float[] zDistances;

    [Header("Parallax Settings")]
    [Tooltip("El factor global que ajusta la velocidad total del parallax. (0.01 a 1.0)")]
    [Range(0.01f, 1f)]
    public float parallaxStrength = 0.2f;

    void Start()
    {
        if (parallaxTarget == null)
        {
            Debug.LogError("Parallax Target no asignado. �Asigna el Transform del jugador!");
            enabled = false;
            return;
        }

        startTargetPos = parallaxTarget.position;

        int bgCount = transform.childCount;
        materials = new Material[bgCount];
        backSpeeds = new float[bgCount];
        backgrounds = new GameObject[bgCount];
        zDistances = new float[bgCount];

        for (int i = 0; i < bgCount; i++)
        {
            backgrounds[i] = transform.GetChild(i).gameObject;
            Renderer renderer = backgrounds[i].GetComponent<Renderer>();

            if (renderer == null)
            {
                Debug.LogError($"Fondo '{backgrounds[i].name}' no tiene SpriteRenderer/Renderer.");
                continue;
            }

            // Usamos .material para obtener una copia �nica y no modificar otros objetos que compartan el mismo material.
            materials[i] = renderer.material;
            zDistances[i] = backgrounds[i].transform.position.z;
        }

        CalculateRelativeSpeeds(bgCount);
    }

    // Calcula la velocidad relativa de cada capa basada en su Z.
    // Z positivo grande = Lejos (Lento), Z positivo peque�o = Cerca (R�pido).
    void CalculateRelativeSpeeds(int backCount)
    {
        // 1. Encontrar la Z m�s cercana (valor m�s peque�o, ej: 1). Esta se mover� m�s r�pido.
        float closestZDistance = float.MaxValue;
        for (int i = 0; i < backCount; i++)
        {
            if (zDistances[i] < closestZDistance)
            {
                closestZDistance = zDistances[i];
            }
        }

        // 2. Calcular la velocidad de cada capa.
        for (int i = 0; i < backCount; i++)
        {
            if (zDistances[i] > 0)
            {
                // Usamos la relaci�n inversa: (Capa m�s cercana) / (Z de la capa actual)
                // Ejemplo: closestZDistance=1. 
                // Capa en Z=1 -> backSpeed = 1/1 = 1 (M�xima velocidad)
                // Capa en Z=50 -> backSpeed = 1/50 = 0.02 (M�nima velocidad)
                backSpeeds[i] = closestZDistance / zDistances[i];
            }
            else
            {
                // Si Z es 0 o negativo, la velocidad es 1 para un movimiento completo (aunque lo ideal es Z > 0).
                backSpeeds[i] = 1.0f;
            }
        }
    }

    // LateUpdate se ejecuta despu�s de que el CamaraManager ha movido la c�mara.
    private void LateUpdate()
    {
        // Calcular la distancia total que el jugador se ha movido desde el inicio.
        float distanceMovedX = parallaxTarget.position.x - startTargetPos.x;

        // **IMPORTANTE:** ELIMINAMOS la l�nea 'transform.position = new Vector3(cam.position.x, ...)'
        // Ya no movemos el contenedor, solo cambiamos el offset de la textura.

        for (int i = 0; i < backgrounds.Length; i++)
        {
            // Velocidad total = Velocidad relativa * Fuerza global
            float speed = backSpeeds[i] * parallaxStrength;

            // C�lculo del offset en X
            float offsetX = distanceMovedX * speed;

            Vector2 currentOffset = materials[i].GetTextureOffset("_MainTex");

            // Aplicar el nuevo offset, manteniendo el Y.
            materials[i].SetTextureOffset("_MainTex", new Vector2(offsetX, currentOffset.y));
        }
    }
}