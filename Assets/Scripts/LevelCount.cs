using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Questo script gestisce la visualizzazione e l’animazione del numero del livello corrente.
// Mostra il livello sulla UI e riproduce un’animazione quando il giocatore sale di livello.
public class LevelCount : MonoBehaviour
{
    public Text level;  // Riferimento al testo UI che mostra il numero del livello

    public static LevelCount occurence;  // Istanza statica per accedere facilmente allo script

    private Vector3 originalPosition;  // Posizione originale del testo sulla UI
    private Vector3 originalScale;     // Scala originale del testo
    private Color originalColor;       // Colore originale del testo

    private void Start()
    {
        // Se non è stato assegnato un riferimento al testo, interrompe l'esecuzione
        if (level == null) return;

        // Salva la posizione, scala e colore originali del testo per poterli ripristinare dopo l'animazione
        originalPosition = level.rectTransform.anchoredPosition;
        originalScale = level.rectTransform.localScale;
        originalColor = level.color;

        // Recupera il livello corrente dal LevelSystem, se disponibile
        if (LevelSystem.instance != null)
        {
            int currentLevel = LevelSystem.instance.GetCurrentLevel();
            UpdateLevel(currentLevel);
        }
        else
        {
            // Se non esiste un LevelSystem, imposta il livello iniziale a 1
            UpdateLevel(1);
        }
    }

    private void Awake()
    {
        // Pattern Singleton per assicurare una sola istanza di LevelCount nella scena
        if (occurence == null)
        {
            occurence = this;
        }
        else if (occurence != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnDestroy()
    {
        if (occurence == this)
        {
            occurence = null;
        }
    }

    // Aggiorna il numero del livello visualizzato e avvia l’animazione di transizione
    public void UpdateLevel(int currentLevel)
    {
        if (level == null) return;

        // Ripristina le proprietà originali del testo
        level.rectTransform.anchoredPosition = originalPosition;
        level.rectTransform.localScale = originalScale;
        level.color = originalColor;

        // Aggiorna il testo con il numero del livello corrente
        level.text = "Level " + currentLevel.ToString();

        // Ferma eventuali animazioni in corso prima di avviarne una nuova
        StopAllCoroutines();
        
        // Avvia l’animazione solo se l’oggetto è attivo nella gerarchia
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(LevelUpAnimation());
        }
    }

    // Coroutine che gestisce l’animazione del testo quando si sale di livello
    IEnumerator LevelUpAnimation()
    {
        // Definizione di posizioni e scale per l’animazione
        Vector2 centerPos = Vector2.zero;
        Vector3 bigScale = originalScale * 1.5f;
        Vector3 overshootScale = originalScale * 2f;

        float duration = 0.3f;  // Durata di ciascuna fase dell’animazione
        float t = 0f;

        // Colori di transizione: da trasparente a verde acceso
        Color brightColor = Color.green;
        brightColor.a = 1f;
        Color transparent = brightColor;
        transparent.a = 0f;

        // Prima fase: il testo si illumina e si sposta al centro dello schermo
        while (t < duration)
        {
            t += Time.deltaTime;
            float lerp = t / duration;
            level.color = Color.Lerp(transparent, brightColor, lerp);
            level.rectTransform.anchoredPosition = Vector2.Lerp(originalPosition, centerPos, lerp);
            level.rectTransform.localScale = Vector3.Lerp(originalScale, bigScale, lerp);

            yield return null;
        }

        // Mantiene il testo visibile per un breve momento
        yield return new WaitForSeconds(1f);

        // Seconda fase: piccola oscillazione di scala (effetto "rimbalzo")
        t = 0f;
        while (t < duration / 1.5f)
        {
            t += Time.deltaTime;
            float lerp = Mathf.SmoothStep(0, 1, t / (duration / 1.5f));
            level.rectTransform.localScale = Vector3.Lerp(overshootScale, bigScale, lerp);
            yield return null;
        }

        // Terza fase: il testo torna alla posizione e colore originali
        t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float lerp = t / duration;
            level.color = Color.Lerp(brightColor, originalColor, lerp);
            level.rectTransform.anchoredPosition = Vector2.Lerp(centerPos, originalPosition, lerp);
            level.rectTransform.localScale = Vector3.Lerp(bigScale, originalScale, lerp);

            yield return null;
        }

        // Ripristina completamente lo stato originale
        level.rectTransform.anchoredPosition = originalPosition;
        level.rectTransform.localScale = originalScale;
        level.color = originalColor;
    }
}
