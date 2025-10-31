using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Transition_Manager : MonoBehaviour
{
    private Animator animator;

    private const string FadeInTrigger = "FadeIn";  
    private const string FadeOutTrigger = "FadeOut"; 

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    public IEnumerator FadeIn()
    {
        animator.SetTrigger(FadeInTrigger);
        yield return new WaitForSeconds(1.0f);
    }
    public IEnumerator FadeOut()
    {
        animator.SetTrigger(FadeOutTrigger);
        yield return new WaitForSeconds(1.0f);
    }
}
