using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DogAttackEvent : MonoBehaviour
{
    public Animator animator;

    [Header("Attack Settings")]
    [SerializeField] public float delayEntreAtaques = 3.0f;
    [SerializeField] public int maxAttackRepetitions = 3;
    [SerializeField] public string animationStateName = "perrolanzanding";
    [SerializeField] public GameObject[] knifes = new GameObject[3];

    [Header("Explosión")]
    [SerializeField] public string explosionAnimationName = "perroAustriaco";
    public UnityEvent OnDogExplode;

    private Coroutine attackCoroutine;
    [SerializeField] public int attackCount = 0;
    private Transform playerTransform;
    private KnifeProjectile[] knifeProjectiles;

    public void Awake()
    {
        Debug.Log("[DogAttackEvent] Awake() iniciado", this);

        animator = GetComponent<Animator>();
        Debug.Log($"[DogAttackEvent] ¿Animator encontrado? {animator != null}", this);

        // Obtener los componentes KnifeProjectile de los cuchillos
        knifeProjectiles = new KnifeProjectile[knifes.Length];
        for (int i = 0; i < knifes.Length; i++)
        {
            if (knifes[i] != null)
            {
                knifeProjectiles[i] = knifes[i].GetComponent<KnifeProjectile>();
                if (knifeProjectiles[i] == null)
                {
                    Debug.LogError($"[DogAttackEvent] Cuchillo {i} no tiene componente KnifeProjectile", this);
                }
                else
                {
                    Debug.Log($"[DogAttackEvent] Cuchillo {i} listo", this);
                }
                knifes[i].SetActive(false);
            }
            else
            {
                Debug.LogError($"[DogAttackEvent] Cuchillo {i} no asignado en el array", this);
            }
        }

        // Encontrar al jugador
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (playerTransform == null)
        {
            Debug.LogError("[DogAttackEvent] ¡¡¡ CRITICO: Player no encontrado. Asegúrate de que tiene el tag 'Player' !!!", this);
        }
        else
        {
            Debug.Log($"[DogAttackEvent] Player encontrado: {playerTransform.gameObject.name}", this);
        }

        Debug.Log("[DogAttackEvent] Awake() completado", this);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[DogAttackEvent] OnTriggerEnter2D detectado: {other.gameObject.name}, Tag: {other.tag}", this);

        // ===== DEBUG: Verificar condiciones =====
        Debug.Log($"[DogAttackEvent] ¿CompareTag('Player')? {other.CompareTag("Player")}", this);
        Debug.Log($"[DogAttackEvent] ¿GameManager.IsEventActive? {GameManager.IsEventActive}", this);
        Debug.Log($"[DogAttackEvent] ¿Este script enabled? {enabled}", this);
        Debug.Log($"[DogAttackEvent] Combinación de condiciones: {other.CompareTag("Player") && !GameManager.IsEventActive && enabled}", this);
        // ==========================================

        if (other.CompareTag("Player") && !GameManager.IsEventActive && enabled)
        {
            Debug.Log("[DogAttackEvent] ¡¡¡ DOG ATTACK TRIGGERED !!!", this);

            // Activar el evento de diálogo
            DialogosEventDogAttack dialogEvent = GetComponentInChildren<DialogosEventDogAttack>();
            Debug.Log($"[DogAttackEvent] ¿DialogosEventDogAttack encontrado? {dialogEvent != null}", this);

            if (dialogEvent != null)
            {
                Debug.Log("[DogAttackEvent] Llamando StartDogAttackEvent()...", this);
                dialogEvent.StartDogAttackEvent();
            }
            else
            {
                Debug.LogError("[DogAttackEvent] DialogosEventDogAttack NO ENCONTRADO en el GameObject", this);
            }
        }
        else
        {
            Debug.LogWarning($"[DogAttackEvent] Colisión no procesada porque una condición falló", this);
            if (!other.CompareTag("Player"))
                Debug.LogWarning($"[DogAttackEvent] - No es Player: {other.tag}", this);
            if (GameManager.IsEventActive)
                Debug.LogWarning($"[DogAttackEvent] - Evento ya activo", this);
            if (!enabled)
                Debug.LogWarning($"[DogAttackEvent] - Script deshabilitado", this);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Debug.Log($"[DogAttackEvent] OnTriggerExit2D detectado: {other.gameObject.name}", this);

        if (other.CompareTag("Player"))
        {
            Debug.Log("[DogAttackEvent] Jugador salió del área de ataque del perro", this);
            animator.SetBool("isPlayerInScene", false);
            StopAttackSequence();
        }
    }

    public void StartAttackSequence()
    {
        Debug.Log("[DogAttackEvent] StartAttackSequence() llamado", this);

        if (attackCoroutine != null)
        {
            Debug.LogWarning("[DogAttackEvent] Deteniendo corrutina de ataque anterior", this);
            StopCoroutine(attackCoroutine);
        }
        attackCoroutine = StartCoroutine(AttackLoopWithDelay());
    }

    public void StopAttackSequence()
    {
        Debug.Log("[DogAttackEvent] StopAttackSequence() llamado", this);

        if (attackCoroutine != null)
        {
            Debug.Log("[DogAttackEvent] Deteniendo secuencia de ataques", this);
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }
    }

    private IEnumerator AttackLoopWithDelay()
    {
        Debug.Log("[DogAttackEvent] AttackLoopWithDelay() iniciado", this);
        attackCount = 0;

        while (attackCount < maxAttackRepetitions && animator.GetBool("isPlayerInScene"))
        {
            attackCount++;
            Debug.Log($"[DogAttackEvent] ========== INICIANDO ATAQUE #{attackCount}/{maxAttackRepetitions} ==========", this);

            animator.SetBool("Lanzar", true);
            Debug.Log($"[DogAttackEvent] SetBool('Lanzar', true)", this);

            float animDuration = GetAnimationClipDuration(animationStateName);
            Debug.Log($"[DogAttackEvent] Duración de animación '{animationStateName}': {animDuration}s", this);

            yield return new WaitForSeconds(animDuration);

            Debug.Log($"[DogAttackEvent] Ataque #{attackCount} completado - Lanzando cuchillo", this);

            if (playerTransform != null && attackCount <= knifes.Length)
            {
                LaunchKnife(attackCount - 1);
            }
            else
            {
                Debug.LogWarning($"[DogAttackEvent] No se pudo lanzar cuchillo. PlayerTransform: {playerTransform != null}, AttackCount: {attackCount}, KnifesLength: {knifes.Length}", this);
            }

            animator.SetBool("Lanzar", false);
            Debug.Log($"[DogAttackEvent] SetBool('Lanzar', false)", this);

            // Esperar transición o timeout de 0.5 segundos
            float transitionWait = 0f;
            while (transitionWait < 0.5f && !IsInState("perroalterao"))
            {
                transitionWait += Time.deltaTime;
                yield return null;
            }

            Debug.Log($"[DogAttackEvent] Transición completada en {transitionWait}s", this);

            if (attackCount < maxAttackRepetitions && animator.GetBool("isPlayerInScene"))
            {
                Debug.Log($"[DogAttackEvent] Esperando {delayEntreAtaques}s antes del próximo ataque...", this);
                yield return new WaitForSeconds(delayEntreAtaques);
            }
        }

        animator.SetBool("isPlayerInScene", false);
        animator.SetBool("Lanzar", false);

        if (attackCount >= maxAttackRepetitions)
        {
            Debug.Log("[DogAttackEvent] !!!!!!!! EL PERRO EXPLOTA - CONTADOR EN 3 !!!!!!!!", this);

            OnDogExplode?.Invoke();
            animator.SetTrigger("Explotar");

            float explosionDuration = GetAnimationClipDuration(explosionAnimationName);
            Debug.Log($"[DogAttackEvent] Duración de explosión: {explosionDuration}s", this);

            yield return new WaitForSeconds(explosionDuration);

            Debug.Log("[DogAttackEvent] Desactivando GameObject del perro", this);
            gameObject.SetActive(false);
        }

        Debug.Log("[DogAttackEvent] Secuencia de ataques completada", this);
    }
    private void LaunchKnife(int knifeIndex)
    {
        if (knifeProjectiles[knifeIndex] == null)
        {
            Debug.LogWarning($"Cuchillo en índice {knifeIndex} no tiene componente KnifeProjectile");
            return;
        }

        knifeProjectiles[knifeIndex].LaunchTowards(playerTransform);
        Debug.Log($"Cuchillo #{knifeIndex + 1} lanzado hacia el jugador");
    }

    private float GetAnimationClipDuration(string clipName)
    {
        AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
        foreach (AnimationClip clip in clips)
        {
            if (clip.name == clipName)
            {
                return clip.length;
            }
        }
        Debug.LogWarning($"Animación '{clipName}' no encontrada");
        return 1f;
    }

    private bool IsInState(string stateName)
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.IsName(stateName);
    }
}
