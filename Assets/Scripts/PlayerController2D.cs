using UnityEngine;

public class PlayerController2D : MonoBehaviour
{

    private Rigidbody2D rb;

    [Header("Movement")]
    public float moveAcceleration = 365f;
    public float maxSpeed = 5f;

    [Header("Jumping")]
    private bool jump = false;
    public float jumpImpulse = 1000f;
    public float HorizontalJumpBonus = 100f;
    public float maxJumpImpulse = 1500f;

    [Header("Walls")]
    public float wallBounceMultiplier = 1.25f;

    [Header("GroundCheck")]
    private bool isGrounded;
    private int groundMask;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.12f;

    [Header("Layers")]
    [SerializeField] private LayerMask groundLayers;
    [SerializeField] private LayerMask wallLayers;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        groundMask = LayerMask.GetMask("Ground");
    }

    private void Update()
    {
        isGrounded = groundCheck &&
                    Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayers);

        if (Input.GetButtonDown("Jump") && isGrounded)
            jump = true;
    }

    private void FixedUpdate()
    {
        float inputX = Input.GetAxis("Horizontal");
        float currSpeedX = rb.linearVelocity.x;

        // Horizontal movement
        if (Mathf.Abs(inputX * currSpeedX) < maxSpeed)
        {
            Vector2 force = new Vector2(inputX * moveAcceleration, 0f);
            rb.AddForce(force);
        }

        // Stop if no input
        if (Mathf.Abs(inputX) <= 0.05f)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }

        if (jump)
        {
            float horizontalBonus = Mathf.Abs(currSpeedX) * HorizontalJumpBonus;
            float totalJumpPower = jumpImpulse + horizontalBonus;

            rb.AddForce(Vector2.up * totalJumpPower);

            jump = false;
        }
    }

    private bool IsInLayerMask(GameObject obj, LayerMask mask)
    {
        return (mask.value & (1 << obj.layer)) != 0;
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (IsInLayerMask(col.gameObject, wallLayers))
        {
            // {
            //     Vector2 rev = new Vector2(rb.linearVelocity.x * wallBounceMultiplier, 0f);
            //     rb.AddForce(rev, ForceMode2D.Impulse);
            // }
            {
                Vector2 n = col.GetContact(0).normal;  // wall surface normal
                Vector2 v = rb.linearVelocity;               // player's current velocity

                if (Vector2.Dot(v, n) < 0f)            // only if moving into the wall
                    rb.linearVelocity = Vector2.Reflect(v, n) * wallBounceMultiplier;
            }
        }
    }
}