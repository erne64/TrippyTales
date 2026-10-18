using UnityEngine;
using UnityEngine.UI;

// Questo script cambia dinamicamente il colore del riempimento di uno slider
// in base al suo valore attuale, utilizzando un gradiente di colori.
[RequireComponent(typeof(Slider))]
public class ColorFullSlider : MonoBehaviour
{
    public Image fillImage;        // Riferimento all'immagine che rappresenta il riempimento dello slider
    public Gradient fillGradient;  // Gradiente di colori usato per variare il colore in base al valore dello slider

    private Slider slider;         // Riferimento al componente Slider collegato all'oggetto

    void Awake()
    {
        // Recupera il componente Slider presente sull'oggetto
        slider = GetComponent<Slider>();
    }

    private float lastValue = -1f;

    void Start()
    {
        UpdateColor();
    }

    void Update()
    {
        if (slider != null && !Mathf.Approximately(slider.normalizedValue, lastValue))
        {
            UpdateColor();
        }
    }

    private void UpdateColor()
    {
        if (slider != null && fillImage != null && fillGradient != null)
        {
            lastValue = slider.normalizedValue;
            fillImage.color = fillGradient.Evaluate(lastValue);
        }
    }
}

// Alias per compatibilità con l'eventuale nome alternativo ColorfulSlider
public class ColorfulSlider : ColorFullSlider
{
}

