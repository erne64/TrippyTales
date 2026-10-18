using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video; 
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

// Questo script gestisce la sequenza introduttiva del gioco composta da uno o più video.
// Riproduce in ordine i video presenti nella lista e, al termine, carica il menu principale.
public class IntroPlayer : MonoBehaviour
{
    public List<VideoClip> videoClips;              // Elenco dei video introduttivi da riprodurre in sequenza
    public string mainMenuSceneName = "MainMenu";   // Nome della scena del menu principale da caricare alla fine

    private VideoPlayer videoPlayer;                // Riferimento al componente VideoPlayer
    private int currentVideoIndex = 0;              // Indice del video attualmente in riproduzione

    private AsyncOperation loadOperation;           // Operazione asincrona per il caricamento anticipato del menu

    void Start()
    {
        // Recupera il componente VideoPlayer presente sull’oggetto
        videoPlayer = GetComponent<VideoPlayer>();

        if (videoPlayer != null)
        {
            // Si iscrive all’evento che viene richiamato quando un video termina
            videoPlayer.loopPointReached += OnVideoFinished;
            
            // Avvia la riproduzione del primo video
            PlayVideo(currentVideoIndex);
        }

        // Inizia a caricare in background la scena del menu principale
        StartCoroutine(StartLoadingMenu());
    }

    // Coroutine che prepara la scena del menu mentre i video introduttivi vengono riprodotti
    IEnumerator StartLoadingMenu()
    {
        // Avvia il caricamento della scena del menu in modo asincrono
        loadOperation = SceneManager.LoadSceneAsync(mainMenuSceneName);
        if (loadOperation == null) yield break;
        
        // Impedisce l’attivazione automatica della scena fino al termine dei video
        loadOperation.allowSceneActivation = false;
        
        // Attende finché la scena non è pronta al 90% 
        while (loadOperation.progress < 0.9f)
        {
            yield return null; 
        }
    }

    // Riproduce il video corrispondente all’indice specificato
    void PlayVideo(int index)
    {
        if (videoPlayer != null && videoClips != null && index < videoClips.Count)
        {
            videoPlayer.clip = videoClips[index];
            videoPlayer.Play();
        }
    }

    // Evento richiamato automaticamente quando il video corrente termina
    void OnVideoFinished(VideoPlayer vp)
    {
        currentVideoIndex++;

        // Se ci sono altri video nella lista, passa al successivo
        if (videoClips != null && currentVideoIndex < videoClips.Count)
        {
            PlayVideo(currentVideoIndex);
        }
        else
        {
            // Altrimenti, attiva la scena del menu principale
            ActivateMenu();
        }
    }

    // Attiva il passaggio alla scena del menu principale
    void ActivateMenu()
    {
        if (loadOperation != null)
        {
            loadOperation.allowSceneActivation = true;
        }
    }

    void Update()
    {
        bool anyKeyPressed = (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
            || (Mouse.current != null && (Mouse.current.leftButton.wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame));

        // Permette al giocatore di saltare il video o passare al successivo premendo un qualsiasi tasto
        if (anyKeyPressed && videoPlayer != null && videoPlayer.time > 0.5f)
        {
            // Se ci sono ancora video da mostrare, passa al prossimo
            if (videoClips != null && currentVideoIndex < videoClips.Count - 1)
            {
                currentVideoIndex++;
                PlayVideo(currentVideoIndex);
            }
            else 
            {
                // Se era l’ultimo, passa direttamente al menu
                ActivateMenu();
            }
        }
    }

    void OnDestroy()
    {
        // Rimuove l’ascoltatore per evitare errori quando l’oggetto viene distrutto
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
        }
    }
}
