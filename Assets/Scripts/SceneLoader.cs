using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

// Questo script gestisce la schermata di caricamento delle scene.
// Mostra una barra di progresso, messaggi di caricamento dinamici e un effetto visivo rotante.
// Una volta completato il caricamento, attiva la scena successiva.
public class SceneLoader : MonoBehaviour
{
    public Slider progressBar;           // Riferimento alla barra di caricamento (UI)
    public TMP_Text progressText;        // Testo che mostra la percentuale di caricamento
    public TMP_Text messageText;         // Testo che mostra messaggi di attesa dinamici
    public Image shroom;                 // Immagine animata (es. un fungo che ruota)
    public float extraDelay = 2f;        // Ritardo extra prima di attivare la scena caricata
    public float messageChangeInterval = 1.5f; // Tempo tra un messaggio di caricamento e il successivo
    
    public static string nextScene;      // Nome della prossima scena da caricare

    // Elenco dei messaggi visualizzati durante il caricamento
    private string[] loadingMessages = new string[]
    {
        "Preparando il viaggio…",
        "Accendendo le sinapsi… ",
        "Sbloccando mondi colorati…",
        "Mescolando realtà e sogno…",
        "Respira… lascia che tutto fluisca…",
        "Incontrando i funghi cosmici…",
        "Regolando la vibrazione…"
    };

    void Start()
    {
        // Avvia contemporaneamente il caricamento della scena e il ciclo dei messaggi
        StartCoroutine(LoadSceneAsync());
        StartCoroutine(ChangeLoadingMessages());
    }

    // Coroutine che gestisce il caricamento asincrono della scena successiva
    IEnumerator LoadSceneAsync()
    {
        yield return null;

        if (string.IsNullOrEmpty(nextScene))
        {
            nextScene = "Video";
        }
        
        // Avvia il caricamento della scena specificata
        AsyncOperation operation = SceneManager.LoadSceneAsync(nextScene);
        if (operation == null) yield break;

        operation.allowSceneActivation = false; // Impedisce l’attivazione automatica finché non è pronto
        bool activationTriggered = false;

        while (!operation.isDone)
        {
            // Calcola il progresso (valore normalizzato da 0 a 1)
            float progress = Mathf.Clamp01(operation.progress / 0.9f);

            // Aggiorna la barra di progresso
            if (progressBar != null)
                progressBar.value = progress;

            // Aggiorna il testo con la percentuale
            if (progressText != null)
                progressText.text = Mathf.RoundToInt(progress * 100f) + "%";

            // Ruota l’immagine decorativa 
            if (shroom != null)
                shroom.transform.Rotate(Vector3.forward * 90 * Time.deltaTime);

            // Quando il caricamento è completo, attende il ritardo una sola volta prima di attivare la scena
            if (progress >= 1f && !activationTriggered)
            {
                activationTriggered = true;
                yield return new WaitForSecondsRealtime(extraDelay);
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }

    // Coroutine che cambia periodicamente i messaggi mostrati sullo schermo
    IEnumerator ChangeLoadingMessages()
    {
        int index = 0;

        while (true)
        {
            if (messageText != null && loadingMessages.Length > 0)
            {
                messageText.text = loadingMessages[index];
                index = (index + 1) % loadingMessages.Length;
            }

            yield return new WaitForSecondsRealtime(messageChangeInterval);
        }
    }
}
