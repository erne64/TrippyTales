using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

// Questo script gestisce il menu di pausa e il menu di morte del gioco.
// Permette di mettere in pausa, riprendere, riavviare o uscire dal gioco.
public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuUI;  // UI del menu di pausa
    [SerializeField] private GameObject dieMenuUI;    // UI mostrata quando il giocatore muore
    [SerializeField] private GameObject crosshair;    // Mirino visivo sullo schermo
    [SerializeField] private GameObject damageEffect; // Effetto visivo di danno
    [SerializeField] private CinemachineBrain cinemachineCamera; // Componente Cinemachine per la gestione della telecamera

    public static bool isPaused = false; // Indica se il gioco è attualmente in pausa
    private bool isDead = false;         // Indica se è visualizzato il menu di morte (ESC disabilitato)

    void Start()
    {
        // Recupera il componente CinemachineBrain dalla camera principale se non assegnato
        if (cinemachineCamera == null && Camera.main != null)
        {
            cinemachineCamera = Camera.main.GetComponent<CinemachineBrain>();
        }
    }

    // Mostra il menu di morte quando il giocatore muore
    public void ShowDieMenu()
    {
        isDead = true;                   // Impedisce di uscire dal menu di morte con ESC
        if (damageEffect != null) damageEffect.SetActive(false);   // Disattiva l'effetto di danno
        if (dieMenuUI != null) dieMenuUI.SetActive(true);         // Mostra la UI di morte
        if (crosshair != null) crosshair.SetActive(false);        // Nasconde il mirino

        Cursor.lockState = CursorLockMode.None; // Sblocca il cursore per l'interazione con la UI

        Time.timeScale = 0f; // Ferma il tempo di gioco
        isPaused = true;
    }

    // Mette in pausa il gioco e mostra il menu
    void Pause()
    {
        if (pauseMenuUI != null) pauseMenuUI.SetActive(true); // Mostra il menu di pausa
        if (crosshair != null) crosshair.SetActive(false);    // Nasconde il mirino

        Time.timeScale = 0f;         // Ferma il tempo di gioco
        isPaused = true;

        Cursor.lockState = CursorLockMode.None; // Sblocca il cursore

        // Disattiva temporaneamente la gestione della telecamera
        if (cinemachineCamera != null)
        {
            cinemachineCamera.enabled = false;
        }
    }

    // Riprende il gioco dopo la pausa
    public void Resume()
    {
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false); // Nasconde il menu di pausa
        if (crosshair != null) crosshair.SetActive(true);      // Riattiva il mirino

        Time.timeScale = 1f;          // Riporta il tempo alla normalità

        Cursor.lockState = CursorLockMode.Locked; // Blocca di nuovo il cursore al centro

        isPaused = false;
        
        // Riattiva la gestione della telecamera
        if (cinemachineCamera != null)
        {
            cinemachineCamera.enabled = true;
        }
    }
    
    void Update()
    {
        // Se il giocatore è morto, il tasto ESC non deve riprendere il gioco
        if (isDead) return;

        // Rilevamento tasto ESC compatibile con il nuovo Input System
        bool escapePressed = (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame);

        if (escapePressed)
        {
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    // Torna al menu principale
    public void LoadMenu()
    {
        isPaused = false;
        Time.timeScale = 1f; // Ripristina la velocità del gioco

        // Reimposta lo stato del gioco
        if (LevelSystem.instance != null)
        {
            LevelSystem.instance.ResetGameState();
        }
        
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    // Riavvia il gioco dalla scena iniziale
    public void RestartGame()
    {
        isPaused = false;
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked; // Blocca il cursore per il gameplay

        // Reimposta lo stato del livello
        if (LevelSystem.instance != null)
        {
            LevelSystem.instance.ResetGameState();
        }

        UnityEngine.SceneManagement.SceneManager.LoadScene("Stage1");
    }
    
    // Chiude completamente l'applicazione
    public void QuitGame()
    {
        Application.Quit();
    }
}
