using System.Collections;
using UnityEngine;

// Este script ahora debe ir en el GameObject RAIZ del Canvas que contiene el BlackScreen
public class Transition_Manager : MonoBehaviour
{
    // Hacemos que sea un Singleton para acceso global
    public static Transition_Manager Instance { get; private set; }

    // Referencia al componente Animator, que sigue estando en el BlackScreen (hijo)
    private Animator animator;

    private const string FadeInTrigger = "FadeIn";
    private const string FadeOutTrigger = "FadeOut";

    private void Awake()
    {
        // 1. Lógica de Singleton y Persistencia en el objeto raíz (Canvas/Contenedor)
        if (Instance == null)
        {
            Instance = this;
            // Quitamos transform.SetParent(null); si este objeto ya es un root de UI
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // Si ya existe una instancia (porque venimos de otra escena), destruye la nueva.
            Destroy(gameObject);
            return;
        }

        Animator[] animators = GetComponentsInChildren<Animator>(true);
        if (animators.Length > 0)
        {
            // Tomamos el primer Animator encontrado en los hijos (el del panel negro)
            animator = animators[0];
        }

        if (animator == null)
        {
            Debug.LogError("Transition_Manager: No se encontró el componente Animator en los hijos. ¿Está en el panel BlackScreen?");
        }
    }

    // ... [Resto de los métodos FadeIn y FadeOut se mantienen igual] ...
    public IEnumerator FadeIn()
    {
        if (animator == null) yield break; // Evita NullReference
        animator.SetTrigger(FadeInTrigger);
        yield return new WaitForSeconds(1.0f);
    }

    public IEnumerator FadeOut()
    {
        if (animator == null) yield break; // Evita NullReference
        animator.SetTrigger(FadeOutTrigger);
        yield return new WaitForSeconds(1.0f);
    }
}