using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;

    [Header("Animation")]
    public string walkAnimParam = "isWalking";

    private Animator animator;
    private CharacterController controller;
    private Camera mainCamera;

    private Vector2 moveInput;

    void Start()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        mainCamera = Camera.main;
    }

    void Update()
    {
        // Читаем WASD / стрелки через новый Input System
        moveInput = new Vector2(
            Keyboard.current != null && Keyboard.current.dKey.isPressed ? 1f :
            Keyboard.current != null && Keyboard.current.aKey.isPressed ? -1f : 0f,
            Keyboard.current != null && Keyboard.current.wKey.isPressed ? 1f :
            Keyboard.current != null && Keyboard.current.sKey.isPressed ? -1f : 0f
        );

        Vector3 direction = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
        bool isMoving = direction.magnitude > 0.1f;

        if (isMoving)
        {
            Vector3 moveDir = direction;
            if (mainCamera != null)
            {
                Vector3 camForward = mainCamera.transform.forward;
                Vector3 camRight = mainCamera.transform.right;
                camForward.y = 0f;
                camRight.y = 0f;
                moveDir = (camForward.normalized * moveInput.y + camRight.normalized * moveInput.x).normalized;
            }

            controller.Move(moveDir * moveSpeed * Time.deltaTime);

            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // Гравитация
        if (!controller.isGrounded)
            controller.Move(Vector3.down * 9.81f * Time.deltaTime);

        animator.speed = isMoving ? 1f : 0f;
    }
}
