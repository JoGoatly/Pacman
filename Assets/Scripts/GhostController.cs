using UnityEngine;
using System.Collections;

public enum GhostType { Blinky, Pinky, Inky, Clyde }

public class GhostController : MonoBehaviour
{
    public GhostType ghostType;
    public float moveSpeed = 3f;
    public float ghostReleaseDelay = 0f; // Zeit bis dieser Geist sich bewegen darf

    private Vector2 direction;
    private Transform player;
    private Vector3 scatterTarget;
    private Vector3 homePosition;

    private bool isVulnerable = false;
    private bool isEaten = false;
    private bool canMove = false; // Neu: Geist kann sich erst nach Delay bewegen
    private float vulnerableTimer = 0f;
    public float vulnerableDuration = 10f; // Jetzt public einstellbar!

    private SpriteRenderer spriteRenderer;
    public Color normalColor;
    public Color vulnerableColor = Color.blue;
    public Color eatenColor = new Color(1f, 1f, 1f, 0.3f); // Halb-transparent
    public float returnSpeed = 6f; // Geschwindigkeit beim Zurückkehren

    private enum GhostMode { Chase, Scatter, Frightened, Eaten }
    private GhostMode currentMode = GhostMode.Chase;

    private float decisionTimer = 0f;
    public float decisionInterval = 0.5f; // Alle 0.5 Sekunden neue Richtung wählen

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        spriteRenderer = GetComponent<SpriteRenderer>();
        homePosition = transform.position;
        normalColor = spriteRenderer.color;

        SetScatterTarget();

        // Starte mit zufälliger Richtung
        direction = GetRandomDirection();

        // Warte auf Release-Signal vom GameManager
        canMove = false;
    }

    IEnumerator ModeSwitcher()
    {
        while (true)
        {
            if (!isVulnerable && !isEaten && canMove)
            {
                // 7 Sekunden Scatter
                currentMode = GhostMode.Scatter;
                yield return new WaitForSeconds(7f);

                // 20 Sekunden Chase
                currentMode = GhostMode.Chase;
                yield return new WaitForSeconds(20f);
            }
            else
            {
                yield return new WaitForSeconds(1f);
            }
        }
    }

    void Update()
    {
        // Warte bis Bewegung erlaubt ist
        if (!canMove && !isEaten) return;

        // Vulnerable Timer
        if (isVulnerable)
        {
            vulnerableTimer -= Time.deltaTime;
            if (vulnerableTimer <= 0)
            {
                isVulnerable = false;
                spriteRenderer.color = normalColor;
                currentMode = GhostMode.Chase;
            }
            else if (vulnerableTimer < 2f)
            {
                // Blinken in den letzten 2 Sekunden
                float blinkSpeed = vulnerableTimer < 1f ? 0.1f : 0.2f;
                spriteRenderer.color = Time.time % blinkSpeed < blinkSpeed / 2f ? vulnerableColor : Color.white;
            }
        }

        // Wenn gefressen, kehre zur Startposition zurück
        if (isEaten)
        {
            ReturnToHome();
            return; // Keine normale Bewegung wenn gefressen
        }

        // Entscheidungs-Timer
        decisionTimer -= Time.deltaTime;
        if (decisionTimer <= 0)
        {
            decisionTimer = decisionInterval;
            ChooseDirection();
        }

        // Bewege den Geist
        Vector2 movement = direction * moveSpeed * Time.deltaTime;
        Vector2 newPosition = (Vector2)transform.position + movement;

        // Prüfe auf Wände
        RaycastHit2D hit = Physics2D.Linecast(transform.position, newPosition, LayerMask.GetMask("Wall"));

        if (hit.collider == null)
        {
            transform.position = newPosition;
        }
        else
        {
            // Wand getroffen, wähle sofort neue Richtung
            ChooseDirection();
        }
    }

    void ReturnToHome()
    {
        // Bewege dich schneller zur Startposition
        float distance = Vector2.Distance(transform.position, homePosition);

        if (distance < 0.5f)
        {
            // Angekommen! Respawn
            transform.position = homePosition;
            Respawn();
        }
        else
        {
            // Bewege direkt zur Startposition (ignoriert Wände)
            Vector2 directionToHome = ((Vector2)homePosition - (Vector2)transform.position).normalized;
            transform.position = Vector2.MoveTowards(transform.position, homePosition, returnSpeed * Time.deltaTime);
        }
    }

    void ChooseDirection()
    {
        Vector2 targetPos = GetTargetPosition();

        // Alle möglichen Richtungen
        Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };

        Vector2 bestDirection = direction;
        float bestScore = float.MaxValue;
        bool foundValidDirection = false;

        foreach (Vector2 dir in directions)
        {
            // Nicht direkt umdrehen (außer im Frightened-Modus)
            if (currentMode != GhostMode.Frightened && dir == -direction)
                continue;

            // Prüfe ob in diese Richtung eine Wand ist
            Vector2 checkPos = (Vector2)transform.position + dir * 0.5f;
            RaycastHit2D hit = Physics2D.Linecast(transform.position, checkPos, LayerMask.GetMask("Wall"));

            if (hit.collider != null)
                continue; // Wand in dieser Richtung

            foundValidDirection = true;

            // Berechne Score für diese Richtung
            float score;

            if (currentMode == GhostMode.Frightened)
            {
                // Im Frightened-Modus: Weg von Pac-Man
                score = -Vector2.Distance(checkPos, targetPos) + Random.Range(-2f, 2f);
            }
            else
            {
                // Normal: Zu Pac-Man hin
                score = Vector2.Distance(checkPos, targetPos);
            }

            if (score < bestScore)
            {
                bestScore = score;
                bestDirection = dir;
            }
        }

        // Wenn keine gültige Richtung gefunden, wähle zufällige
        if (!foundValidDirection)
        {
            direction = GetRandomDirection();
        }
        else
        {
            direction = bestDirection;
        }
    }

    Vector2 GetTargetPosition()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
            else
                return transform.position;
        }

        if (isEaten)
        {
            return homePosition;
        }

        if (currentMode == GhostMode.Scatter)
        {
            return scatterTarget;
        }

        if (currentMode == GhostMode.Frightened)
        {
            // Weg von Pac-Man laufen
            Vector2 awayDirection = (Vector2)transform.position - (Vector2)player.position;
            return (Vector2)transform.position + awayDirection * 10f;
        }

        // Chase-Modus: Verschiedene Strategien
        switch (ghostType)
        {
            case GhostType.Blinky: // Rot - Direkte Verfolgung
                return player.position;

            case GhostType.Pinky: // Pink - 4 Tiles voraus
                PacManController pacman = player.GetComponent<PacManController>();
                if (pacman != null)
                {
                    Vector2 playerDir = pacman.GetCurrentDirection();
                    if (playerDir == Vector2.zero)
                        playerDir = Vector2.right;
                    return (Vector2)player.position + playerDir * 4f;
                }
                return player.position;

            case GhostType.Inky: // Cyan - Arbeitet mit Blinky
                GhostController blinky = FindBlinky();
                if (blinky != null)
                {
                    Vector2 playerPos = player.position;
                    Vector2 blinkyPos = blinky.transform.position;
                    Vector2 offset = playerPos - blinkyPos;
                    return playerPos + offset;
                }
                return player.position;

            case GhostType.Clyde: // Orange - Schüchtern
                float distance = Vector2.Distance(transform.position, player.position);
                if (distance > 8f)
                {
                    return player.position; // Verfolge wenn weit weg
                }
                else
                {
                    return scatterTarget; // Fliehe wenn nah
                }

            default:
                return player.position;
        }
    }

    Vector2 GetRandomDirection()
    {
        Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
        return directions[Random.Range(0, directions.Length)];
    }

    void SetScatterTarget()
    {
        // Ecken des Labyrinths
        switch (ghostType)
        {
            case GhostType.Blinky:
                scatterTarget = new Vector3(12, 14, 0);
                break;
            case GhostType.Pinky:
                scatterTarget = new Vector3(-12, 14, 0);
                break;
            case GhostType.Inky:
                scatterTarget = new Vector3(12, -14, 0);
                break;
            case GhostType.Clyde:
                scatterTarget = new Vector3(-12, -14, 0);
                break;
        }
    }

    GhostController FindBlinky()
    {
        GhostController[] allGhosts = FindObjectsByType<GhostController>(FindObjectsSortMode.None);
        foreach (var ghost in allGhosts)
        {
            if (ghost.ghostType == GhostType.Blinky && ghost != this)
                return ghost;
        }
        return null;
    }

    public void SetVulnerable()
    {
        if (isEaten) return;

        isVulnerable = true;
        vulnerableTimer = vulnerableDuration;
        currentMode = GhostMode.Frightened;
        spriteRenderer.color = vulnerableColor;

        // Drehe um
        direction = -direction;
    }

    public bool IsVulnerable()
    {
        return isVulnerable;
    }

    public void GetEaten()
    {
        isVulnerable = false;
        isEaten = true;
        currentMode = GhostMode.Eaten;
        spriteRenderer.color = eatenColor;

        // Collider deaktivieren während Rückkehr
        GetComponent<Collider2D>().enabled = false;

        Debug.Log($"{ghostType} wurde gefressen und kehrt zurück!");
    }

    void Respawn()
    {
        isEaten = false;
        currentMode = GhostMode.Chase;
        spriteRenderer.color = normalColor;
        direction = GetRandomDirection();

        // Collider wieder aktivieren
        GetComponent<Collider2D>().enabled = true;

        // Erlaube Bewegung wieder
        canMove = true;

        Debug.Log($"{ghostType} ist wieder da!");
    }

    public void EnableMovement()
    {
        canMove = true;
        // Starte Mode-Switcher
        StartCoroutine(ModeSwitcher());
    }

    public void DisableMovement()
    {
        canMove = false;
        StopAllCoroutines();
    }

    IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSeconds(3f);

        transform.position = homePosition;
        Respawn();
    }
}