using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

// Questo script gestisce l'intero sistema dei livelli del gioco.
// Tiene traccia del numero di nemici uccisi, del livello corrente, del progresso tra i livelli
// e attiva portali quando il giocatore avanza o completa la missione.
public class LevelSystem : MonoBehaviour
{
    public static LevelSystem instance;  // Istanza statica per accesso globale al sistema dei livelli

    public SpawnPoint spawnPoint1;       // Primo punto di spawn per i nemici
    public SpawnPoint spawnPoint2;       // Secondo punto di spawn per i nemici

    [Header("Level Settings")]
    public int enemiesToNextLevel = 8;   // Numero di nemici da eliminare per passare al livello successivo
    private int currentLevel = 1;        // Numero del livello corrente
    private int enemiesKilled = 0;       // Numero di nemici eliminati nel livello attuale

    [Header("Enemy Settings")]
    public float zombieSpeedIncrease = 0.2f; // Aumento bilanciato della velocità dei nemici a ogni livello (+0.2 per livello)
    public float baseZombieSpeed = 2.8f;     // Velocità base iniziale dei nemici (il giocatore ha velocità 5.0)

    [Header("Portal Settings")]
    public int portalActivationLevel = 5;    // Livello in cui si attiva il portale per il cambio scena
    public string nextSceneName = "PortalVideo"; // Nome della scena da caricare dopo il portale
    private GameObject portalObject;             // Riferimento all’oggetto portale nella scena
    private TextMeshProUGUI portalMessageText;   // Testo UI che mostra il messaggio del portale

    [Header("Victory Settings")]
    public int maxLevel = 15;                   // Livello massimo prima della vittoria finale
    public string finalSceneName = "VictoryScene"; // Scena finale di vittoria
    private GameObject finalPortalObject;           // Oggetto del portale finale
    private TextMeshProUGUI finalPortalMessageText; // Testo UI per il messaggio del portale finale

    private TextMeshProUGUI stageMessageText;   // Testo UI per mostrare il nome dell’area/stage
    private TextMeshProUGUI bonusMessageText;   // Testo UI per i messaggi bonus (es. potenziamenti)

    // Restituisce il nome della località in base alla scena corrente
    private string GetCountryName(string sceneName)
    {
        switch (sceneName)
        {
            case "Stage1":
                return "QUATTROMIGLIA";
            case "Stage2":
                return "PIANE CRATI";
            case "Stage3":
                return "NICOTERA";
            default:
                return "Area Sconosciuta";
        }
    }

    // Restituisce il livello attuale
    public int GetCurrentLevel()
    {
        return currentLevel;
    }

    private void Awake()
    {
        // Implementazione del pattern Singleton: assicura un’unica istanza del LevelSystem
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Mantiene l’oggetto anche nei cambi scena
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Metodo chiamato ogni volta che un nemico viene ucciso
    public void EnemyKilled()
    {
        enemiesKilled++;
        
        // Se il numero di nemici uccisi raggiunge la soglia, si passa al livello successivo
        if (enemiesKilled >= enemiesToNextLevel)
        {
            LevelUp();
        }
    }

    // Gestisce il passaggio al livello successivo
    private void LevelUp()
    {
        int previousLevel = currentLevel;

        // Aggiorna parametri di livello
        currentLevel++;
        enemiesKilled = 0;
        enemiesToNextLevel += 2; // Aumenta la difficoltà progressivamente

        // Incrementa la velocità base dei nemici
        baseZombieSpeed += zombieSpeedIncrease;

        // Aggiorna la velocità di tutti i nemici già presenti
        ZombieAI[] zombies = FindObjectsOfType<ZombieAI>();
        foreach (var z in zombies)
        {
            z.UpdateSpeed(baseZombieSpeed);
        }

        // Aggiorna la UI del livello se non si è ancora raggiunto il portale o la fine
        if (previousLevel != maxLevel && previousLevel != portalActivationLevel)
        {
            if (LevelCount.occurence != null)
            {
                LevelCount.occurence.UpdateLevel(currentLevel); 
            }
        }

        // Controlla le condizioni speciali 
        if (previousLevel == maxLevel)
        {
            HandleVictory();
            return;
        }

        if (previousLevel == portalActivationLevel)
        {
            ActivatePortal();
        }
        else
        {
            // Fa ripartire lo spawn dei nemici nel nuovo livello
            StartSpawnPoints();
        }
    }

    private void OnEnable()
    {
        // Si iscrive all’evento di caricamento scena
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // Rimuove la sottoscrizione per evitare errori
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Viene chiamato ogni volta che una nuova scena viene caricata
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string sceneName = scene.name;

        // Gestisce la configurazione delle scene di gioco principali
        if (sceneName == "Stage1" || sceneName == "Stage2" || sceneName == "Stage3") 
        {
            SetupSceneReferences();

            // Mostra il nome dell’area corrente
            if (stageMessageText != null)
            {
                ShowStageName(sceneName);
            }
            
            // Mostra un messaggio bonus in alcune scene
            if (bonusMessageText != null && (sceneName == "Stage2" || sceneName == "Stage3"))
            {
                StartCoroutine(ShowBonusMessage());
            }
            
            // Imposta il portale speciale per lo Stage2
            if (sceneName == "Stage2")
            {
                portalActivationLevel = 10; 
                nextSceneName = "PortalVideo2";
            }
            
            // Avvia lo spawn dei nemici
            StartSpawnPoints();
        }
    }

    // Recupera e collega tutti i riferimenti della scena attuale
    private void SetupSceneReferences()
    {
        spawnPoint1 = null;
        spawnPoint2 = null;

        stageMessageText = null;
        bonusMessageText = null;

        portalObject = null;
        portalMessageText = null;

        finalPortalObject = null;
        finalPortalMessageText = null;

        // Trova i punti di spawn nella scena
        SpawnPoint[] spawns = FindObjectsOfType<SpawnPoint>();
        if (spawns.Length >= 2)
        {
            spawnPoint1 = spawns[0];
            spawnPoint2 = spawns[1];
        }
        else if (spawns.Length == 1)
        {
            spawnPoint1 = spawns[0];
            spawnPoint2 = null;
        }

        // Trova gli oggetti UI tramite i loro tag in modo protetto
        GameObject stageMessageObj = SafeFindTag("StageMessage");
        if (stageMessageObj != null)
            stageMessageText = stageMessageObj.GetComponent<TextMeshProUGUI>();

        GameObject boostObj = SafeFindTag("BoostMessage");
        if (boostObj != null)
            bonusMessageText = boostObj.GetComponent<TextMeshProUGUI>();

        GameObject newPortal = SafeFindTag("Portal");
        if (newPortal != null)
            portalObject = newPortal;

        GameObject messageObj = SafeFindTag("PortalMessage");
        if (messageObj != null)
            portalMessageText = messageObj.GetComponent<TextMeshProUGUI>();

        // Disattiva gli elementi finché non servono
        if (stageMessageText != null) stageMessageText.gameObject.SetActive(false);
        if (bonusMessageText != null) bonusMessageText.gameObject.SetActive(false); 
        if (portalObject != null) portalObject.SetActive(false);
        if (portalMessageText != null) portalMessageText.gameObject.SetActive(false);

        // Configura i riferimenti per la scena finale (Stage3)
        if (SceneManager.GetActiveScene().name == "Stage3")
        {
            GameObject finalPortalObj = SafeFindTag("FinalPortal");
            if (finalPortalObj != null)
                finalPortalObject = finalPortalObj;

            GameObject finalMessageObj = SafeFindTag("FinalPortalMessage");
            if (finalMessageObj != null)
                finalPortalMessageText = finalMessageObj.GetComponent<TextMeshProUGUI>();

            if (finalPortalObject != null) finalPortalObject.SetActive(false);
            if (finalPortalMessageText != null) finalPortalMessageText.gameObject.SetActive(false);
        }
    }

    // Ricerca sicura tramite Tag che evita UnityException se un tag non è definito nel TagManager
    private GameObject SafeFindTag(string tagName)
    {
        try
        {
            return GameObject.FindGameObjectWithTag(tagName);
        }
        catch
        {
            return null;
        }
    }

    // Gestisce l'avvio dello spawn che ci siano 1 o 2 punti di spawn configurati
    private void StartSpawnPoints()
    {
        if (spawnPoint1 != null && spawnPoint2 != null)
        {
            int half = Mathf.Max(1, enemiesToNextLevel / 2);
            spawnPoint1.StartSpawning(half);
            spawnPoint2.StartSpawning(enemiesToNextLevel - half);
        }
        else if (spawnPoint1 != null)
        {
            spawnPoint1.StartSpawning(enemiesToNextLevel);
        }
        else if (spawnPoint2 != null)
        {
            spawnPoint2.StartSpawning(enemiesToNextLevel);
        }
    }

    // Mostra a schermo il nome dell’area corrente 
    private void ShowStageName(string sceneName)
    {
        if (stageMessageText == null) return;

        string countryName = GetCountryName(sceneName);
        stageMessageText.text = countryName;
        stageMessageText.gameObject.SetActive(true);
    }
    
    // Mostra un messaggio bonus temporaneo
    IEnumerator ShowBonusMessage()
    {
        if (bonusMessageText == null) yield break;

        bonusMessageText.gameObject.SetActive(true);
        yield return new WaitForSeconds(5f);
        bonusMessageText.gameObject.SetActive(false);
    }
    
    // Attiva il portale per il cambio scena e ferma lo spawn dei nemici
    private void ActivatePortal()
    {
        if (portalObject != null)
        {
            portalObject.SetActive(true);
        }

        if (portalMessageText != null)
        {
            portalMessageText.gameObject.SetActive(true);
            portalMessageText.text = "Ottimo lavoro, sei sopravvissuto!\nTrova il portale per andare avanti.";
        }

        if (spawnPoint1 != null) spawnPoint1.StopSpawning();
        if (spawnPoint2 != null) spawnPoint2.StopSpawning();
    }

    // Gestisce la fase finale del gioco (vittoria)
    private void HandleVictory()
    {  
        if (spawnPoint1 != null) spawnPoint1.StopSpawning();
        if (spawnPoint2 != null) spawnPoint2.StopSpawning();

        if (finalPortalObject != null)
        {
            finalPortalObject.SetActive(true);
        }

        if (finalPortalMessageText != null)
        {
            finalPortalMessageText.gameObject.SetActive(true);
            finalPortalMessageText.text = "MISSIONE COMPIUTA!\nTrova il portale per tornare a casa!";
        }
    }

    // Reimposta tutti i parametri del sistema di livelli per una nuova partita
    public void ResetGameState()
    {
        currentLevel = 1;
        enemiesKilled = 0;
        enemiesToNextLevel = 8;
        baseZombieSpeed = 2.8f;

        portalActivationLevel = 5;
        nextSceneName = "PortalVideo"; // Stesso valore configurato nella scena Stage1
    }
}
