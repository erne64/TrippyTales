using UnityEngine;
using UnityEngine.UI;

// Questo script gestisce l'aggiornamento grafico dei testi che mostrano
// il numero di proiettili e caricatori rimanenti nell'interfaccia utente.
public class AmmoCount : MonoBehaviour
{
    public Text ammoText;  // Riferimento al testo UI che mostra i colpi nel caricatore
    [UnityEngine.Serialization.FormerlySerializedAs("magTex")]
    public Text magTex;    // Riferimento al testo UI che mostra il numero di caricatori

    public static AmmoCount occurence;  // Istanza statica per accedere facilmente a questo script da altri oggetti

    private void Awake()
    {
        // Implementazione del pattern Singleton:
        // garantisce che ci sia solo un'istanza di AmmoCount nella scena
        if (occurence == null)
        {
            occurence = this;
        }
        else if (occurence != this)
        {
            Destroy(gameObject); // Elimina eventuali duplicati
        }
    }

    private void OnDestroy()
    {
        if (occurence == this)
        {
            occurence = null;
        }
    }

    // Aggiorna il testo che mostra il numero di proiettili attuali
    public void UpdateAmmo(int currentAmmo)
    {
        // Se non c'è un riferimento al testo, esce
        if (ammoText == null) return;

        // Aggiorna il valore numerico mostrato a schermo
        // (mostra il valore reale, anche 0, così il giocatore capisce quando è scarico)
        ammoText.text = currentAmmo.ToString();
    }
    
    // Aggiorna il testo che mostra il numero di caricatori rimanenti
    public void UpdateMag(int currentMag)
    {
        // Se non c'è un riferimento al testo, esce
        if (magTex == null) return;
        
        // Aggiorna il valore numerico mostrato a schermo
        magTex.text = currentMag.ToString();
    }
}
