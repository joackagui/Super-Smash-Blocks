using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [System.Serializable]
    public class GameplayInputBindings
    {
        public InputActionReference movement;
        public InputActionReference jump;
        public InputActionReference action1;
        public InputActionReference action2;
    }

    [Header("State")]
    [SerializeField] private float damageTaken;
    [SerializeField] private float damageDone;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private LayerMask groundLayers = ~0;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.25f;

    [Header("Facing (Y Rotation)")]
    [SerializeField] private float faceRightY = 90f;
    [SerializeField] private float faceLeftY = 270f;

    private GameplayInputBindings inputBindings;
    private Character character;
    private Rigidbody rb;
    private float moveInput;
    private bool jumpRequested;
    private bool initialized;

    private void Awake()
    {
        character = GetComponentInChildren<Character>();
        rb = GetComponent<Rigidbody>();

        if (groundCheck == null)
        {
            groundCheck = transform;
        }
    }

    private void OnEnable()
    {
        BindInputs();
    }

    private void OnDisable()
    {
        UnbindInputs();
    }

    private void Update()
    {
        ApplyFacing();
    }

    private void FixedUpdate()
    {
        ApplyHorizontalMovement();
        TryJump();
    }

    public void Initialize(Character assignedCharacter, GameplayInputBindings bindings)
    {
        character = assignedCharacter != null ? assignedCharacter : GetComponentInChildren<Character>();
        inputBindings = bindings;
        initialized = true;

        UnbindInputs();
        BindInputs();
    }

    public float GetDamageTaken()
    {
        return damageTaken;
    }

    public float GetDamageDone()
    {
        return damageDone;
    }

    public void AddDamageTaken(float amount)
    {
        damageTaken += Mathf.Max(0f, amount);
    }

    public void AddDamageDone(float amount)
    {
        damageDone += Mathf.Max(0f, amount);
    }

    private void BindInputs()
    {
        if (!initialized || inputBindings == null)
        {
            return;
        }

        BindAction(inputBindings.movement, OnMovePerformed, OnMoveCanceled, true);
        BindAction(inputBindings.jump, OnJumpPerformed);
        BindAction(inputBindings.action1, OnAction1Performed);
        BindAction(inputBindings.action2, OnAction2Performed);
    }

    private void UnbindInputs()
    {
        if (inputBindings == null)
        {
            return;
        }

        UnbindAction(inputBindings.movement, OnMovePerformed, OnMoveCanceled);
        UnbindAction(inputBindings.jump, OnJumpPerformed);
        UnbindAction(inputBindings.action1, OnAction1Performed);
        UnbindAction(inputBindings.action2, OnAction2Performed);
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        Vector2 axis = context.ReadValue<Vector2>();
        moveInput = axis.x;
        character?.OnMove(moveInput);
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        moveInput = 0f;
        character?.OnMove(0f);
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        jumpRequested = true;
    }

    private void OnAction1Performed(InputAction.CallbackContext context)
    {
        character?.OnAction1();
    }

    private void OnAction2Performed(InputAction.CallbackContext context)
    {
        character?.OnAction2();
    }

    private void ApplyHorizontalMovement()
    {
        float velocityX = moveInput * moveSpeed;

        if (rb != null)
        {
            Vector3 velocity = rb.linearVelocity;
            velocity.x = velocityX;
            rb.linearVelocity = velocity;
            return;
        }

        Vector3 movement = new Vector3(velocityX * Time.fixedDeltaTime, 0f, 0f);
        transform.position += movement;
    }

    private void TryJump()
    {
        if (!jumpRequested)
        {
            return;
        }

        jumpRequested = false;
        if (!IsGrounded())
        {
            return;
        }

        if (rb != null)
        {
            Vector3 velocity = rb.linearVelocity;
            velocity.y = 0f;
            rb.linearVelocity = velocity;
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
        else
        {
            transform.position += Vector3.up * (jumpForce * Time.fixedDeltaTime);
        }

        character?.OnJump();
    }

    private bool IsGrounded()
    {
        return Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayers, QueryTriggerInteraction.Ignore);
    }

    private void ApplyFacing()
    {
        if (moveInput > 0.01f)
        {
            transform.rotation = Quaternion.Euler(0f, faceRightY, 0f);
        }
        else if (moveInput < -0.01f)
        {
            transform.rotation = Quaternion.Euler(0f, faceLeftY, 0f);
        }
    }

    private static void BindAction(
        InputActionReference actionReference,
        System.Action<InputAction.CallbackContext> onPerformed,
        System.Action<InputAction.CallbackContext> onCanceled = null,
        bool readContinuous = false)
    {
        if (actionReference == null || actionReference.action == null)
        {
            return;
        }

        actionReference.action.performed += onPerformed;
        if (onCanceled != null)
        {
            actionReference.action.canceled += onCanceled;
        }

        if (readContinuous)
        {
            actionReference.action.Enable();
        }
        else
        {
            actionReference.action.Enable();
        }
    }

    private static void UnbindAction(
        InputActionReference actionReference,
        System.Action<InputAction.CallbackContext> onPerformed,
        System.Action<InputAction.CallbackContext> onCanceled = null)
    {
        if (actionReference == null || actionReference.action == null)
        {
            return;
        }

        actionReference.action.performed -= onPerformed;
        if (onCanceled != null)
        {
            actionReference.action.canceled -= onCanceled;
        }
    }
}
