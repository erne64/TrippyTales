using UnityEngine;
using UnityEngine.AI;
using System.Collections;

// Questo script gestisce l’intelligenza artificiale (AI) degli zombie.
// Gli zombie rilevano il giocatore, lo inseguono, lo attaccano e reagiscono ai colpi subiti.
public class ZombieAI : MonoBehaviour
{
    public int health = 100;                      // Punti vita dello zombie
    public Transform Player;                      // Riferimento al giocatore
    public float detectionRange = 80f;            // Raggio di rilevamento attivo (80m, ottimizza il pathfinding)
    public float attackDistance = 2.2f;           // Distanza di contatto per l'attacco corpo a corpo
    public float attackInterval = 1.6f;           // Tempo tra un colpo e l'altro dello zombie
    public float attackDamage = 15f;              // Danno inflitto al giocatore per attacco (circa 7 colpi per 100 HP)
    public float attackHitTolerance = 0.5f;       // Margine extra sulla distanza quando il colpo va a segno
    public float corpseCleanupDelay = 15f;        // Tempo in secondi prima di distruggere il cadavere dello zombie

    PlayerController Controller;                  // Riferimento al controller del giocatore
    NavMeshAgent Agent;                           // Componente per il movimento automatico 
    Animator Animator;                            // Controlla le animazioni dello zombie
    bool isDead = false;                          // Indica se lo zombie è morto
    bool isAttacking = false;                     // Indica se lo zombie sta attaccando

    private Coroutine hitCoroutine;               // Riferimento per evitare sovrapposizioni di animazioni colpo
    private Coroutine attackCoroutine;            // Riferimento alla coroutine di attacco
    private float nextDestinationUpdateTime = 0f; // Timer per ottimizzare la frequenza di calcolo del NavMesh

    public HealthBarScript healthBarZombie;       // Barra della salute visibile sopra lo zombie

    private void Start()
    {
        // Recupera i componenti necessari
        Agent = GetComponent<NavMeshAgent>();
        Animator = GetComponent<Animator>();
        Controller = PlayerController.instance != null ? PlayerController.instance : FindObjectOfType<PlayerController>();

        // Se non è stato assegnato manualmente, trova il giocatore nella scena
        if (Player == null && Controller != null)
        {
            Player = Controller.transform;
        }
        else if (Player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                Player = playerObj.transform;
            }
        }

        // Evita errori del NavMeshAgent
        if (Agent != null && Agent.isActiveAndEnabled)
        {
            Agent.Warp(transform.position);
        }

        // Imposta animazione iniziale e parametri di salute
        if (Animator != null)
            Animator.SetBool("isWalking", true);

        if (healthBarZombie != null)
            healthBarZombie.SetMaxHealth(health);

        // Imposta la velocità base in base al livello corrente
        if (LevelSystem.instance != null && Agent != null)
        {
            Agent.speed = LevelSystem.instance.baseZombieSpeed;
        }
    }

    private void Update()
    {
        // Se lo zombie è morto o il NavMesh non è disponibile, non fa nulla
        if (isDead || Agent == null || !Agent.isActiveAndEnabled || !Agent.isOnNavMesh) return;

        // Se il giocatore non esiste più (es. è morto ed è stato distrutto), lo zombie si ferma
        if (Player == null)
        {
            if (Agent.isOnNavMesh) Agent.ResetPath();
            if (Animator != null) Animator.SetBool("isWalking", false);
            return;
        }

        // Calcola la distanza dal giocatore
        float Distance = Vector3.Distance(transform.position, Player.position);

        // Se il giocatore è nel raggio di rilevamento, lo insegue
        if (Distance <= detectionRange)
        {
            // Ottimizzazione: aggiorna la destinazione a intervalli regolari (5 volte al secondo anziché 60)
            if (Time.time >= nextDestinationUpdateTime && Agent.isOnNavMesh && !isAttacking)
            {
                nextDestinationUpdateTime = Time.time + 0.2f;
                Agent.SetDestination(Player.position);
            }

            if (Animator != null) Animator.SetBool("isWalking", !isAttacking);

            // Quando è abbastanza vicino, attacca
            if (Distance <= attackDistance && !isAttacking)
            {
                attackCoroutine = StartCoroutine(PlayAttackAnimation());
            }
        }
        else
        {
            // Se il giocatore è troppo lontano, ferma il movimento
            if (Agent.isOnNavMesh)
            {
                Agent.ResetPath();
            }
            if (Animator != null) Animator.SetBool("isWalking", false);
        }
    }

    // Gestisce la ricezione del danno
    public void Hit(int damage)
    {
        if (isDead) return;

        health -= damage;

        if (healthBarZombie != null)
            healthBarZombie.SetHealth(health);

        // Se la salute arriva a zero, muore
        if (health <= 0)
        {
            Die();
        }
        else
        {
            // Ferma eventuali animazioni di colpo precedenti per evitare sovrapposizioni
            if (hitCoroutine != null)
            {
                StopCoroutine(hitCoroutine);
            }
            hitCoroutine = StartCoroutine(PlayHitAnimation());
        }
    }

    // Riproduce l’animazione di "colpito"
    IEnumerator PlayHitAnimation()
    {
        if (Agent != null && Agent.enabled && Agent.isOnNavMesh)
        {
            Agent.isStopped = true;
        }

        if (Animator != null)
        {
            Animator.SetBool("isWalking", false);
            Animator.SetTrigger("Hit");
        }

        yield return new WaitForSeconds(1.5f);

        // Dopo il colpo, riprende a camminare se non è morto o attaccando
        if (!isDead && !isAttacking && Agent != null && Agent.enabled && Agent.isOnNavMesh)
        {
            Agent.isStopped = false;
            if (Animator != null) Animator.SetBool("isWalking", true);
        }

        hitCoroutine = null;
    }

    // Gestisce il comportamento di attacco
    IEnumerator PlayAttackAnimation()
    {
        isAttacking = true;

        if (Agent != null && Agent.enabled && Agent.isOnNavMesh)
        {
            Agent.isStopped = true;
        }

        if (Animator != null) Animator.SetTrigger("Attack");

        yield return new WaitForSeconds(1f); // Attesa prima di infliggere danno

        // Infligge danno al giocatore solo se lo zombie è ancora vivo
        // e il giocatore è ancora a portata
        if (!isDead && Controller != null && Player != null &&
            Vector3.Distance(transform.position, Player.position) <= attackDistance + attackHitTolerance)
        {
            Controller.TakeDamage(attackDamage);
        }

        yield return new WaitForSeconds(attackInterval);

        // Dopo l’attacco, riprende il movimento
        if (!isDead && Agent != null && Agent.enabled && Agent.isOnNavMesh)
        {
            Agent.isStopped = false;
        }
        isAttacking = false;
        attackCoroutine = null;
    }

    // Gestisce la morte dello zombie
    void Die()
    {
        if (isDead) return;

        isDead = true;

        // Interrompe tutte le coroutine attive (attacco o reazione al colpo)
        StopAllCoroutines();
        hitCoroutine = null;
        attackCoroutine = null;

        // Ferma il movimento e resetta le animazioni
        if (Agent != null && Agent.enabled)
        {
            if (Agent.isOnNavMesh) Agent.ResetPath();
            Agent.isStopped = true;
        }

        if (Animator != null)
        {
            Animator.ResetTrigger("Hit");
            Animator.ResetTrigger("Attack");
            Animator.SetBool("isWalking", false);
            Animator.SetTrigger("Dead");
        }

        // Disattiva la collisione su tutti i collider dell'oggetto
        Collider[] allColliders = GetComponentsInChildren<Collider>();
        foreach (var col in allColliders)
        {
            col.enabled = false;
        }

        // Disattiva il NavMeshAgent
        StartCoroutine(DisableAgentAfterDelay());

        if (healthBarZombie != null)
        {
            healthBarZombie.gameObject.SetActive(false);
        }

        // Aggiorna il sistema di livello (zombie ucciso)
        if (LevelSystem.instance != null)
        {
            LevelSystem.instance.EnemyKilled();
        }

        // Rimuove il corpo dopo il tempo configurato per liberare memoria
        Destroy(gameObject, corpseCleanupDelay);
    }

    // Disattiva il NavMeshAgent poco dopo la morte 
    IEnumerator DisableAgentAfterDelay()
    {
        yield return new WaitForSeconds(0.1f);

        if (Agent != null && Agent.enabled)
        {
            Agent.enabled = false;
        }
    }

    // Aggiorna la velocità dello zombie (usato quando si passa di livello)
    public void UpdateSpeed(float newSpeed)
    {
        if (Agent != null && !isDead)
        {
            Agent.speed = newSpeed;
        }
    }
}
