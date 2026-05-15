using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;


public class CharacterMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;

    [SerializeField] private float groundDistanceTolerance = 0.1f;
    [SerializeField] private float fallDelay = 0.15f;
    [SerializeField] private float fallMinHeight = 0.8f;
    [SerializeField] private LayerMask groundLayerMask;

    private Animator animator;
    private CharacterController controller;
    private Camera mainCamera;
    private bool isFalling;
    private float distanceToGround;
    private float notGroundedTime;
    public bool IsGrounded => isGrounded;
    private bool isGrounded;
    private Vector3 velocity;
    private Vector2 moveInput;
    public bool IsLocked { get; set; }
    public bool LockInput { get; set; }
    public float GravityMultiplier { get; set; } = 1f;

    public void ResetVerticalVelocity() => velocity.y = 0f;

    void Start()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        mainCamera = Camera.main;

        isGrounded = true;
        animator.SetBool("isGrounded", true);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        mainCamera = Camera.main;
    }

    public void ForceUnground()
    {
        isGrounded = false;
        isFalling = false;
        velocity.y = 0f;
        animator.SetBool("isGrounded", false);
    }

    void Update()
    {
        UpdateGrounded();

        if (IsLocked || (DialogueManager.Instance != null && DialogueManager.Instance.IsActive))
        {
            animator?.SetFloat("speed", 0f);
            return;
        }

        if (!LockInput)
        {
            var kb = Keyboard.current;
            if (kb != null)
            {
                float h = (kb.dKey.isPressed ? 1f : 0f) - (kb.aKey.isPressed ? 1f : 0f);
                float v = (kb.wKey.isPressed ? 1f : 0f) - (kb.sKey.isPressed ? 1f : 0f);
                moveInput = new Vector2(h, v);
            }
        }
        else
        {
            moveInput = Vector2.zero;
        }

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

        if (isGrounded)
        {
            if (velocity.y < 0f) velocity.y = 0f;
            isFalling = false;
            animator.SetBool("isGrounded", true);
            animator.SetBool("isFalling", false);
        }

        velocity.y += Physics.gravity.y * GravityMultiplier * Time.deltaTime;

        if (!isGrounded)
            notGroundedTime += Time.deltaTime;
        else
            notGroundedTime = 0f;

        bool nearGround = Physics.Raycast(transform.position + Vector3.up * 5f, Vector3.down, fallMinHeight + 5f, groundLayerMask, QueryTriggerInteraction.Ignore);
        if (!isGrounded && !isFalling && velocity.y < 0f && notGroundedTime > fallDelay && !nearGround)
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
        float rayOffset = 5f;
        Vector3 rayOrigin = transform.position + Vector3.up * rayOffset;

        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hitInfo, 100f, groundLayerMask, QueryTriggerInteraction.Ignore))
            distanceToGround = hitInfo.distance - rayOffset;
        else
            distanceToGround = float.MaxValue;

        isGrounded = controller.isGrounded || distanceToGround <= groundDistanceTolerance;
    }
}
