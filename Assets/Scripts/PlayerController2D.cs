using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController2D : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] Transform groundCheck;
    [SerializeField] LayerMask groundLayers = 0;

    [Header("Ground Check")]
    [SerializeField] Vector2 groundCheckSize = new Vector2(0.6f, 0.1f);

    [Header("Run")]
    [SerializeField] float moveSpeed = 12f;        // target horizontal speed at full stick
    [SerializeField] float accel = 70f;            // when pressing towards target
    [SerializeField] float decel = 80f;            // when no input or opposite
    [SerializeField] float airControl = 0.7f;      // percentage of accel/decel allowed in air
    [SerializeField] float apexHang = 0.25f;       // 0..0.4: slight slowdown near jump apex
    [SerializeField] float maxRunSpeed = 16f;      // safety clamp (mobile frame hitches)

    [Header("Jump")]
    [SerializeField] float jumpForce = 16f;        // impulse when starting a jump
    [SerializeField] float coyoteTime = 0.12f;     // after leaving ground
    [SerializeField] float jumpBuffer = 0.12f;     // before landing
    [SerializeField] float baseGravityScale = 3.3f;// baseline gravity
    [SerializeField] float fallGravityMul = 2.2f;  // extra gravity when falling
    [SerializeField] float jumpCutMul = 2.8f;      // extra gravity when jump released early
    [SerializeField] float maxFallSpeed = 28f;     // terminal velocity clamp

    [Header("Debug")]
    [SerializeField] bool drawGizmos = true;

    // Public read-only state (useful for UI/FX)
    public bool Grounded { get; private set; }
    public Vector2 Velocity => rb.linearVelocity;
    public System.Action OnLand;

    // Internals
    Rigidbody2D rb;
    bool wasGrounded;
    float lastGroundedTime = -999f;
    float lastJumpPressedTime = -999f;

    // cached input (filled by feeders each frame)
    float inputX;
    bool jumpHeld;

    // ================= LIFECYCLE =================
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = baseGravityScale;
    }

    /// <summary>Feed inputs from any source once per frame.</summary>
    public void SetInput(float moveX, bool jumpDown, bool jumpIsHeld)
    {
        inputX = Mathf.Clamp(moveX, -1f, 1f);
        if (jumpDown) lastJumpPressedTime = Time.time; // buffer the press time
        jumpHeld = jumpIsHeld;
    }

    void Update()
    {
        // 1) Ground check (do in Update for snappier events)
        Grounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayers);
        if (Grounded) lastGroundedTime = Time.time;
        if (Grounded && !wasGrounded) OnLand?.Invoke();
        wasGrounded = Grounded;

        // 2) Gravity shaping for variable jump & fast fall (frame-rate independent)
        ApplyBetterJumpGravity();
    }

    void FixedUpdate()
    {
        HandleRun();
        TryConsumeJump();
        ClampFallSpeed();
    }

    // ================= MOVEMENT =================
    void HandleRun()
    {
        float target = inputX * moveSpeed;

        // Apex hang: reduce effective target speed when near top of jump for precision control
        float verticalSpeed = Mathf.Abs(rb.linearVelocity.y);
        float nearApex01 = 1f - Mathf.InverseLerp(0f, 2f, verticalSpeed); // 1 near apex, 0 when moving fast vertically
        float apexFactor = 1f - nearApex01 * apexHang;                    // reduce target near apex
        if (!Grounded) target *= apexFactor;

        float accelRate = (Mathf.Abs(target) > 0.01f) ? accel : decel;
        if (!Grounded) accelRate *= airControl;

        float speedDiff = target - rb.linearVelocity.x;
        float force = speedDiff * accelRate * Time.fixedDeltaTime;
        rb.AddForce(new Vector2(force, 0f), ForceMode2D.Force);

        // safety clamp
        rb.linearVelocity = new Vector2(Mathf.Clamp(rb.linearVelocity.x, -maxRunSpeed, maxRunSpeed), rb.linearVelocity.y);
    }

    // ================= JUMPING =================
    void TryConsumeJump()
    {
        bool canCoyote = (Time.time - lastGroundedTime) <= coyoteTime;
        bool hasBuffered = (Time.time - lastJumpPressedTime) <= jumpBuffer;

        if (hasBuffered && canCoyote)
        {
            // reset vertical for consistent jump height
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

            // consume buffer
            lastJumpPressedTime = -999f;
        }
    }

    void ApplyBetterJumpGravity()
    {
        // jump cut: if rising and jump is released, increase gravity
        if (rb.linearVelocity.y > 0f && !jumpHeld)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (jumpCutMul - 1f) * Time.deltaTime * rb.gravityScale;
        }
        // faster fall
        if (rb.linearVelocity.y < 0f)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallGravityMul - 1f) * Time.deltaTime * rb.gravityScale;
        }
    }

    void ClampFallSpeed()
    {
        if (rb.linearVelocity.y < -maxFallSpeed)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -maxFallSpeed);
    }

    // ================= DEBUG =================
    void OnDrawGizmosSelected()
    {
        if (!drawGizmos || !groundCheck) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
    }
}
