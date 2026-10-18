using UnityEngine;

// Questo script gestisce la generazione dei nemici in un punto specifico della mappa.
// È utilizzato dal LevelSystem per creare ondate di zombie durante le varie fasi di gioco.
public class SpawnPoint : MonoBehaviour
{
    [Header("Spawn Point Settings")]
    public GameObject ZombiePrefab1;           // Primo tipo di zombie che può essere generato
    public GameObject ZombiePrefab2;           // Secondo tipo di zombie
    public Transform SpawnPointTransform;      // Posizione effettiva dove verranno generati i nemici
    public float SpawnInterval = 3f;           // Intervallo di tempo tra uno spawn e l’altro

    private int maxZombies = 0;                // Numero massimo di zombie da generare per questa ondata
    private int currentZombieCount = 0;        // Numero di zombie attualmente generati

    // Avvia la generazione degli zombie per l’ondata corrente
    public void StartSpawning(int numberOfZombies)
    {
        maxZombies = numberOfZombies;
        currentZombieCount = 0;

        // Annulla eventuali invocazioni precedenti per evitare spawn multipli sovrapposti
        CancelInvoke(nameof(SpawnZombie));

        // Chiama ripetutamente il metodo SpawnZombie a intervalli regolari
        InvokeRepeating(nameof(SpawnZombie), 1f, SpawnInterval);
    }

    // Metodo che genera effettivamente gli zombie
    private void SpawnZombie()
    {
        if (currentZombieCount < maxZombies)
        {
            // Sceglie casualmente quale tipo di zombie spawnare con fallback
            GameObject prefabToSpawn = (Random.value > 0.5f) ? ZombiePrefab2 : ZombiePrefab1;
            if (prefabToSpawn == null)
            {
                prefabToSpawn = ZombiePrefab1 != null ? ZombiePrefab1 : ZombiePrefab2;
                if (prefabToSpawn == null) return;
            }

            // Posizione e rotazione di spawn sicure (fallback su transform dell'oggetto)
            Vector3 spawnPos = SpawnPointTransform != null ? SpawnPointTransform.position : transform.position;
            Quaternion spawnRot = SpawnPointTransform != null ? SpawnPointTransform.rotation : transform.rotation;

            // Crea lo zombie nella posizione del punto di spawn
            GameObject zombie = Instantiate(prefabToSpawn, spawnPos, spawnRot);
            currentZombieCount++;

            // Aggiorna la velocità dello zombie in base al livello corrente
            ZombieAI zombieAI = zombie.GetComponent<ZombieAI>();
            if (zombieAI != null && LevelSystem.instance != null)
            {
                zombieAI.UpdateSpeed(LevelSystem.instance.baseZombieSpeed);
            }
        }
        else
        {
            // Se il numero massimo è stato raggiunto, interrompe la generazione
            CancelInvoke(nameof(SpawnZombie));
        }
    }

    // Ferma manualmente la generazione di zombie
    public void StopSpawning()
    {
        CancelInvoke(nameof(SpawnZombie));
    }

    private void OnDisable()
    {
        StopSpawning();
    }
}
