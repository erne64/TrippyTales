using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

// Questo script gestisce il menu delle opzioni del gioco.
// Permette di modificare e salvare le impostazioni di audio, qualità grafica e modalità schermo.
public class OptionsMenu : MonoBehaviour
{
    [Header("Audio")]
    public AudioMixer mixer;          // Mixer audio principale del gioco
    public Slider musicSlider;        // Slider per regolare il volume della musica
    public Slider sfxSlider;          // Slider per regolare il volume degli effetti sonori

    [Header("Video")]
    public TMP_Dropdown qualityDropdown;  // Menu a tendina per selezionare la qualità grafica
    public Toggle fullscreenToggle;       // Toggle per attivare o disattivare la modalità schermo intero

    private void Start()
    {
        // Recupera le impostazioni salvate (o usa i valori predefiniti se non esistono)
        float musicVol = PlayerPrefs.GetFloat("MusicVolume", 1f);
        float sfxVol = PlayerPrefs.GetFloat("SFXVolume", 1f);
        bool isFullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        int qualityIndex = PlayerPrefs.GetInt("Quality", 2);

        // Applica le impostazioni all'avvio
        SetMusicVolume(musicVol);
        SetSFXVolume(sfxVol);
        SetFullscreen(isFullscreen);
        SetQuality(qualityIndex);

        // Aggiorna i valori visivi dei controlli UI
        if (musicSlider != null) musicSlider.value = musicVol;
        if (sfxSlider != null) sfxSlider.value = sfxVol;
        if (fullscreenToggle != null) fullscreenToggle.isOn = isFullscreen;
        if (qualityDropdown != null) qualityDropdown.value = qualityIndex;
    }

    // Imposta e salva il volume della musica
    public void SetMusicVolume(float value)
    {
        if (mixer != null)
            mixer.SetFloat("MusicVolume", ToDecibel(value));       // Conversione in decibel
        PlayerPrefs.SetFloat("MusicVolume", value);             // Salva la preferenza
    }

    // Imposta e salva il volume degli effetti sonori
    public void SetSFXVolume(float value)
    {
        if (mixer != null)
            mixer.SetFloat("SFXVolume", ToDecibel(value));        // Conversione in decibel
        PlayerPrefs.SetFloat("SFXVolume", value);             // Salva la preferenza
    }

    // Converte un valore lineare (0-1) in decibel.
    // Il valore viene limitato a 0.0001 (-80 dB) perché Log10(0) darebbe -Infinito.
    private static float ToDecibel(float value)
    {
        return Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20f;
    }

    // Applica le impostazioni salvate senza bisogno della scena Opzioni.
    // Viene richiamato all'avvio del gioco (dal MusicManager) così volume e qualità
    // vengono rispettati anche se il giocatore non apre il menu delle opzioni.
    public static void ApplySavedSettings(AudioMixer targetMixer)
    {
        if (targetMixer != null)
        {
            targetMixer.SetFloat("MusicVolume", ToDecibel(PlayerPrefs.GetFloat("MusicVolume", 1f)));
            targetMixer.SetFloat("SFXVolume", ToDecibel(PlayerPrefs.GetFloat("SFXVolume", 1f)));
        }

        QualitySettings.SetQualityLevel(PlayerPrefs.GetInt("Quality", 2));
        Screen.fullScreenMode = PlayerPrefs.GetInt("Fullscreen", 1) == 1
            ? FullScreenMode.FullScreenWindow
            : FullScreenMode.Windowed;
    }

    // Imposta la qualità grafica in base al valore selezionato
    public void SetQuality(int index)
    {
        QualitySettings.SetQualityLevel(index);
        PlayerPrefs.SetInt("Quality", index);
    }

    // Attiva o disattiva la modalità schermo intero
    public void SetFullscreen(bool isFullscreen)
    {
        FullScreenMode mode = isFullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        Screen.fullScreenMode = mode;
        
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
    }

    // Applica tutte le impostazioni correnti 
    public void ApplySettings()
    {
        if (musicSlider != null) SetMusicVolume(musicSlider.value);
        if (sfxSlider != null) SetSFXVolume(sfxSlider.value);
        if (qualityDropdown != null) SetQuality(qualityDropdown.value);
        if (fullscreenToggle != null) SetFullscreen(fullscreenToggle.isOn);
    }

    // Torna al menu principale
    public void BackToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
