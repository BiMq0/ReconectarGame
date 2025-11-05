using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class ChiruController : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float speed = 10f; 

    [Header("Componentes")]
    private Animator animator;
    private Rigidbody2D rb;

    private Vector2 movementInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }
    private void FixedUpdate()
    {
        rb.velocity = movementInput * speed;
    }

    public void MoveCharacter(InputAction.CallbackContext context)
    {

        animator.SetBool("isWalking", true);
        if (context.canceled)
        {
            animator.SetBool("isWalking", false);
            animator.SetFloat("LastInputX", movementInput.x);
            animator.SetFloat("LastInputY", movementInput.y);
        }
        movementInput = context.ReadValue<Vector2>();
        animator.SetFloat("InputX", movementInput.x);
        animator.SetFloat("InputY",movementInput.y);
    }
   
}