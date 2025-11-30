using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [HideInInspector] public int score = 0; // Score bleibt über Szenen erhalten
    public int lives = 3;
    public Text scoreText;
    public Text livesText;
    public Text messageText; // Für "Ready!", "Game Over", "You Win!"
    public GameObject[] lifeIcons;

    [Header("Ghost Release Settings")]
    public float blinkyReleaseDelay = 0f;     // Rot - sofort
    public float pinkyReleaseDelay = 3f;      // Pink - nach 3 Sek
    public float inkyReleaseDelay = 6f;       // Cyan - nach 6 Sek
    public float clydeReleaseDelay = 9f;      // Orange - nach 9 Sek

    [Header("Game Start")]
    public float introDelay = 3f; // Zeit bevor Spiel startet
    public AudioClip introSound;

    private GameObject player;
    private GhostController[] ghosts;
    private Vector3 playerStartPosition;
    private AudioSource audioSource;
    private int totalPellets = 0;
    private int collectedPellets = 0;
    private static int persistentScore = 0; // Statischer Score über Szenen

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        // Lade persistenten Score
        score = persistentScore;
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        playerStartPosition = player.transform.position;
        ghosts = FindObjectsByType<GhostController>(FindObjectsSortMode.None);

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Zähle alle Pellets
        CountPellets();

        UpdateUI();

        // Starte Intro-Sequenz
        StartCoroutine(IntroSequence());
    }

    void CountPellets()
    {
        GameObject[] pellets = GameObject.FindGameObjectsWithTag("Pellet");
        GameObject[] powerPellets = GameObject.FindGameObjectsWithTag("PowerPellet");
        totalPellets = pellets.Length + powerPellets.Length;
        collectedPellets = 0;

        Debug.Log($"Total Pellets: {totalPellets}");
    }

    IEnumerator IntroSequence()
    {
        // Zeige "READY!" Text
        if (messageText != null)
        {
            messageText.gameObject.SetActive(true);
            messageText.text = "READY!";
            messageText.color = Color.yellow;
        }

        // Spiele Intro Sound
        if (introSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(introSound);
        }

        // Warte Intro-Dauer
        yield return new WaitForSeconds(introDelay);

        // Verstecke Text
        if (messageText != null)
        {
            messageText.gameObject.SetActive(false);
        }

        // Starte Spiel
        StartGame();
    }

    void StartGame()
    {
        // Erlaube Pac-Man Bewegung
        PacManController pacman = player.GetComponent<PacManController>();
        if (pacman != null)
        {
            pacman.EnableMovement();
        }

        // Release Geister gestaffelt
        StartCoroutine(ReleaseGhosts());
    }

    IEnumerator ReleaseGhosts()
    {
        // Erstelle eine sortierte Liste für die richtige Reihenfolge
        System.Collections.Generic.List<GhostController> sortedGhosts = new System.Collections.Generic.List<GhostController>();

        // Füge Geister in der richtigen Reihenfolge hinzu: Rot, Pink, Cyan, Orange
        GhostController blinky = null, pinky = null, inky = null, clyde = null;

        foreach (GhostController ghost in ghosts)
        {
            switch (ghost.ghostType)
            {
                case GhostType.Blinky: blinky = ghost; break;
                case GhostType.Pinky: pinky = ghost; break;
                case GhostType.Inky: inky = ghost; break;
                case GhostType.Clyde: clyde = ghost; break;
            }
        }

        // Richtige Reihenfolge
        if (blinky != null) sortedGhosts.Add(blinky);
        if (pinky != null) sortedGhosts.Add(pinky);
        if (inky != null) sortedGhosts.Add(inky);
        if (clyde != null) sortedGhosts.Add(clyde);

        // Release in der richtigen Reihenfolge mit den konfigurierten Delays
        for (int i = 0; i < sortedGhosts.Count; i++)
        {
            GhostController ghost = sortedGhosts[i];
            float delay = 0f;

            switch (ghost.ghostType)
            {
                case GhostType.Blinky:
                    delay = blinkyReleaseDelay;
                    break;
                case GhostType.Pinky:
                    delay = pinkyReleaseDelay;
                    break;
                case GhostType.Inky:
                    delay = inkyReleaseDelay;
                    break;
                case GhostType.Clyde:
                    delay = clydeReleaseDelay;
                    break;
            }

            // Setze das Delay auch im Geist
            ghost.ghostReleaseDelay = delay;

            // Warte das Delay ab (nur für Geister nach dem ersten)
            if (i == 0)
            {
                // Erster Geist (Blinky) - warte das konfigurierte Delay
                if (delay > 0)
                {
                    yield return new WaitForSeconds(delay);
                }
            }
            else
            {
                // Alle anderen: Warte den Unterschied zum vorherigen
                float previousDelay = 0f;
                switch (sortedGhosts[i - 1].ghostType)
                {
                    case GhostType.Blinky: previousDelay = blinkyReleaseDelay; break;
                    case GhostType.Pinky: previousDelay = pinkyReleaseDelay; break;
                    case GhostType.Inky: previousDelay = inkyReleaseDelay; break;
                    case GhostType.Clyde: previousDelay = clydeReleaseDelay; break;
                }

                float waitTime = delay - previousDelay;
                if (waitTime > 0)
                {
                    yield return new WaitForSeconds(waitTime);
                }
            }

            ghost.EnableMovement();
            Debug.Log($"{ghost.ghostType} released after {delay} seconds!");
        }
    }

    public void AddScore(int points)
    {
        score += points;
        persistentScore = score; // Speichere Score persistent

        // Prüfe ob Pellet eingesammelt wurde
        if (points == 10 || points == 50)
        {
            collectedPellets++;

            // Prüfe Win-Condition
            if (collectedPellets >= totalPellets)
            {
                Win();
            }
        }

        UpdateUI();

        // Extra Leben bei 10000 Punkten
        if (score >= 10000 && lives < 5)
        {
            lives++;
            UpdateUI();
        }
    }

    public void ActivatePowerMode()
    {
        foreach (var ghost in ghosts)
        {
            ghost.SetVulnerable();
        }
    }

    public void PlayerDied()
    {
        lives--;
        UpdateUI();

        // Stoppe alle Geister
        foreach (var ghost in ghosts)
        {
            ghost.DisableMovement();
        }

        if (lives <= 0)
        {
            GameOver();
        }
        else
        {
            // Warte kurz, dann respawn
            StartCoroutine(RespawnSequence());
        }
    }

    IEnumerator RespawnSequence()
    {
        yield return new WaitForSeconds(2f);

        // Teleportiere Pac-Man zurück
        if (player != null)
        {
            player.transform.position = playerStartPosition;
            PacManController pacman = player.GetComponent<PacManController>();
            if (pacman != null)
            {
                pacman.ResetPlayer();
                pacman.EnableMovement();
            }
        }

        // Release Geister wieder gestaffelt
        StartCoroutine(ReleaseGhosts());
    }

    void Win()
    {
        Debug.Log("YOU WIN!");

        // Stoppe Pac-Man
        PacManController pacman = player.GetComponent<PacManController>();
        if (pacman != null)
        {
            pacman.ResetPlayer(); // Stoppt Bewegung (canMove = false)
        }

        // Stoppe alle Geister
        foreach (var ghost in ghosts)
        {
            ghost.DisableMovement();
        }

        // Zeige Win-Text
        if (messageText != null)
        {
            messageText.gameObject.SetActive(true);
            messageText.text = "YOU WIN!";
            messageText.color = Color.green;
        }

        // Nach 5 Sekunden: Neues Level (Score bleibt)
        Invoke("NextLevel", 5f);
    }

    void NextLevel()
    {
        // Score bleibt erhalten (persistentScore ist bereits gesetzt)
        // Lade Scene neu
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void GameOver()
    {
        Debug.Log("Game Over! Final Score: " + score);

        // Zeige Game Over Text
        if (messageText != null)
        {
            messageText.gameObject.SetActive(true);
            messageText.text = "GAME OVER";
            messageText.color = Color.red;
        }

        Invoke("RestartGame", 3f);
    }

    void RestartGame()
    {
        // NUR bei Game Over: Score zurücksetzen
        score = 0;
        persistentScore = 0;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score;

        if (livesText != null)
            livesText.text = "Lives: " + lives;

        // Update life icons
        for (int i = 0; i < lifeIcons.Length; i++)
        {
            if (lifeIcons[i] != null)
                lifeIcons[i].SetActive(i < lives);
        }
    }
}