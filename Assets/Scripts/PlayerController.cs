using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

// Questo script gestisce tutte le funzioni principali del giocatore:
// movimento, salto, sparatoria, ricarica, danni, salute, interfaccia e interazioni.
[RequireComponent(typeof(CharacterController), typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float playerSpeed = 5.0f;   // Velocità di movimento del giocatore
    [SerializeField] private float jumpHeight = 1.5f;    // Altezza massima del salto
    [SerializeField] private float gravityValue = -9.81f;    // Forza di gravità applicata al giocatore
    [SerializeField] private float rotationSpeed = 1f;   // Velocità di rotazione del giocatore

    private CharacterController controller;  // Gestisce il movimento fisico del giocatore
    private PlayerInput playerInput;         // Riferimento al sistema di input 
    private Vector3 playerVelocity;          // Vettore per la gestione della velocità
    private bool groundedPlayer;             // Indica se il giocatore è a terra
    private Transform cameraTransform;       // Riferimento alla camera principale per orientare il movimento

    // Azioni di input del giocatore
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction shootAction;
    private InputAction reloadAction;

    public Animator animator;               // Controlla le animazioni del personaggio
    public PauseMenu pauseMenu;             // Riferimento al menu di pausa

    [Header("Rifle Effects")]
    public ParticleSystem muzzleSpark;      // Effetto visivo per lo sparo
    public GameObject impactEffect;         // Effetto visivo del proiettile sul punto d’impatto

    [Header("Audio Settings")]
    public AudioSource gunAudioSource;      // Sorgente audio per i suoni delle armi
    public AudioClip gunShotClip;           // Clip audio dello sparo
    public AudioClip reloadClip;            // Clip audio della ricarica
    public AudioClip emptyClip;             // Clip audio per quando le munizioni sono esaurite

    [Header("Rifle Stats")]
    public int damage = 25;                 // Danno inflitto ai nemici (4 colpi per eliminare uno zombie da 100 HP)
    public float fireCharge = 10f;          // Frequenza di fuoco (10 colpi al secondo, ~600 RPM)
    private float nextTimeToFire = 0f;      // Controlla la cadenza di fuoco
    public int mag = 10;                    // Numero di caricatori totali
    private int maxAmmo = 32;               // Numero massimo di colpi per caricatore
    private int currentAmmo;                // Numero di colpi attuali
    public float reloadTime = 1.6f;         // Durata della ricarica
    private bool isReloading = false;       // Indica se il giocatore sta ricaricando

    [Header("Player Health")]
    public float maxHealth = 100f;          // Punti vita massimi
    private float currentHealth;            // Punti vita attuali
    public HealthBarScript healthBar;       // Riferimento alla barra della salute UI
    public GameObject damageEffect;         // Effetto visivo quando il giocatore viene colpito

    [Header("UI Messages")]
    public TextMeshProUGUI ammoOutText;             // Testo mostrato quando le munizioni sono finite
    public TextMeshProUGUI interactionAmmoBoxMessage; // Messaggio mostrato quando si interagisce con le munizioni
    public float messageDuration = 2f;              // Durata di visualizzazione dei messaggi UI

    public static PlayerController instance;        // Istanza statica del giocatore 
    private int lastDisplayedAmmo = -1;             // Cache per evitare allocazioni stringa a ogni frame
    private int lastDisplayedMag = -1;              // Cache per evitare allocazioni stringa a ogni frame

    private void Start()
    {
        currentAmmo = maxAmmo;
        currentHealth = maxHealth;

        // Blocca il cursore per la modalità di gioco in prima persona
        Cursor.lockState = CursorLockMode.Locked;

        // Imposta la barra della salute al valore massimo
        if (healthBar != null)
            healthBar.SetMaxHealth(maxHealth);
    }

    private void Awake()
    {
        instance = this;

        // Recupera i componenti principali
        controller = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();

        if (Camera.main != null)
            cameraTransform = Camera.main.transform;

        if (pauseMenu == null)
            pauseMenu = FindFirstObjectByType<PauseMenu>();

        // Assegna le azioni definite nell'Input System in modo sicuro
        if (playerInput != null && playerInput.actions != null)
        {
            moveAction = playerInput.actions["Move"];
            jumpAction = playerInput.actions["Jump"];
            shootAction = playerInput.actions["Shoot"];
            reloadAction = playerInput.actions["reload"];
        }
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    void Update()
    {
        // Evita errori se la telecamera principale non è ancora assegnata
        if (cameraTransform == null)
        {
            if (Camera.main != null)
                cameraTransform = Camera.main.transform;
            else
                return;
        }

        // Blocca tutti i controlli quando il gioco è in pausa
        if (PauseMenu.isPaused) return;

        // --- SISTEMA DI RICARICA ---
        if (reloadAction != null)
        {
            if ((currentAmmo <= 0 && mag > 0) || (reloadAction.triggered && mag > 0))
            {
                if (currentAmmo != maxAmmo)
                {
                    if (!isReloading)
                    {
                        StartCoroutine(Reload());
                        if (animator != null) animator.SetTrigger("reload");
                        if (gunAudioSource != null && reloadClip != null)
                            gunAudioSource.PlayOneShot(reloadClip);
                    }
                }
            }
        }

        // --- GESTIONE MOVIMENTO ---
        groundedPlayer = controller.isGrounded;
        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }

        Vector2 input = moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;
        
        // Proietta i vettori della camera sul piano orizzontale per evitare rallentamenti guardando in alto/basso
        Vector3 camForward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
        Vector3 camRight = Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized;
        Vector3 move = (camForward * input.y + camRight * input.x);
        if (move.sqrMagnitude > 1f) move.Normalize();

        float currentSpeed = playerSpeed;
        if (input.y < -0.1f)
            currentSpeed *= 0.6f; // Rallenta quando si cammina all’indietro

        controller.Move(move * currentSpeed * Time.deltaTime);

        float speed = move.magnitude;
        if (animator != null)
        {
            animator.SetFloat("run", speed);
            animator.SetBool("BackRun", input.y < -0.1f);
            animator.SetBool("idle", speed < 0.05f);
        }

        // --- SALTO ---
        if (jumpAction != null && jumpAction.triggered && groundedPlayer)
        {
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
            if (animator != null) animator.SetTrigger("jump");
        }

        // --- GRAVITÀ ---
        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);

        // --- ROTAZIONE CAMERA ---
        Quaternion targetRotation = Quaternion.Euler(0, cameraTransform.eulerAngles.y, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * 10f * Time.deltaTime);

        // --- UI AGGIORNATA (solo se i valori sono cambiati, per eliminare GC allocation e rebuild UI) ---
        if (AmmoCount.occurence != null)
        {
            if (currentAmmo != lastDisplayedAmmo)
            {
                AmmoCount.occurence.UpdateAmmo(currentAmmo);
                lastDisplayedAmmo = currentAmmo;
            }
            if (mag != lastDisplayedMag)
            {
                AmmoCount.occurence.UpdateMag(mag);
                lastDisplayedMag = mag;
            }
        }
    }

    private void OnEnable()
    {
        // Ascolta l’azione di sparo se disponibile
        if (shootAction != null)
            shootAction.performed += ShootGun;
    }

    private void OnDisable()
    {
        if (shootAction != null)
            shootAction.performed -= ShootGun;
    }

    // --- FUNZIONE DI SPARO ---
    private void ShootGun(InputAction.CallbackContext context)
    {
        if (PauseMenu.isPaused || isReloading) return;
        if (Time.time < nextTimeToFire) return;

        // Se non ci sono munizioni, riproduce il suono di “arma scarica”
        if (currentAmmo <= 0)
        {
            if (gunAudioSource != null && emptyClip != null)
                gunAudioSource.PlayOneShot(emptyClip);

            // Mostra un messaggio “Munizioni esaurite” se anche i caricatori sono finiti
            if (currentAmmo <= 0 && mag <= 0 && ammoOutText != null)
                StartCoroutine(HideAmmoOutMessage());

            return;
        }

        nextTimeToFire = Time.time + 1f / fireCharge; // Imposta la frequenza di fuoco

        currentAmmo--;

        // Effetti visivi e sonori dello sparo
        if (muzzleSpark != null)
            muzzleSpark.Play();

        if (gunAudioSource != null && gunShotClip != null)
            gunAudioSource.PlayOneShot(gunShotClip);

        // Controlla se il colpo ha colpito un oggetto
        RaycastHit hit;
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, Mathf.Infinity))
        {
            ObjectToHit objectToHit = hit.transform.GetComponent<ObjectToHit>();
            if (objectToHit != null)
                objectToHit.ObjectHItDamage(damage);

            ZombieAI zombie = hit.transform.GetComponent<ZombieAI>();
            if (zombie != null)
                zombie.Hit(damage);

            if (impactEffect != null)
            {
                GameObject impactGO = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(impactGO, 1f);
            }
        }
    }

    // Coroutine che gestisce la ricarica dell’arma
    IEnumerator Reload()
    {
        isReloading = true;

        yield return new WaitForSeconds(reloadTime);

        if (mag > 0)
        {
            mag--;
            currentAmmo = maxAmmo;
        }

        isReloading = false;
    }

    // Riduce la salute del giocatore quando riceve danno
    public void TakeDamage(float amount)
    {
        currentHealth -= amount;

        if (healthBar != null)
            healthBar.SetHealth(currentHealth);

        if (damageEffect != null)
            StartCoroutine(DamageEffect());

        if (currentHealth <= 0)
            Die();
    }

    // Gestisce la morte del giocatore
    private void Die()
    {
        enabled = false;

        if (playerInput != null && playerInput.actions != null)
            playerInput.actions.Disable();

        if (pauseMenu == null)
            pauseMenu = FindFirstObjectByType<PauseMenu>();

        if (pauseMenu != null)
            pauseMenu.ShowDieMenu(); // Mostra il menu di morte

        Cursor.lockState = CursorLockMode.None;

        Destroy(gameObject, 0.1f);
    }

    // Effetto visivo temporaneo di danno
    IEnumerator DamageEffect()
    {
        damageEffect.SetActive(true);
        yield return new WaitForSecondsRealtime(0.2f);
        damageEffect.SetActive(false);
    }

    // Aggiunge salute al giocatore (utilizzato dagli oggetti curativi)
    public void AddHealth(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        if (healthBar != null)
            healthBar.SetHealth(currentHealth);
    }

    // Se entra in una zona "KillZone", il giocatore muore istantaneamente
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("KillZone") || other.gameObject.name == "KillZone")
        {
            Die();
        }
    }

    // Controlla se il giocatore può ricevere altra salute
    public bool canTakeHealth()
    {
        return currentHealth < maxHealth;
    }

    // Mostra il messaggio “Munizioni esaurite” 
    IEnumerator HideAmmoOutMessage()
    {
        if (ammoOutText == null) yield break;
        if (ammoOutText.gameObject.activeInHierarchy) yield break;
        if (interactionAmmoBoxMessage != null && interactionAmmoBoxMessage.gameObject.activeInHierarchy) yield break;
        
        ammoOutText.text = "Munizioni Esaurite!";
        ammoOutText.gameObject.SetActive(true);

        yield return new WaitForSeconds(messageDuration);

        ammoOutText.gameObject.SetActive(false);
    }
}
