using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController2D : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform groundCheck;        // child at feet
    [SerializeField] LayerMask groundMask;         // include your Ground layer

    [Header("Run")]
    [SerializeField] float maxRunSpeed = 12f;
    [SerializeField] float runAccel = 90f;         // accel on ground
    [SerializeField] float runDecel = 100f;        // decel on ground
    [SerializeField, Range(0f, 1f)] float airControl = 0.6f;

    [Header("Jump")]
    [SerializeField] float jumpVelocity = 16f;     // initial jump takeoff speed
    [SerializeField] float coyoteTime = 0.10f;     // grace after leaving ledge
    [SerializeField] float jumpBuffer = 0.12f;     // press jump a little early

    [Header("Better Jump")]
    [SerializeField] float fallGravityMul = 2.0f;  // extra gravity when falling
    [SerializeField] float jumpCutMul = 2.5f;      // extra gravity when jump is released early
    [SerializeField] float maxRiseSpeed = 20f;     // safety clamp
    [SerializeField] float maxFallSpeed = -25f;    // safety clamp (negative)
    [SerializeField] float fastFallMul = 2.2f;     // keyboard down to fall faster

    [Header("Ground Check")]
    [SerializeField] float groundCheckRadius = 0.18f;

    [Header("Classic Wall Bounce (Icy Tower)")]
    [SerializeField] float bounceElasticity = 1.0f;   // 1 = keep |vx|, <1 softens
    [SerializeField] float minBounceSpeed = 9f;       // ensure you don't die on the wall
    [SerializeField] float wallNudgeUp = 2.5f;        // tiny upward kick on bounce
    [SerializeField] float wallBounceCooldown = 0.08f;
    float lastBounceTime = -999f;

    public enum InputMode { Keyboard, Touch }

    [Header("Input Source")]
    [SerializeField] InputMode inputMode = InputMode.Keyboard;

    Rigidbody2D rb;
    IPlayerInput input;

    // state
    bool isGrounded;
    float lastGroundedTime;       // counts down from coyoteTime
    float lastJumpPressedTime;    // counts down from jumpBuffer

    const float EPS = 0.0001f;

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
        input = (mode == InputMode.Keyboard)
            ? (IPlayerInput)new KeyboardInput()
            : (IPlayerInput)new TouchInput();
    }

    void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.F1)) SwitchInput(InputMode.Keyboard);
        if (Input.GetKeyDown(KeyCode.F2)) SwitchInput(InputMode.Touch);
#endif
        input.UpdateInput();

        // Ground check + timers
        if (IsOnGround())
        {
            isGrounded = true;
            lastGroundedTime = coyoteTime;
        }
        else
        {
            isGrounded = false;
            lastGroundedTime -= Time.deltaTime;
        }

        if (input.JumpPressed) lastJumpPressedTime = jumpBuffer;
        else                   lastJumpPressedTime -= Time.deltaTime;

        // Jump (uses coyote + buffer)
        if (CanJump())
        {
            DoJump();
        }

        // Variable jump height: if rising and jump released, add extra downward accel
        if (!isGrounded && rb.linearVelocity.y > 0f && !input.JumpHeld)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                rb.linearVelocity.y + (jumpCutMul - 1f) * Physics2D.gravity.y * Time.deltaTime
            );
        }

        // Optional fast-fall on keyboard (down key)
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                Mathf.Max(maxFallSpeed, rb.linearVelocity.y + Physics2D.gravity.y * (fastFallMul - 1f) * Time.deltaTime)
            );
        }
    }

    void FixedUpdate()
    {
        // Horizontal movement (accel/decel + air control)
        float targetSpeed = input.MoveX * maxRunSpeed;
        float speedDif = targetSpeed - rb.linearVelocity.x;

        float accelRate;
        if (Mathf.Abs(targetSpeed) > EPS)
            accelRate = isGrounded ? runAccel : runAccel * airControl;
        else
            accelRate = isGrounded ? runDecel : runDecel * airControl;

        float movement = speedDif * accelRate * Time.fixedDeltaTime;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x + movement, rb.linearVelocity.y);

        // Better fall gravity (stronger downward)
        if (rb.linearVelocity.y < -EPS)
        {
            float extraG = (fallGravityMul - 1f) * Physics2D.gravity.y * Time.fixedDeltaTime; // negative
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                Mathf.Max(maxFallSpeed, rb.linearVelocity.y + extraG)
            );
        }

        // Safety clamp for rise/fall
        rb.linearVelocity = new Vector2(
            rb.linearVelocity.x,
            Mathf.Clamp(rb.linearVelocity.y, maxFallSpeed, maxRiseSpeed)
        );
    }

    // ======== Classic Icy Tower wall bounce ========
    void OnCollisionEnter2D(Collision2D col) { ClassicWallBounce(col); }
    void OnCollisionStay2D(Collision2D col)  { ClassicWallBounce(col); }

    void ClassicWallBounce(Collision2D col)
    {
        if (Time.time < lastBounceTime + wallBounceCooldown) return;
        if (col.contactCount == 0) return;

        var cp = col.GetContact(0);
        Vector2 n = cp.normal; // normal points from surface -> player

        // Only respond to side walls (ignore floor/ceiling)
        if (Mathf.Abs(n.x) < 0.7f) return;

        // Only if moving into the wall (approach speed > 0)
        float approach = Vector2.Dot(rb.linearVelocity, -n);
        if (approach <= 0f) return;

        // Flip horizontal velocity, keep magnitude (elasticity) and ensure minimum push
        float speedX = Mathf.Max(minBounceSpeed, Mathf.Abs(rb.linearVelocity.x)) * bounceElasticity;
        float newVx = Mathf.Sign(n.x) * speedX; // n.x>0 => left wall -> push right

        // Keep vertical, add a small upward nudge so it feels lively
        float newVy = rb.linearVelocity.y;
        if (newVy < wallNudgeUp) newVy += wallNudgeUp;

        rb.linearVelocity = new Vector2(newVx, newVy);
        lastBounceTime = Time.time;
    }

    // ======== Helpers ========
    bool CanJump() => lastGroundedTime > 0f && lastJumpPressedTime > 0f;

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

    // ======== Input abstraction (old Input system) ========
    public interface IPlayerInput
    {
        float MoveX { get; }
        bool JumpPressed { get; }
        bool JumpHeld { get; }
        void UpdateInput();
    }

    class KeyboardInput : IPlayerInput
    {
        public float MoveX { get; private set; }
        public bool JumpPressed { get; private set; }
        public bool JumpHeld { get; private set; }

        public void UpdateInput()
        {
            MoveX = Input.GetAxisRaw("Horizontal"); // old Input system axes
            JumpPressed = Input.GetKeyDown(KeyCode.Space);
            JumpHeld = Input.GetKey(KeyCode.Space);
        }
    }

    class TouchInput : IPlayerInput
    {
        public float MoveX { get; private set; }
        public bool JumpPressed { get; private set; }
        public bool JumpHeld { get; private set; }

        // Simple zones: left 40% move left, right 40% move right; quick tap upper half = jump
        const float tapMaxDuration = 0.18f;
        const float tapMaxMove = 30f; // pixels

        public void UpdateInput()
        {
            MoveX = 0f;
            JumpPressed = false;
            JumpHeld = false;

            for (int i = 0; i < Input.touchCount; i++)
            {
                var t = Input.GetTouch(i);
                var pos = t.position;

                if (pos.x < Screen.width * 0.4f) MoveX = Mathf.Min(MoveX, -1f);
                else if (pos.x > Screen.width * 0.6f) MoveX = Mathf.Max(MoveX, 1f);

                if (t.phase == TouchPhase.Stationary || t.phase == TouchPhase.Moved)
                    JumpHeld = true;

                if (t.phase == TouchPhase.Ended)
                {
                    bool smallMove = (t.deltaPosition.magnitude <= tapMaxMove);
                    bool upperHalf = (pos.y > Screen.height * 0.5f);
                    if ((t.tapCount > 0 || smallMove) && upperHalf && t.deltaTime <= tapMaxDuration)
                        JumpPressed = true;
                }
            }
        }
    }
}

