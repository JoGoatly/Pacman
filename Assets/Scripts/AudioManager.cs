using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Default Settings")]
    [Range(0f, 1f)]
    public float defaultVolume = 0.5f; // Startet bei 50% beim allerersten Start

    void Awake()
    {
        // Singleton Pattern - bleibt über alle Scenes
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Initialisiere Lautstärke
            InitializeVolume();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void InitializeVolume()
    {
        // Prüfe ob es das erste Mal ist
        if (!PlayerPrefs.HasKey("MasterVolume"))
        {
            // Erstes Mal - setze Default
            PlayerPrefs.SetFloat("MasterVolume", defaultVolume);
            PlayerPrefs.Save();
            Debug.Log($"Erstes Mal! Default-Lautstärke gesetzt auf: {Mathf.RoundToInt(defaultVolume * 100)}%");
        }

        // Lade und setze gespeicherte Lautstärke
        float savedVolume = PlayerPrefs.GetFloat("MasterVolume", defaultVolume);
        AudioListener.volume = savedVolume;

        Debug.Log($"AudioManager initialisiert. Lautstärke: {Mathf.RoundToInt(savedVolume * 100)}%");
    }

    // Methode um Lautstärke zu ändern
    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("MasterVolume", volume);
        PlayerPrefs.Save();

        Debug.Log($"Lautstärke geändert auf: {Mathf.RoundToInt(volume * 100)}%");
    }

    // Methode um aktuelle Lautstärke zu holen
    public float GetVolume()
    {
        return AudioListener.volume;
    }

    // Methode um gespeicherte Lautstärke zu holen
    public float GetSavedVolume()
    {
        return PlayerPrefs.GetFloat("MasterVolume", defaultVolume);
    }
}