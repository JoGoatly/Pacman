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

        // Lade gespeicherte Lautstärke
        float savedVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        SetVolume(savedVolume);

        if (volumeSlider != null)
        {
            volumeSlider.value = savedVolume;
            volumeSlider.onValueChanged.AddListener(SetVolume);
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
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("MasterVolume", volume);
        PlayerPrefs.Save();

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