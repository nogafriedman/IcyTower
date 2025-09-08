using UnityEngine;

public class PlayerController2D : MonoBehaviour
{
    public ScoreManager scoreManager;
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
        isGrounded = groundCheck && Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayers);
        if (Input.GetButtonDown("Jump") && isGrounded)
            jump = true;

        scoreManager.UpdateComboTimeout();
    }

    private void FixedUpdate()
    {
        float inputX = Input.GetAxis("Horizontal");
        float currSpeedX = rb.linearVelocity.x;

        // Horizontal movement
        if (Mathf.Abs(inputX * currSpeedX) < maxSpeed)
        // if (Mathf.Abs(rb.linearVelocity.x) < maxSpeed)

        {
            Vector2 force = new Vector2(inputX * moveAcceleration, 0f);
            rb.AddForce(force);
            // rb.AddForce(inputX * moveAcceleration * Vector2.right);
            // rb.linearVelocity = new Vector2(inputX * maxSpeed, rb.linearVelocity.y);
        }

        if (Mathf.Abs(inputX) <= 0.05f)
        {
            float decelRate = 40f; // tweak until it feels right
            rb.linearVelocity = new Vector2(
                Mathf.MoveTowards(rb.linearVelocity.x, 0f, decelRate * Time.fixedDeltaTime),
                rb.linearVelocity.y
            );
            // rb.linearVelocity = new Vector2(0, rb.linearVelocityY);
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (IsInLayerMask(collision.gameObject, wallLayers))
        {
            Vector2 normalVector = collision.GetContact(0).normal;
            Vector2 CurrVelocity = rb.linearVelocity;

            Debug.Log("Collision with wall, current score: " + scoreManager.CurrentScore);
            Vector2 r = Vector2.Reflect(CurrVelocity, normalVector) * wallBounceMultiplier;
            rb.linearVelocity = r;
        }

        // Collision with platform (update score):
        if (IsInLayerMask(collision.gameObject, groundLayers) &&
            collision.gameObject.TryGetComponent<PlatformIndex>(out var p))
        {
            int idx = (int)p.floorIndex;
            scoreManager.UpdateState(idx);
        }
    }
}