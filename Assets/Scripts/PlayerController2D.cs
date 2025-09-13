using System.Collections;
using UnityEngine;

public class PlayerController2D : MonoBehaviour
{
    [SerializeField] private PowerUpSpawner2D[] powerUpSpawners;
    public ScoreManager scoreManager;
    private Rigidbody2D rb;

    [Header("Movement")]
    public float moveAcceleration = 360f;
    public float maxSpeed = 5f;
    private float baseMoveAcceleration;
    private float baseMaxSpeed;
    private Coroutine moveBoostRoutine;

    [Header("Jumping")]
    private bool jump = false;
    public float jumpImpulse = 1200f;
    public float maxJumpImpulse = 1500f;
    public float HorizontalJumpBonus = 100f;
    public float maxHorizontalBonus = 200f;

    [Header("Airtime / Floatiness")]
    // Gravity scale while ascending and jump is still held (< 1 = floatier)
    public float ascendGravityScale = 0.75f;
    // Gravity scale while falling (≈1 stays floaty; >1 falls faster)
    public float fallGravityScale = 1.5f;
    // Extra upward force time while holding Jump after takeoff
    public float jumpSustainTime = 0.12f;
    // FixedUpdate upward force during sustain
    public float jumpSustainForce = 18f;
    private float sustainTimer;

    [Header("Walls")]
    public float wallBounceMultiplier = 1.25f;
    public float maxBounceSpeed = 8f;

    [Header("GroundCheck")]
    private bool isGrounded;
    private int groundMask;
    private IncreasePlayerSpeed speedBoost;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.12f;

    [Header("Layers")]
    [SerializeField] private LayerMask groundLayers;
    [SerializeField] private LayerMask wallLayers;
    [Header("Jetpack")]
    [SerializeField] private float jetpackThrust = 18f;            // upward force per FixedUpdate while held
    [SerializeField] private float jetpackMaxVerticalSpeed = 8f;    // cap so it doesn't rocket away
    [SerializeField] private float jetpackGravityScale = 0.25f; // low gravity while jetpack runs
    private bool jetpackActive = false;
    private Coroutine jetpackRoutine = null;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (powerUpSpawners == null || powerUpSpawners.Length == 0)
        {
#if UNITY_2023_1_OR_NEWER
            powerUpSpawners = UnityEngine.Object.FindObjectsByType<PowerUpSpawner2D>(FindObjectsSortMode.None);
#else
    powerUpSpawners = UnityEngine.Object.FindObjectsOfType<PowerUpSpawner2D>();
#endif
        }
        groundMask = LayerMask.GetMask("Ground");
        baseMoveAcceleration = moveAcceleration;
        baseMaxSpeed = maxSpeed;

    }

    private void Update()
    {
        isGrounded = groundCheck && Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayers);
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            jump = true;
            sustainTimer = jumpSustainTime;
        }
    }

    private void FixedUpdate()
    {
        float inputX = Input.GetAxisRaw("Horizontal");
        float currSpeedX = rb.linearVelocity.x;

        float accel = isGrounded ? moveAcceleration : moveAcceleration * 0.80f;

        // BOOST: apply boost to movement caps/forces
        float boost = speedBoost != null ? speedBoost.CurrentMultiplier : 1f;
        float boostedMaxSpeed = maxSpeed * boost;
        float boostedAcceleration = moveAcceleration * boost;

        if (Mathf.Abs(inputX) > 0.05f)
        {
            rb.AddForce(new Vector2(inputX * accel, 0f));
        }
        else
        {
            // faster decel on ground, gentle in air
            float decel = isGrounded ? 20f : 4f;
            rb.linearVelocity = new Vector2(
                Mathf.MoveTowards(currSpeedX, 0f, decel * Time.fixedDeltaTime),
                rb.linearVelocity.y
            );
        }

        // hard cap X speed (lower cap in air)
        float cap = isGrounded ? maxSpeed : maxSpeed * 0.95f;
        rb.linearVelocity = new Vector2(
            Mathf.Clamp(rb.linearVelocity.x, -cap, cap),
            rb.linearVelocity.y
        );

        if (jump)
        {
            float horizontalBonus = Mathf.Min(Mathf.Abs(rb.linearVelocity.x) * HorizontalJumpBonus, maxHorizontalBonus);
            float totalJumpPower = Mathf.Min(jumpImpulse + horizontalBonus, maxJumpImpulse);

            AudioManager.Instance?.PlayJumpByForce(totalJumpPower, jumpImpulse, maxJumpImpulse);

            rb.AddForce(Vector2.up * totalJumpPower, ForceMode2D.Force);
            jump = false;
        }
        // Don't stack sustain with jetpack
        if (sustainTimer > 0f && Input.GetButton("Jump") && rb.linearVelocity.y > 0f && !jetpackActive)
        {
            rb.AddForce(Vector2.up * jumpSustainForce, ForceMode2D.Force);
            sustainTimer -= Time.fixedDeltaTime;
        }
        // --- Jetpack mode: free flight while active ---
        if (jetpackActive)
        {
            // Light gravity while the pack is on (tune in Inspector)
            rb.gravityScale = jetpackGravityScale;

            // Hold Jump to go up; cap the vertical speed
            if (Input.GetButton("Jump") && rb.linearVelocity.y < jetpackMaxVerticalSpeed)
            {
                rb.AddForce(Vector2.up * jetpackThrust, ForceMode2D.Force);
            }
        }
        else
        {
            // Normal gravity behaviour when jetpack is off
            if (rb.linearVelocity.y > 0f)
                rb.gravityScale = Input.GetButton("Jump") ? ascendGravityScale : fallGravityScale;
            else if (rb.linearVelocity.y < 0f)
                rb.gravityScale = fallGravityScale;
            else
                rb.gravityScale = 1f;
        }

    }

    private bool IsInLayerMask(GameObject obj, LayerMask mask)
    {
        return (mask.value & (1 << obj.layer)) != 0;
    }


// private void OnTriggerEnter2D(Collider2D other)
// {
//     if (!IsInLayerMask(other.gameObject, groundLayers)) return;

//     Debug.Log($"[Player] OnTriggerEnter");

//     // Must be falling (ignore when coming up through
//     //  the effector)
//     if (rb.linearVelocity.y > -0.01f) return;
//     Debug.Log($"[Player] OnTriggerEnter passed falling check");

//     var idxComp = other.GetComponentInParent<PlatformIndex>();
//     if (idxComp == null) return;

//     int idx = idxComp.floorIndex;

//     if (idx == _lastScoredFloor || Time.frameCount == _lastLandingFrame) return;

//     _lastScoredFloor = idx;
//     _lastLandingFrame = Time.frameCount;


//         // Update score and notify spawner
//         scoreManager.UpdateState(idx);
//     if (powerUpSpawner == null) powerUpSpawner = FindObjectOfType<PowerUpSpawner2D>();
//     powerUpSpawner?.NotifyReachedFloor(idx);
// }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (IsInLayerMask(collision.gameObject, wallLayers))
        {
            Vector2 v = new Vector2(rb.linearVelocity.x * wallBounceMultiplier, rb.linearVelocityY * 0.2f);
            rb.AddForce(v, ForceMode2D.Impulse);
        }

        // Collision with platform (update score):
        if (IsInLayerMask(collision.gameObject, groundLayers) && collision.gameObject.TryGetComponent<PlatformIndex>(out var p) && isGrounded)
        {
            int idx = (int)p.floorIndex;
            scoreManager.UpdateState(idx);
            foreach (var s in powerUpSpawners)
            {
                s?.NotifyReachedFloor(idx);
            }
        }
    }
    public void ApplyTemporaryMovementBoost(float multiplier, float durationSeconds)
    {
        if (moveBoostRoutine != null)
        {
            StopCoroutine(moveBoostRoutine);
            ResetMovementToBase();
        }
        moveBoostRoutine = StartCoroutine(DoMovementBoost(multiplier, durationSeconds));
    }

    private IEnumerator DoMovementBoost(float multiplier, float durationSeconds)
    {
        moveAcceleration = baseMoveAcceleration * multiplier;
        maxSpeed = baseMaxSpeed * multiplier;
        yield return new WaitForSeconds(durationSeconds);
        ResetMovementToBase();
        moveBoostRoutine = null;
    }

    private void ResetMovementToBase()
    {
        moveAcceleration = baseMoveAcceleration;
        maxSpeed = baseMaxSpeed;
    }

    public void ApplyJetpack(float durationSeconds)
    {
        if (jetpackRoutine != null) StopCoroutine(jetpackRoutine);
        jetpackRoutine = StartCoroutine(JetpackFor(durationSeconds));
    }

    private IEnumerator JetpackFor(float seconds)
    {
        jetpackActive = true;
        yield return new WaitForSeconds(seconds);
        jetpackActive = false;
        jetpackRoutine = null;
    }


}