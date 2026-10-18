using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

// Questo script gestisce il passaggio tra la visuale in terza persona e quella di mira.
// Utilizza il sistema Cinemachine e il nuovo Input System per cambiare telecamera e interfaccia.
public class SwitchCam : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;    // Riferimento al componente PlayerInput per leggere i comandi del giocatore
    [SerializeField] private int priority = 10;          // Priorità della camera quando attiva la modalità mira
    [SerializeField] private Canvas thirdPersonCanvas;   // Interfaccia della visuale normale in terza persona
    [SerializeField] private Canvas aimCanvas;           // Interfaccia mostrata durante la mira 

    private CinemachineVirtualCamera virtualCamera;      // Riferimento alla camera gestita da Cinemachine
    private InputAction aimAction;                       // Azione di input associata alla mira
    private bool isAiming = false;                       // Indica se il giocatore sta attualmente mirando

    private void Awake()
    {
        // Recupera il componente della camera virtuale
        virtualCamera = GetComponent<CinemachineVirtualCamera>();

        // Trova automaticamente il PlayerInput se non assegnato
        if (playerInput == null)
            playerInput = FindFirstObjectByType<PlayerInput>();

        // Recupera in modo sicuro l'azione "Aim"
        if (playerInput != null && playerInput.actions != null)
        {
            aimAction = playerInput.actions["Aim"];
        }
    }

    private void OnEnable()
    {
        // Sottoscrive gli eventi di input per attivare e disattivare la mira
        if (aimAction != null)
        {
            aimAction.performed += OnAimPerformed;
            aimAction.canceled += OnAimCanceled;
        }
    }

    private void OnDisable()
    {
        // Rimuove le sottoscrizioni per evitare errori quando lo script è disattivato
        if (aimAction != null)
        {
            aimAction.performed -= OnAimPerformed;
            aimAction.canceled -= OnAimCanceled;
        }

        // Assicura il reset dello stato di mira
        if (isAiming)
        {
            StopAim();
        }
    }

    private void OnAimPerformed(InputAction.CallbackContext context) => StartAim();
    private void OnAimCanceled(InputAction.CallbackContext context) => StopAim();

    // Attiva la modalità mira: aumenta la priorità della camera e aggiorna la UI
    private void StartAim()
    {
        if (virtualCamera == null || isAiming) return;
        isAiming = true;

        virtualCamera.Priority += priority; // Attiva questa camera in Cinemachine

        if (aimCanvas != null)
            aimCanvas.enabled = true;       // Mostra la UI di mira

        if (thirdPersonCanvas != null)
            thirdPersonCanvas.enabled = false; // Nasconde la UI della terza persona
    }

    // Disattiva la modalità mira: ripristina priorità e interfaccia
    private void StopAim()
    {
        if (virtualCamera == null || !isAiming) return;
        isAiming = false;

        virtualCamera.Priority -= priority; // Ritorna alla priorità normale

        if (aimCanvas != null)
            aimCanvas.enabled = false;      // Nasconde la UI di mira

        if (thirdPersonCanvas != null)
            thirdPersonCanvas.enabled = true; // Riattiva la UI in terza persona
    }
}
