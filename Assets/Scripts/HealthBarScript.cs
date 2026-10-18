using UnityEngine;
using UnityEngine.UI;

// Questo script gestisce la barra della salute del giocatore (Health Bar).
// Aggiorna graficamente lo slider e il suo colore in base ai punti vita attuali.
public class HealthBarScript : MonoBehaviour
{
    public Slider slider;        // Riferimento allo slider UI che rappresenta la barra della salute
    public Gradient gradient;    // Gradiente di colori usato per variare il colore in base alla salute 

    private Image fillImage;

    private void Awake()
    {
        if (slider == null)
            slider = GetComponent<Slider>();

        if (slider != null && slider.fillRect != null)
            fillImage = slider.fillRect.GetComponent<Image>();
    }

    // Imposta i valori iniziali della barra della salute (usato quando si avvia o ripristina il giocatore)
    public void SetMaxHealth(float health)
    {
        if (slider == null) return;

        // Imposta i valori massimo e iniziale della barra
        slider.maxValue = health;
        slider.value = health;

        // Imposta il colore iniziale (piena salute)
        if (fillImage != null && gradient != null)
            fillImage.color = gradient.Evaluate(1f);
    }
    
    // Aggiorna graficamente la barra della salute in base ai punti vita attuali
    public void SetHealth(float health)
    {
        if (slider == null) return;
        
        // Aggiorna il valore della barra
        slider.value = health;

        // Aggiorna il colore in base alla percentuale di salute rimanente
        if (fillImage != null && gradient != null)
            fillImage.color = gradient.Evaluate(slider.normalizedValue);
    } 
}
