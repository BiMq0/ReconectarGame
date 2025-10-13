using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Dependencies.Sqlite;
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
    private InputAction pauseAction;
    private InputAction pauseActionUI;

    [Header("Componentes")]
    public InputActionAsset playerActions;
    private Rigidbody2D rigidbody;
    public GameObject pauseDisplay;
    // private Animator animator;
    private Vector2 movementInput;

    private void OnEnable()
    {
        playerActions.FindActionMap("Player").Enable();
    }
    private void OnDisable()
    {
        playerActions.FindActionMap("Player").Disable();
    }
    private void Awake()
    {
        movementAction = playerActions.FindActionMap("Player").FindAction("Movimiento");
        jumpAction = playerActions.FindActionMap("Player").FindAction("Salto");
        pauseAction = playerActions.FindActionMap("Player").FindAction("Pausa");
        pauseActionUI = playerActions.FindActionMap("UI").FindAction("Pausa"); 

        rigidbody = GetComponent<Rigidbody2D>();

        pauseAction.performed += ctx => DisplayPause();
    }

    void Update()
    {
        movementInput = movementAction.ReadValue<Vector2>();

        if (jumpAction.WasPressedThisFrame() && IsGrounded()) 
        {
            Jump();
        }
    }
    private void FixedUpdate()
    {
        Walking();
    }

    private void Walking()
    {
        Vector2 velocity = new Vector2(movementInput.x * WalkSpeed, rigidbody.velocity.y);
        rigidbody.velocity = velocity;
    }

    private void Jump()
    {
        rigidbody.AddForce(Vector2.up * JumpSpeed, ForceMode2D.Impulse);
    }
    private bool IsGrounded()
    {
        return true;
    }
    private void DisplayPause()
    {
        if (pauseAction.WasPressedThisFrame())
        {
            pauseDisplay.SetActive(true);
            playerActions.FindActionMap("Player").Disable();
            playerActions.FindActionMap("UI").Enable();
        }
        else if (pauseActionUI.WasPressedThisFrame())
        {
            pauseDisplay.SetActive(false);
            playerActions.FindActionMap("UI").Disable();
            playerActions.FindActionMap("Player").Enable();
        }
    }
}
