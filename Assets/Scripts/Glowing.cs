using UnityEngine;
using TMPro;

// Questo script crea un effetto di bagliore pulsante su un testo TMP (TextMeshPro).
// Il bagliore varia nel tempo utilizzando una funzione sinusoidale per creare un effetto "glow" dinamico.
public class Glowing : MonoBehaviour
{
    public TMP_Text text;       // Riferimento al testo TextMeshPro a cui applicare l’effetto
    public float speed = 2f;    // Velocità di variazione del bagliore
    public float intensity = 0.5f; // Intensità dell’effetto di bagliore

    Material mat;               // Materiale condiviso del testo, su cui si applica la modifica dello shader

    void Start()
    {
        if (text == null)
            text = GetComponent<TMP_Text>();

        // Ottiene un'istanza del materiale specifica per questo testo (evita di modificare l'asset su disco)
        if (text != null)
            mat = text.fontMaterial;
    }

    void Update()
    {
        if (mat == null) return;

        // Calcola un valore di bagliore che oscilla nel tempo (effetto pulsante)
        float glow = 0.2f + Mathf.Sin(Time.time * speed) * intensity;

        // Imposta il valore del bagliore nel materiale del testo
        mat.SetFloat(ShaderUtilities.ID_GlowOuter, glow);
    }
}
