using UnityEngine;
using System.Collections;

// Questo script gestisce il comportamento di un oggetto curativo (Health Item).
// Quando il giocatore lo raccoglie, guadagna punti vita e l’oggetto scompare temporaneamente
// per poi riapparire dopo un certo tempo di respawn.
public class HealthItem : MonoBehaviour
{
    [Header("Item Settings")]
    [SerializeField] private int healthToGive = 25;      // Quantità di salute che viene restituita al giocatore (25% della vita max)
    [SerializeField] private float respawnTime = 15f;    // Tempo in secondi prima che l'oggetto ricompaia

    [Header("Audio Settings")]
    [SerializeField] private AudioClip healthPickupClip; // Suono riprodotto quando il giocatore raccoglie l’oggetto
    [SerializeField] private AudioSource audioSource;    // Componente AudioSource per riprodurre il suono

    [Header("Indicator Settings")]
    [SerializeField] private GameObject indicator;       // Oggetto visivo sopra la cura
    [SerializeField] private float speed = 1.5f;         // Velocità dell’oscillazione dell’indicatore
    [SerializeField] private float amplitude = 0.1f;     // Ampiezza del movimento verticale

    private Renderer[] itemRenderers;    // Gestisce la visibilità di tutte le mesh dell’oggetto
    private Collider[] itemColliders;    // Collider per rilevare la raccolta da parte del giocatore
    private bool isCollected = false;    // Indica se l’oggetto è già stato raccolto
    private Vector3 startPosition;       // Posizione iniziale dell’indicatore per il movimento fluttuante

    private void Start()
    {
        // Recupera i riferimenti a tutti i componenti visivi e collider
        itemRenderers = GetComponentsInChildren<Renderer>();
        itemColliders = GetComponentsInChildren<Collider>();

        // Salva la posizione iniziale dell’indicatore
        if (indicator != null)
            startPosition = indicator.transform.position;
    }

    private void Update()
    {
        // Se l’oggetto non è stato raccolto, fa oscillare e ruotare l’indicatore
        if (!isCollected && indicator != null)
        {
            float newY = startPosition.y + Mathf.Sin(Time.time * speed) * amplitude;
            indicator.transform.position = new Vector3(startPosition.x, newY, startPosition.z);

            // Rotazione costante per un effetto visivo più dinamico
            indicator.transform.Rotate(Vector3.up, 30f * Time.deltaTime, Space.World);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Se l’oggetto è già stato raccolto, ignora
        if (isCollected) return;

        // Controlla se chi è entrato nel trigger è il giocatore
        PlayerController player = other.GetComponentInParent<PlayerController>();
    
        if (player != null)
        {
            // Se il giocatore può ricevere salute, applica l’effetto di cura
            if (player.canTakeHealth())
            {
                isCollected = true;

                // Aggiunge la salute al giocatore
                player.AddHealth(healthToGive);

                // Riproduce il suono di raccolta
                if (audioSource != null && healthPickupClip != null)
                    audioSource.PlayOneShot(healthPickupClip);

                // Disattiva temporaneamente la visibilità e le collisioni
                SetItemActiveState(false);

                // Avvia la coroutine per farla riapparire dopo un certo tempo
                StartCoroutine(RespawnAfterDelay());
            }
        }
    }

    // Coroutine che gestisce la riapparizione dell’oggetto dopo un certo intervallo di tempo
    private IEnumerator RespawnAfterDelay()
    {
        // Attende il tempo di respawn
        yield return new WaitForSeconds(respawnTime);

        // Ripristina la posizione iniziale dell'indicatore
        if (indicator != null)
        {
            indicator.transform.position = startPosition;
        }

        // Rende di nuovo visibile e interattivo l’oggetto
        SetItemActiveState(true);

        // L’oggetto può essere raccolto nuovamente
        isCollected = false;
    }

    private void SetItemActiveState(bool active)
    {
        if (indicator != null)
            indicator.SetActive(active);

        if (itemRenderers != null)
        {
            foreach (var r in itemRenderers)
            {
                if (r != null && (indicator == null || !r.transform.IsChildOf(indicator.transform)))
                    r.enabled = active;
            }
        }

        if (itemColliders != null)
        {
            foreach (var c in itemColliders)
            {
                if (c != null) c.enabled = active;
            }
        }
    }
}
