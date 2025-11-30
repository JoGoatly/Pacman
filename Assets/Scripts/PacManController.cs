using UnityEngine;

public class PacManController : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Vector2 currentDirection = Vector2.zero;
    private Vector2 nextDirection = Vector2.zero;

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;

    [Header("Audio")]
    public AudioClip pelletSound;
    public AudioClip powerPelletSound;
    public AudioClip deathSound; // Spielt nur wenn wirklich Game Over (letztes Leben)
    public AudioClip hitSound; // Spielt wenn getroffen aber noch Leben übrig
    private AudioSource audioSource;

    private bool canMove = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();

        // Falls kein AudioSource vorhanden, erstelle einen
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Rigidbody2D auf Kinematic setzen - WICHTIG!
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
    }

    void Update()
    {
        // Warte bis Spiel startet
        if (!canMove) return;
        // Input abfragen
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            nextDirection = Vector2.up;
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            nextDirection = Vector2.down;
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            nextDirection = Vector2.left;
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            nextDirection = Vector2.right;

        // Versuche Richtungswechsel
        if (nextDirection != Vector2.zero && CanMove(nextDirection))
        {
            currentDirection = nextDirection;
        }

        // Bewege Pac-Man kontinuierlich
        if (currentDirection != Vector2.zero && CanMove(currentDirection))
        {
            Vector2 movement = currentDirection * moveSpeed * Time.deltaTime;
            transform.position = (Vector2)transform.position + movement;
        }

        // Animation und Rotation
        UpdateVisuals();
    }

    bool CanMove(Vector2 direction)
    {
        Vector2 startPos = transform.position;
        Vector2 targetPos = startPos + direction * 0.5f;

        // Raycast zur Wanderkennung
        RaycastHit2D hit = Physics2D.Linecast(startPos, targetPos, LayerMask.GetMask("Wall"));
        return hit.collider == null;
    }

    void UpdateVisuals()
    {
        if (currentDirection == Vector2.zero) return;

        // Rotation basierend auf Richtung
        float angle = Mathf.Atan2(currentDirection.y, currentDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    public Vector2 GetCurrentDirection()
    {
        return currentDirection;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Pellet"))
        {
            Destroy(other.gameObject);
            GameManager.Instance.AddScore(10);

            // Spiele Pellet Sound
            if (pelletSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(pelletSound);
            }
        }
        else if (other.CompareTag("PowerPellet"))
        {
            Destroy(other.gameObject);
            GameManager.Instance.AddScore(50);
            GameManager.Instance.ActivatePowerMode();

            // Spiele Power Pellet Sound
            if (powerPelletSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(powerPelletSound);
            }
        }
        else if (other.CompareTag("Ghost"))
        {
            GhostController ghost = other.GetComponent<GhostController>();
            if (ghost.IsVulnerable())
            {
                ghost.GetEaten();
                GameManager.Instance.AddScore(200);
            }
            else if (canMove) // Geändert von isRespawning zu canMove
            {
                OnHit();
            }
        }
    }

    void OnHit()
    {
        // Stoppe Bewegung komplett
        // Stoppe Bewegung komplett
        canMove = false;
        currentDirection = Vector2.zero;
        nextDirection = Vector2.zero;

        // Setze Velocity auf 0 - WICHTIG gegen Rumfliegen!
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        // Prüfe ob letztes Leben
        bool isLastLife = GameManager.Instance.lives <= 1;

        if (isLastLife)
        {
            // Letztes Leben - Death Sound
            if (deathSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(deathSound);
            }
        }
        else
        {
            // Noch Leben übrig - Hit Sound
            if (hitSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(hitSound);
            }
        }

        // Benachrichtige GameManager
        GameManager.Instance.PlayerDied();
    }

    public void ResetPlayer()
    {
        canMove = false;
        currentDirection = Vector2.zero;
        nextDirection = Vector2.zero;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    public void EnableMovement()
    {
        canMove = true;
    }
}