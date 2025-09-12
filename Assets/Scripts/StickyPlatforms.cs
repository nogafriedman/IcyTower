using UnityEngine;

public class StickyPlatforms : MonoBehaviour
{
    public float stickyDuration = 2f;
    private Rigidbody2D stickyRb;
    private float stickyTimer = 0f;
    private bool isSticky = false;
    private PlayerController2D playerController;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Rigidbody2D rb = collision.gameObject.GetComponent<Rigidbody2D>();

        if (rb != null && !isSticky)
        {
            float playerBottom = rb.transform.position.y - rb.GetComponent<Collider2D>().bounds.extents.y;
            float platformTop = transform.position.y + GetComponent<Collider2D>().bounds.extents.y;

            if (playerBottom >= platformTop - 0.05f)
            {
                stickyRb = rb;
                stickyTimer = stickyDuration;
                isSticky = true;
                playerController = rb.GetComponent<PlayerController2D>();

                if (playerController != null)
                {
                    playerController.enabled = false;
                }

                rb.linearVelocity = Vector2.zero;
                rb.gravityScale = 0f;
            }
        }
    }

    private void Update()
    {
        if (isSticky && stickyRb != null)
        {
            stickyTimer -= Time.deltaTime;
            if (stickyTimer <= 0f)
            {
                if (playerController != null)
                {
                    playerController.enabled = true;
                    playerController = null;
                }

                stickyRb.gravityScale = 1f;
                stickyRb = null;
                isSticky = false;
            }
        }
    }

    public void ResetSticky()
    {
        if (stickyRb != null)
        {
            if (playerController != null)
            {
                playerController.enabled = true;
                playerController = null;
            }
            stickyRb.gravityScale = 1f;
            stickyRb = null;
        }
        isSticky = false;
        stickyTimer = 0f;
    }

}
