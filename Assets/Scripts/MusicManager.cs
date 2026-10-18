using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Audio;

// Questo script gestisce la musica di sottofondo del gioco.
// Riproduce una playlist di brani in loop continuo e mantiene la musica attiva tra le scene.
public class MusicManager : MonoBehaviour
{
    public List<AudioClip> playlist;   // Lista di tracce audio da riprodurre come playlist
    private AudioSource audioSource;   // Componente AudioSource responsabile della riproduzione
    private int currentTrackIndex = 0; // Indice della traccia attualmente in riproduzione
    private bool isStopped = false;    // Indica se la musica è stata fermata manualmente (es. durante un video)

    public static MusicManager instance; // Istanza statica per mantenere un solo MusicManager 

    void Awake()
    {
        // Implementazione del pattern Singleton:
        // garantisce che esista un solo MusicManager attivo in tutte le scene
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Mantiene l’oggetto quando si cambia scena
            audioSource = GetComponent<AudioSource>();
        }
        else
        {
            Destroy(gameObject); // Evita duplicati
        }
    }

    void Start()
    {
        // I duplicati (distrutti in Awake) non devono eseguire nulla
        if (instance != this) return;

        // Applica volume, qualità e schermo intero salvati nelle opzioni.
        // Il mixer viene ricavato dal gruppo a cui è collegato l'AudioSource della musica.
        if (audioSource != null && audioSource.outputAudioMixerGroup != null)
        {
            OptionsMenu.ApplySavedSettings(audioSource.outputAudioMixerGroup.audioMixer);
        }
        else
        {
            OptionsMenu.ApplySavedSettings(null);
        }

        // Se la playlist contiene almeno un brano, avvia la riproduzione
        if (playlist.Count > 0)
        {
            PlayTrack(0);
        }
    }

    void Update()
    {
        if (instance != this) return;
        // Se la traccia attuale è terminata, passa automaticamente alla successiva
        // (ma non se la musica è stata fermata volontariamente con StopMusic)
        if (!isStopped && audioSource != null && !audioSource.isPlaying && playlist != null && playlist.Count > 0)
        {
            NextTrack();
        }
    }

    // Riproduce una traccia specifica in base all’indice fornito
    void PlayTrack(int index)
    {
        if (audioSource == null || playlist == null || index < 0 || index >= playlist.Count) return;

        currentTrackIndex = index;
        audioSource.clip = playlist[currentTrackIndex];
        audioSource.Play();
    }

    // Passa alla traccia successiva nella playlist (ciclo continuo)
    void NextTrack()
    {
        if (playlist == null || playlist.Count == 0) return;

        currentTrackIndex = (currentTrackIndex + 1) % playlist.Count;
        PlayTrack(currentTrackIndex);
    }
    
    // Ferma completamente la musica
    public void StopMusic()
    {
        isStopped = true;
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }
    
    // Riprende la musica se è ferma
    public void ResumeMusic()
    {
        isStopped = false;
        if (audioSource != null && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }
}
