using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController2D : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform groundCheck;        // child at feet
    [SerializeField] LayerMask groundMask;

    [Header("Run")]
    [SerializeField] float maxRunSpeed = 12f;
    [SerializeField] float runAccel = 90f;         // accel on ground
    [SerializeField] float runDecel = 100f;        // decel on ground
    [SerializeField, Range(0f,1f)] float airControl = 0.6f;

    [Header("Jump")]
    [SerializeField] float jumpVelocity = 16f;
    [SerializeField] float coyoteTime = 0.1f;      // grace after leaving ledge
    [SerializeField] float jumpBuffer = 0.12f;     // press jump slightly early

    [Header("Better Jump")]
    [SerializeField] float fallGravityMul = 2.0f;  // extra gravity when falling
    [SerializeField] float jumpCutMul = 2.5f;      // extra gravity when jump released early
    [SerializeField] float maxFallSpeed = -25f;
    [SerializeField] float fastFallMul = 2.2f;     // down key pressed

    [Header("Ground Check")]
    [SerializeField] float groundCheckRadius = 0.18f;

    public enum InputMode { Keyboard, Touch }

    [Header("Input Source")]
    [SerializeField] InputMode inputMode = InputMode.Keyboard;

    Rigidbody2D rb;
    IPlayerInput input;

    // state
    bool isGrounded;
    float lastGroundedTime;
    float lastJumpPressedTime;

    // cached
    const float EPS = 0.001f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    void Start()
    {
        SwitchInput(inputMode);
    }

    void SwitchInput(InputMode mode)
    {
        inputMode = mode;
        input = (mode == InputMode.Keyboard) ? (IPlayerInput)new KeyboardInput()
                                             : (IPlayerInput)new TouchInput();
    }

    void Update()
    {
        // Swap inputs at runtime (optional, for testing)
        if (Application.isEditor)
        {
            if (Input.GetKeyDown(KeyCode.F1)) SwitchInput(InputMode.Keyboard);
            if (Input.GetKeyDown(KeyCode.F2)) SwitchInput(InputMode.Touch);
        }

        input.UpdateInput();

        // Timers for coyote & buffer
        if (IsOnGround()) { isGrounded = true; lastGroundedTime = coyoteTime; }
        else              { isGrounded = false; lastGroundedTime -= Time.deltaTime; }

        if (input.JumpPressed) lastJumpPressedTime = jumpBuffer;
        else                   lastJumpPressedTime -= Time.deltaTime;

        // Try to jump in Update (read inputs here), apply in FixedUpdate via velocity set
        if (CanJump())
        {
            DoJump();
        }

        // Variable jump: if player releases jump early and is rising, apply extra gravity
        if (!isGrounded && rb.linearVelocity.y > 0f && !input.JumpHeld)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y + (jumpCutMul -1f) * Physics2D.gravity.y * Time.deltaTime);
        }

        // Fast fall
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            // Extra gravity when pressing down (keyboard only; on touch you could map a two-finger hold)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x,
                Mathf.Max(maxFallSpeed, rb.linearVelocity.y + Physics2D.gravity.y * (fastFallMul - 1f) * Time.deltaTime));
        }
    }

    void FixedUpdate()
    {
        // Horizontal movement
        float targetSpeed = input.MoveX * maxRunSpeed;
        float speedDif = targetSpeed - rb.linearVelocity.x;

        float accelRate = 0f;
        if (Mathf.Abs(targetSpeed) > EPS)
            accelRate = isGrounded ? runAccel : runAccel * airControl;
        else
            accelRate = isGrounded ? runDecel : runDecel * airControl;

        float movement = speedDif * accelRate * Time.fixedDeltaTime;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x + movement, rb.linearVelocity.y);

        // Better fall gravity (when moving downward)
        if (rb.linearVelocity.y < -EPS)
        {
            float extraG = (fallGravityMul - 1f) * Physics2D.gravity.y * Time.fixedDeltaTime;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Max(maxFallSpeed, rb.linearVelocity.y + extraG));
        }
    }

    bool CanJump()
    {
        return lastGroundedTime > 0f && lastJumpPressedTime > 0f;
    }

    void DoJump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpVelocity);
        lastJumpPressedTime = 0f;
        lastGroundedTime = 0f;
    }

    bool IsOnGround()
    {
        if (!groundCheck) return false;
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundMask);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (groundCheck)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
#endif
}
