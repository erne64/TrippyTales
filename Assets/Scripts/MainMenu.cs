using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

// Questo script gestisce il menu principale del gioco.
// Contiene le funzioni associate ai pulsanti: Gioca, Opzioni ed Esci.
public class MainMenu : MonoBehaviour
{
    private void Start()
    {
        // Sblocca il cursore del mouse 
        Cursor.lockState = CursorLockMode.None;
    }
    
    // Funzione chiamata dal pulsante "Gioca"
    // Avvia una coroutine che carica la scena di caricamento (Loading)
    public void Gioca()
    {
        // Reimposta lo stato dei livelli (necessario se si arriva dalla VictoryScene,
        // perché il LevelSystem persiste tra le scene)
        if (LevelSystem.instance != null)
        {
            LevelSystem.instance.ResetGameState();
        }

        StartCoroutine(LoadLoadingSceneAsync());
    }

    // Funzione chiamata dal pulsante "Opzioni"
    // Carica la scena delle impostazioni del gioco
    public void Opzioni()
    {
        SceneManager.LoadScene("Options");
    }

    // Funzione chiamata dal pulsante "Esci"
    // Mostra un messaggio di debug e chiude l’applicazione
    public void Esci()
    {
        Debug.Log("Uscita dal gioco...");
        Application.Quit(); // Termina il gioco 
    }

    // Coroutine che gestisce il caricamento asincrono della scena di transizione ("Loading")
    IEnumerator LoadLoadingSceneAsync()
    {
        // Imposta il nome della prossima scena da caricare dopo la schermata di caricamento
        SceneLoader.nextScene = "Video";

        // Avvia il caricamento della scena "Loading"
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Loading");
        if (asyncLoad == null) yield break;

        asyncLoad.allowSceneActivation = true; // Consente l’attivazione automatica della scena

        // Attende finché il caricamento non è completato
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }
}

// Alias per compatibilità con eventuali vecchi riferimenti dell'editor di Unity
public class Menu : MainMenu
{
}

