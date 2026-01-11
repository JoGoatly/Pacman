using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject pauseMenuPanel;
    public GameObject settingsButton; // Das Zahnrad
    public Slider volumeSlider;
    public Text volumeText;

    private bool isPaused = false;

    void Start()
    {
        // Verstecke Pause Menu beim Start
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        // Lade AKTUELLE Lautstärke
        float currentVolume = AudioListener.volume;

        Debug.Log($"PauseMenu Start: AudioListener.volume = {currentVolume}");

        if (volumeSlider != null)
        {
            volumeSlider.SetValueWithoutNotify(currentVolume);
            volumeSlider.onValueChanged.AddListener(SetVolume);
            UpdateVolumeText(currentVolume);
        }
    }

    void Update()
    {
        // ESC-Taste zum Öffnen/Schließen
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                Resume();
            else
                Pause();
        }
    }

    public void TogglePause()
    {
        if (isPaused)
            Resume();
        else
            Pause();
    }

    public void Pause()
    {
        isPaused = true;

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(true);

        // WICHTIG: Lade aktuelle Lautstärke vom AudioListener
        float currentVolume = AudioListener.volume;

        Debug.Log($"Pause: AudioListener.volume = {currentVolume}");

        if (volumeSlider != null)
        {
            volumeSlider.SetValueWithoutNotify(currentVolume); // Wichtig: SetValueWithoutNotify!
        }
        UpdateVolumeText(currentVolume);

        // Pausiere das Spiel
        Time.timeScale = 0f;

        Debug.Log($"Spiel pausiert. Lautstärke: {Mathf.RoundToInt(currentVolume * 100)}%");
    }

    public void Resume()
    {
        isPaused = false;

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        // Setze Spiel fort
        Time.timeScale = 1f;

        Debug.Log("Spiel fortgesetzt");
    }

    public void SetVolume(float volume)
    {
        // Nutze AudioManager falls vorhanden
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetVolume(volume);
        }
        else
        {
            AudioListener.volume = volume;
            PlayerPrefs.SetFloat("MasterVolume", volume);
            PlayerPrefs.Save();
        }

        UpdateVolumeText(volume);
    }

    void UpdateVolumeText(float volume)
    {
        if (volumeText != null)
        {
            volumeText.text = Mathf.RoundToInt(volume * 100) + "%";
        }
    }

    public void ReturnToMainMenu()
    {
        // Setze Time Scale zurück
        Time.timeScale = 1f;

        // Lade Main Menu
        SceneManager.LoadScene("MainMenu");

        Debug.Log("Zurück zum Hauptmenü");
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}