using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;

    [SerializeField] private float groundDistanceTolerance = 0.1f;
    [SerializeField] private LayerMask groundLayerMask;

    private Animator animator;
    private CharacterController controller;
    private Camera mainCamera;
    private bool isFalling;
    public bool IsGrounded => isGrounded;
    private bool isGrounded;
    private Vector3 velocity;
    private Vector2 moveInput;

    void Start()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        mainCamera = Camera.main;

        isGrounded = true;
        animator.SetBool("isGrounded", true);
    }

    public void ForceUnground()
    {
        isGrounded = false;
        isFalling = false;
        velocity.y = 0f;
        animator.SetBool("isGrounded", false);
    }

    private void OnMove(InputValue inputValue)
    {
        moveInput = inputValue.Get<Vector2>();
    }

    void Update()
    {
        UpdateGrounded();

        Vector3 direction = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
        bool isRunning = direction.magnitude > 0.1f;

        if (controller == null) return;

        Vector3 horizontalMove = Vector3.zero;
        if (isRunning)
        {
            horizontalMove = direction;
            if (mainCamera != null)
            {
                Vector3 camForward = mainCamera.transform.forward;
                Vector3 camRight = mainCamera.transform.right;
                camForward.y = 0f;
                camRight.y = 0f;
                horizontalMove = (camForward.normalized * moveInput.y + camRight.normalized * moveInput.x).normalized;
            }

            Quaternion targetRotation = Quaternion.LookRotation(horizontalMove);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        if (isGrounded && velocity.y < 0f)
        {
            velocity.y = 0f;
            isFalling = false;
            animator.SetBool("isGrounded", true);
            animator.SetBool("isFalling", false);
        }

        velocity.y += Physics.gravity.y * Time.deltaTime;

        if (!isGrounded && !isFalling && velocity.y < 0f)
        {
            isFalling = true;
            animator.SetBool("isGrounded", false);
            animator.SetBool("isFalling", true);
        }

        float currentSpeed = PlayerStats.Instance != null ? PlayerStats.Instance.Speed : moveSpeed;
        Vector3 move = horizontalMove * currentSpeed + Vector3.up * velocity.y;
        controller.Move(move * Time.deltaTime);

        animator?.SetFloat("speed", isRunning ? 1f : 0f);
    }

    private void UpdateGrounded()
    {
        float sphereCastRadius = controller.radius - 0.1f;
        Vector3 sphereCastOrigin = transform.position + new Vector3(0f, controller.radius, 0f);

        bool isGroundBelow = Physics.SphereCast(
            sphereCastOrigin,
            sphereCastRadius,
            Vector3.down,
            out RaycastHit hitInfo,
            1000f,
            groundLayerMask,
            QueryTriggerInteraction.Ignore);

        if (isGroundBelow)
        {
            float distanceToGround = transform.position.y - hitInfo.point.y;
            isGrounded = distanceToGround <= groundDistanceTolerance;
        }
        else
        {
            isGrounded = false;
        }
    }
}
