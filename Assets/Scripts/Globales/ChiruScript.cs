using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChiruScript : MonoBehaviour
{
    private Animator animator;
    private Vector2 movementInput;

    [Header("Velocidad")]
    [SerializeField] private float speed = 300f;

    private Rigidbody2D rb;

    private void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        movementInput = new Vector2(horizontal, vertical).normalized;

        if (movementInput.magnitude > 0)
        {
            PlayAnimationByDirection(horizontal, vertical);
        }
    }

    private void FixedUpdate()
    {
        rb.velocity = movementInput * speed;
    }

    private void PlayAnimationByDirection(float horizontal, float vertical)
    {
        if (vertical > 0)
        {
            animator.SetTrigger("chiruBackward");
        }
        else if (vertical < 0)
        {
            animator.SetTrigger("chiruForward");
        }
        else if (horizontal != 0)
        {
            animator.SetTrigger("chiruLado");
        }
        else
        {
            animator.SetTrigger("chiruIdle");
        }
    }
}
