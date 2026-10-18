using UnityEngine;
using UnityEngine.SceneManagement;

// Questo script gestisce il comportamento del portale.
// Quando il giocatore entra nel trigger, viene caricata la scena successiva definita nel LevelSystem.
public class Portal : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Controlla se l'oggetto che entra nel trigger è il giocatore
        if (other.CompareTag("Player"))
        {
            // Carica la scena successiva definita nel LevelSystem
            if (LevelSystem.instance != null && !string.IsNullOrEmpty(LevelSystem.instance.nextSceneName))
            {
                SceneManager.LoadScene(LevelSystem.instance.nextSceneName);
            }
        }
    }
}
