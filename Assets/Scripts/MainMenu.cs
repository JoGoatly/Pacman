using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Menu Panels")]
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;

    [Header("Settings UI")]
    public Slider volumeSlider;
    public Text volumeText;

    [Header("Audio")]
    public AudioClip menuMusicClip;
    private AudioSource audioSource;

    private void Start()
    {
        // Zeige Main Menu, verstecke Settings
        ShowMainMenu();

        // AudioSource Setup
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.loop = true;
        audioSource.playOnAwake = false;

        // WARTE kurz damit AudioManager Zeit hat zu initialisieren
        Invoke("InitializeAudio", 0.1f);
    }

    void InitializeAudio()
    {
        // Hole aktuelle Lautstärke vom AudioListener
        float currentVolume = AudioListener.volume;

        Debug.Log($"MainMenu Start: AudioListener.volume = {currentVolume}");

        if (volumeSlider != null)
        {
            volumeSlider.SetValueWithoutNotify(currentVolume);
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }

        if (volumeText != null)
        {
            volumeText.text = Mathf.RoundToInt(currentVolume * 100) + "%";
        }

        // Spiele Menu-Musik
        if (menuMusicClip != null)
        {
            audioSource.clip = menuMusicClip;
            audioSource.Play();
        }
    }

    // MAIN MENU BUTTONS
    public void PlayGame()
    {
        Debug.Log("Starte Spiel...");
        // Lade die Game-Scene (Scene muss "Game" heißen oder passe den Namen an)
        SceneManager.LoadScene("Game");
    }

    public void OpenSettings()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);

        // WICHTIG: Lade aktuelle Lautstärke vom AudioListener
        float currentVolume = AudioListener.volume;

        Debug.Log($"OpenSettings: AudioListener.volume = {currentVolume}");

        if (volumeSlider != null)
        {
            volumeSlider.SetValueWithoutNotify(currentVolume); // Wichtig: SetValueWithoutNotify!
        }
        if (volumeText != null)
        {
            volumeText.text = Mathf.RoundToInt(currentVolume * 100) + "%";
        }
    }

    public void QuitGame()
    {
        Debug.Log("Spiel wird beendet...");
        Application.Quit();

        // Für Unity Editor
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // SETTINGS BUTTONS
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

        if (volumeText != null)
        {
            volumeText.text = Mathf.RoundToInt(volume * 100) + "%";
        }

        Debug.Log($"Lautstärke auf {Mathf.RoundToInt(volume * 100)}% gesetzt");
    }

    public void BackToMainMenu()
    {
        settingsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    void ShowMainMenu()
    {
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }
}