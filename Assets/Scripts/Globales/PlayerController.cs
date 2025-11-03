using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    public float WalkSpeed = 5f;
    public float RunSpeed = 8f;
    public float JumpSpeed = 5f;

    [Header("Acciones")]
    private InputAction movementAction;
    private InputAction jumpAction;
    private InputAction interactAction; 
    private InputAction dialogueAdvanceAction; 

    [Header("Componentes")]
    public InputActionAsset playerActions;
    private Animator animator;
    private Rigidbody2D rb;

    private Vector2 movementInput;
    bool isFacingRight = true;
    private bool isInputBlocked = false;

    private static PlayerController Instance;

    private void OnEnable()
    {
        GameManager.CambioEstadoControles += HandleCambioControles;
        playerActions.FindActionMap("Player").Enable();
    }

    private void OnDisable()
    {
        playerActions.FindActionMap("Player").Disable();
        GameManager.CambioEstadoControles -= HandleCambioControles;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        movementAction = playerActions.FindActionMap("Player").FindAction("Movimiento");
        jumpAction = playerActions.FindActionMap("Player").FindAction("Salto");

        interactAction = playerActions.FindActionMap("Player").FindAction("Interactuar");
        dialogueAdvanceAction = playerActions.FindActionMap("Player").FindAction("SaltarDialogo");

        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void HandleCambioControles(bool isBlocked)
    {
        isInputBlocked = isBlocked;

        if (isBlocked)
        {
            rb.velocity = Vector2.zero;
            animator.SetFloat("speed", 0f);

            movementAction.Disable();
            jumpAction.Disable();
            if (interactAction != null) interactAction.Disable();

            if (dialogueAdvanceAction != null) dialogueAdvanceAction.Enable();

        }
        else
        {
            movementAction.Enable();
            jumpAction.Enable();
            if (interactAction != null) interactAction.Enable();

            playerActions.FindActionMap("Player").Enable();
        }
    }

    void Update()
    {
        if (isInputBlocked) return;

        movementInput = movementAction.ReadValue<Vector2>();

        if (jumpAction.WasPressedThisFrame() && IsGrounded())
        {
            // Jump();
        }
    }
    private void FixedUpdate()
    {
        if (isInputBlocked) return;
        Walking();
        Flip();
    }

    void Flip()
    {
        if (Mathf.Abs(movementInput.x) > 0.01f)
        {
            if (isFacingRight && movementInput.x < 0f || !isFacingRight && movementInput.x > 0f)
            {
                isFacingRight = !isFacingRight;
                Vector3 localScale = transform.localScale;
                localScale.x *= -1f;
                transform.localScale = localScale;
            }
        }
    }
    private void Walking()
    {
        animator.SetFloat("speed", Math.Abs(movementInput.x));
        Vector2 velocity = new Vector2(movementInput.x * WalkSpeed, rb.velocity.y);
        rb.velocity = velocity;
    }

    private void Jump()
    {
        rb.AddForce(Vector2.up * JumpSpeed, ForceMode2D.Impulse);
    }
    private bool IsGrounded()
    {
        return true;
    }
}