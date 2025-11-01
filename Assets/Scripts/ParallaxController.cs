using UnityEngine;

public class ParallaxController : MonoBehaviour
{
    [Tooltip("Arrastra aquí el Transform del jugador o target que sigue la cámara.")]
    public Transform parallaxTarget;

    private Vector3 startTargetPos;
    private Material[] materials;
    private float[] backSpeeds;
    private float[] zDistances;

    [Header("Parallax Settings")]
    [Tooltip("El factor global que ajusta la velocidad total del parallax. (0.01 a 1.0)")]
    [Range(0.01f, 1f)]
    public float parallaxStrength = 0.2f;

    void Start()
    {
        // === SOLUCIÓN AL PROBLEMA DE REFERENCIA ===
        if (parallaxTarget == null)
        {
            // Busca al jugador persistente usando el Tag "Player".
            GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO != null)
            {
                parallaxTarget = playerGO.transform;
            }
        }
        // === FIN DE SOLUCIÓN ===

        if (parallaxTarget == null)
        {
            // Si después de buscar sigue siendo nulo, lanza el error y se deshabilita.
            Debug.LogError("Parallax Target no asignado. ¡El jugador persistente no fue encontrado (revisa su Tag 'Player')!");
            enabled = false;
            return;
        }

        startTargetPos = parallaxTarget.position;

        int bgCount = transform.childCount;
        materials = new Material[bgCount];
        backSpeeds = new float[bgCount];
        // Nota: backgrounds y zDistances no se usan en LateUpdate, se pueden optimizar
        // pero se dejan para mantener la estructura original.
        zDistances = new float[bgCount];

        for (int i = 0; i < bgCount; i++)
        {
            GameObject background = transform.GetChild(i).gameObject;
            Renderer renderer = background.GetComponent<Renderer>();

            if (renderer == null)
            {
                Debug.LogError($"Fondo '{background.name}' no tiene SpriteRenderer/Renderer.");
                continue;
            }

            materials[i] = renderer.material;
            zDistances[i] = background.transform.position.z;
        }

        CalculateRelativeSpeeds(bgCount);
    }

    void CalculateRelativeSpeeds(int backCount)
    {
        // ... (Tu lógica de cálculo de velocidad permanece igual)
        float closestZDistance = float.MaxValue;
        for (int i = 0; i < backCount; i++)
        {
            if (zDistances[i] < closestZDistance)
            {
                closestZDistance = zDistances[i];
            }
        }

        for (int i = 0; i < backCount; i++)
        {
            if (zDistances[i] > 0)
            {
                backSpeeds[i] = closestZDistance / zDistances[i];
            }
            else
            {
                backSpeeds[i] = 1.0f;
            }
        }
    }

    private void LateUpdate()
    {
        if (!enabled || parallaxTarget == null) return;
        float distanceMovedX = parallaxTarget.position.x - startTargetPos.x;
        for (int i = 0; i < materials.Length; i++)
        {
            float speed = backSpeeds[i] * parallaxStrength;
            float offsetX = distanceMovedX * speed;

            Vector2 currentOffset = materials[i].GetTextureOffset("_MainTex");

            materials[i].SetTextureOffset("_MainTex", new Vector2(offsetX, currentOffset.y));
        }
    }
}