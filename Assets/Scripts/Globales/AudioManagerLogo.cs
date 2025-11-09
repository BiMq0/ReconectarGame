using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManagerLogo : MonoBehaviour
{
    [Header("Configuración de Carga")]
    [Tooltip("El nombre de la escena a cargar después del splash (ej: MainMenu).")]
    public string nextSceneName = "Introduccion";

    [Tooltip("Tiempo extra a esperar después de que el audio termina.")]
    public float extraWaitTime = 0.5f;

    [Tooltip("Tiempo de espera para el segundo logo (sin audio).")]
    public float logoDisplayTime = 4.0f;

    private const float FadeDuration = 1.0f;
    private const string FadeInTrigger = "FadeIn";
    private const string FadeOutTrigger = "FadeOut";

    [Header("Referencias Locales")]
    [Tooltip("GameObject del primer logo (ej: QPHPAM).")]
    public GameObject logoDisplayObject;

    [Tooltip("GameObject del segundo logo (ej: Logo original).")]
    public GameObject logoText;

    public Animator animator;
    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        // Inicializa el logo y el texto como ocultos
        if (logoDisplayObject != null) logoDisplayObject.SetActive(false);
        if (logoText != null) logoText.SetActive(false);

        if (animator == null)
        {
            Debug.LogError("AudioManagerLogo: ¡El componente Animator está vacío! Arrastra la referencia del Canvas de Transición.");
        }
    }
    void Start()
    {
        if (animator != null && audioSource != null && audioSource.clip != null)
        {
            StartCoroutine(RunSplashSequence());
        }
        else
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }

    private IEnumerator RunSplashSequence()
    {
        if (logoDisplayObject != null) logoDisplayObject.SetActive(true);

        animator.SetTrigger(FadeOutTrigger);

        yield return new WaitForSeconds(logoDisplayTime);

        animator.SetTrigger(FadeInTrigger);

        if (logoDisplayObject != null) logoDisplayObject.SetActive(false);

        yield return new WaitForSeconds(2);


        animator.SetTrigger(FadeOutTrigger);

        if (logoText != null) logoText.SetActive(true);

        yield return new WaitForSeconds(logoDisplayTime);

        animator.SetTrigger(FadeInTrigger);

        if (logoText != null) logoText.SetActive(false);
        yield return new WaitForSeconds(2);

        SceneManager.LoadScene(nextSceneName);
    }
}