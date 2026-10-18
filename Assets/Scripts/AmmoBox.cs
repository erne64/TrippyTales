using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

// Gestisce la raccolta di munizioni, l'indicatore fluttuante e l'interazione da parte del giocatore.
public class AmmoBox : MonoBehaviour
{
    [Header("Impostazioni Cassa Munizioni")]
    [SerializeField] private int magToGive = 2;         // Caricatori forniti per raccolta
    [SerializeField] private float radius = 2.5f;       // Raggio di interazione
    [SerializeField] private int maxPlayerMags = 12;    // Numero massimo di caricatori trasportabili

    [Header("Audio")]
    [SerializeField] private AudioClip ammoPickupClip;  // Suono di raccolta
    [SerializeField] private AudioClip fullAmmoClip;    // Suono quando le munizioni sono già al massimo
    [SerializeField] private AudioSource audioSource;   // Sorgente audio

    [Header("Indicatore Visivo e UI")]
    [SerializeField] private GameObject indicator;              // Oggetto grafico fluttuante
    [SerializeField] private TextMeshProUGUI interactionText;   // Testo UI di interazione
    [SerializeField] private float speed = 1.5f;                // Velocità oscillazione indicatore
    [SerializeField] private float amplitude = 0.1f;            // Ampiezza oscillazione indicatore

    private PlayerController player;
    private Vector3 startPosition;
    private bool playerInRange = false;

    private void Start()
    {
        player = PlayerController.instance != null ? PlayerController.instance : FindFirstObjectByType<PlayerController>();

        if (indicator != null)
            startPosition = indicator.transform.position;

        if (interactionText != null)
            interactionText.gameObject.SetActive(false);
    }

    private void Update()
    {
        // Effetto di fluttuazione e rotazione costante dell'indicatore sopra la scatola
        if (indicator != null)
        {
            float newY = startPosition.y + Mathf.Sin(Time.time * speed) * amplitude;
            indicator.transform.position = new Vector3(startPosition.x, newY, startPosition.z);
            indicator.transform.Rotate(Vector3.up, 30f * Time.deltaTime, Space.World);
        }

        // Se il giocatore non è presente (es. è morto), prova a ritrovarlo o nasconde il testo ed esce
        if (player == null)
        {
            player = PlayerController.instance != null ? PlayerController.instance : FindFirstObjectByType<PlayerController>();
            if (player == null)
            {
                if (playerInRange) SetPlayerInRange(false);
                return;
            }
        }

        // Controlla se il giocatore è entro il raggio di interazione
        bool inRange = Vector3.Distance(transform.position, player.transform.position) <= radius;

        // Mostra/nasconde il testo solo quando lo stato cambia: così più scatole
        // possono condividere lo stesso testo UI senza disattivarselo a vicenda
        if (inRange != playerInRange)
        {
            SetPlayerInRange(inRange);
        }

        // Se il giocatore è vicino e preme "E", tenta di dare munizioni
        if (inRange && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            TryGiveAmmo();
        }
    }

    private void OnDisable()
    {
        if (playerInRange)
        {
            SetPlayerInRange(false);
        }
    }

    // Aggiorna lo stato "giocatore vicino" e la visibilità del testo di interazione
    private void SetPlayerInRange(bool inRange)
    {
        playerInRange = inRange;

        if (interactionText != null)
            interactionText.gameObject.SetActive(inRange);
    }

    // Prova a dare munizioni al giocatore, se non ha già il massimo
    private void TryGiveAmmo()
    {
        // Se il giocatore ha già il numero massimo di caricatori, riproduce il suono "pieno"
        if (player.mag >= maxPlayerMags)
        {
            if (audioSource != null && fullAmmoClip != null)
                audioSource.PlayOneShot(fullAmmoClip);

            if (interactionText != null)
                interactionText.gameObject.SetActive(false);
            return;
        }

        // Calcola quante munizioni può ancora ricevere
        int space = maxPlayerMags - player.mag;
        int toGive = Mathf.Min(magToGive, space);

        // Aggiunge le munizioni al giocatore
        player.mag += toGive;

        // Riproduce il suono di raccolta
        if (audioSource != null && ammoPickupClip != null)
            audioSource.PlayOneShot(ammoPickupClip);

        // Se il giocatore ha raggiunto il massimo dei caricatori, nasconde il testo di interazione
        if (player.mag >= maxPlayerMags && interactionText != null)
        {
            interactionText.gameObject.SetActive(false);
        }
    }
}
