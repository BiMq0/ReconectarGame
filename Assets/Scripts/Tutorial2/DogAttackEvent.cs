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
    [SerializeField] public string explosionAnimationName = "perroexplosion";
    public UnityEvent OnDogExplode;

    private Coroutine attackCoroutine;
    [SerializeField] private int attackCount = 0;
    private Transform playerTransform;
    private KnifeProjectile[] knifeProjectiles;

    public void Awake()
    {
        animator = GetComponent<Animator>();

        // Obtener los componentes KnifeProjectile de los cuchillos
        knifeProjectiles = new KnifeProjectile[knifes.Length];
        for (int i = 0; i < knifes.Length; i++)
        {
            if (knifes[i] != null)
            {
                knifeProjectiles[i] = knifes[i].GetComponent<KnifeProjectile>();
                if (knifeProjectiles[i] == null)
                {
                    Debug.LogError($"Cuchillo {i} no tiene componente KnifeProjectile");
                }
                knifes[i].SetActive(false);
            }
            else
            {
                Debug.LogError($"Cuchillo {i} no asignado en el array");
            }
        }

        // Encontrar al jugador
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (playerTransform == null)
        {
            Debug.LogError("Player no encontrado. Asegúrate de que tiene el tag 'Player'");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Console.WriteLine("Dog attack triggered!");
            animator.SetBool("isPlayerInScene", true);
            StartAttackSequence();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Console.WriteLine("Dog attack ended!");
            animator.SetBool("isPlayerInScene", false);
            StopAttackSequence();
        }
    }

    private void StartAttackSequence()
    {
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
        }
        attackCoroutine = StartCoroutine(AttackLoopWithDelay());
    }

    private void StopAttackSequence()
    {
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }
    }

    private IEnumerator AttackLoopWithDelay()
    {
        attackCount = 0;

        while (attackCount < maxAttackRepetitions && animator.GetBool("isPlayerInScene"))
        {
            attackCount++;
            Debug.Log($"Iniciando ataque #{attackCount}/{maxAttackRepetitions}");

            animator.SetBool("Lanzar", true);

            yield return new WaitForSeconds(GetAnimationClipDuration(animationStateName));

            Debug.Log($"Ataque #{attackCount} completado");

            if (playerTransform != null && attackCount <= knifes.Length)
            {
                LaunchKnife(attackCount - 1);
            }

            animator.SetBool("Lanzar", false);

            // Esperar a que realmente transicione a perroalterao
            yield return new WaitUntil(() => IsInState("perroalterao"));

            if (attackCount < maxAttackRepetitions && animator.GetBool("isPlayerInScene"))
            {
                Debug.Log($"Esperando {delayEntreAtaques} segundos antes del próximo ataque");
                yield return new WaitForSeconds(delayEntreAtaques);
            }
        }

        animator.SetBool("isPlayerInScene", false);
        animator.SetBool("Lanzar", false);

        if (attackCount >= maxAttackRepetitions)
        {
            Debug.Log("¡El perro explota!");
            yield return new WaitForSeconds(delayEntreAtaques);

            OnDogExplode?.Invoke();
            animator.SetTrigger("Explotar");

            yield return new WaitForSeconds(GetAnimationClipDuration(explosionAnimationName));

            gameObject.SetActive(false);
        }

        Debug.Log("Secuencia de ataques completada");
    }
    private void LaunchKnife(int knifeIndex)
    {
        if (knifeProjectiles[knifeIndex] == null)
        {
            Debug.LogWarning($"Cuchillo en índice {knifeIndex} no tiene componente KnifeProjectile");
            return;
        }

        knifeProjectiles[knifeIndex].LaunchTowards(playerTransform.position);
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
