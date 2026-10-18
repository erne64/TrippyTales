using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

// Questo script gestisce la riproduzione di un video introduttivo o di transizione tra le scene.
// Dopo la riproduzione (o se il giocatore preme un tasto), viene caricata la scena principale del gioco.
public class Video : MonoBehaviour
{
    public VideoClip videoToPlay;                // Clip video da riprodurre
    public string gameSceneName = "Stage1";      // Nome della scena di gioco da caricare dopo il video

    private VideoPlayer videoPlayer;             // Componente responsabile della riproduzione video
    private bool isTransitioning = false;        // Evita carichi multipli della scena

    void Start()
    {
        // Ferma la musica in sottofondo durante la riproduzione del video, se attiva
        if (MusicManager.instance != null)
        {
            MusicManager.instance.StopMusic();
        }

        // Recupera il componente VideoPlayer e imposta il video da riprodurre
        videoPlayer = GetComponent<VideoPlayer>();
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoFinished; // Evento chiamato quando il video termina
            if (videoToPlay != null)
                videoPlayer.clip = videoToPlay;

            // Avvia la riproduzione del video
            videoPlayer.Play();
        }
        else
        {
            // Se non c'è VideoPlayer, passa direttamente alla scena
            TransitionToGameScene();
        }
    }

    // Metodo chiamato automaticamente al termine del video
    void OnVideoFinished(VideoPlayer vp)
    {
        TransitionToGameScene();
    }

    void Update()
    {
        // Rileva se un tasto della tastiera o del mouse è stato premuto
        bool anyKeyPressed = (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
            || (Mouse.current != null && (Mouse.current.leftButton.wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame));

        // Permette al giocatore di saltare il video premendo un tasto
        if (!isTransitioning && anyKeyPressed && videoPlayer != null && videoPlayer.time > 0.5f)
        {
            TransitionToGameScene();
        }
    }

    private void TransitionToGameScene()
    {
        if (isTransitioning) return;
        isTransitioning = true;

        if (MusicManager.instance != null)
        {
            MusicManager.instance.ResumeMusic();
        }

        SceneManager.LoadScene(gameSceneName);
    }

    // Rimuove la sottoscrizione all’evento quando lo script viene distrutto
    void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
        }
    }
}
