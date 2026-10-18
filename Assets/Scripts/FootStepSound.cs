using UnityEngine;

// Questo script gestisce la riproduzione dei suoni dei passi del personaggio.
public class FootStepSound : MonoBehaviour
{
    private AudioSource footstepAudioSource;  // Componente AudioSource utilizzato per riprodurre i suoni dei passi
    public AudioClip[] footstepClips;         // Array di clip audio contenenti i suoni dei passi

    private void Awake()
    {
        // Recupera automaticamente il componente AudioSource presente sull'oggetto
        footstepAudioSource = GetComponent<AudioSource>();
    }

    // Restituisce un clip audio casuale dall’array di suoni disponibili
    private AudioClip GetRandomStep()
    {
        return footstepClips[Random.Range(0, footstepClips.Length)];
    }

    // Riproduce un suono di passo casuale
    private void Step()
    {
        // Controlla che l’array non sia vuoto e l'audioSource esista prima di riprodurre
        if (footstepAudioSource != null && footstepClips != null && footstepClips.Length > 0)
        {
            AudioClip clip = GetRandomStep();                 // Seleziona un clip casuale
            if (clip != null)
                footstepAudioSource.PlayOneShot(clip);        // Lo riproduce una volta
        }
    }
}
