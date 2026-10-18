using UnityEngine;
using UnityEngine.SceneManagement;

// Questo script gestisce il portale finale del gioco.
// Quando il giocatore entra nel trigger, viene caricata la scena finale del gioco.
public class FinalPortal : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Controlla se l'oggetto che entra nel trigger è il giocatore
        if (other.CompareTag("Player"))
        {
            // Carica la scena finale
            if (LevelSystem.instance != null && !string.IsNullOrEmpty(LevelSystem.instance.finalSceneName))
            {
                SceneManager.LoadScene(LevelSystem.instance.finalSceneName);
            }
        }
    }
}
